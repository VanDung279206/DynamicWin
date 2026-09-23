using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DynamicWin.Core;

namespace DynamicWin;

/// <summary>
/// Custom WPF Canvas that renders the black hole and all orbital objects
/// entirely via DrawingContext (OnRender). No child UIElement per object —
/// this keeps object count low and performance high.
///
/// Rendering order each frame:
///  1. Accretion disk glow (behind everything)
///  2. Orbital objects with ZDepth > 0  (behind black hole)
///  3. Black hole core + corona (always on top of "behind" objects)
///  4. Orbital objects with ZDepth ≤ 0 (in front of black hole)
///  5. Labels for hovered object
/// </summary>
public class OrbitalCanvas : Canvas
{
    // ── References injected from MainWindow ──────────────────────────────────
    public OrbitalSystem? OrbitalSystem { get; set; }
    public Point          BlackHoleCenter { get; set; }
    public ParticleEngine? Particles { get; set; }

    // ── Black hole visual constants ──────────────────────────────────────────
    private const double CoreRadius      = 22;   // px — the pure black circle
    private const double CoronaRadius    = 38;   // px — inner glow ring
    private const double OuterGlowRadius = 70;   // px — soft purple outer glow
    private const double DiskA           = 80;   // px — accretion disk half-width
    private const double DiskB           = 18;   // px — accretion disk half-height

    // ── Interaction state ─────────────────────────────────────────────────────
    public bool IsActive { get; set; }    // true when mouse is near

    // Pulse phase for idle animation
    private double _pulsePhase;

    // ── Pre-built brushes (created once, reused) ─────────────────────────────
    private readonly RadialGradientBrush _coreBrush;
    private readonly RadialGradientBrush _coronaBrush;
    private readonly RadialGradientBrush _outerGlowBrush;
    private readonly Pen                 _diskPen;
    private readonly Pen                 _diskPenFaint;
    private readonly Pen                 _orbitTrailPen;
    private readonly Typeface            _iconTypeface;
    private readonly Typeface            _labelTypeface;

    public OrbitalCanvas()
    {
        // Black hole core: pure black center fading to transparent
        _coreBrush = new RadialGradientBrush();
        _coreBrush.GradientStops.Add(new GradientStop(Color.FromArgb(255, 0,   0,   0),   0.0));
        _coreBrush.GradientStops.Add(new GradientStop(Color.FromArgb(255, 0,   0,   0),   0.65));
        _coreBrush.GradientStops.Add(new GradientStop(Color.FromArgb(0,   0,   0,   0),   1.0));
        _coreBrush.Freeze();

        // Corona glow: deep purple / violet
        _coronaBrush = new RadialGradientBrush();
        _coronaBrush.GradientStops.Add(new GradientStop(Color.FromArgb(0,   60, 0,  120),  0.0));
        _coronaBrush.GradientStops.Add(new GradientStop(Color.FromArgb(180, 90, 0,  180),  0.45));
        _coronaBrush.GradientStops.Add(new GradientStop(Color.FromArgb(100, 60, 0,  120),  0.75));
        _coronaBrush.GradientStops.Add(new GradientStop(Color.FromArgb(0,   30, 0,   60),  1.0));
        _coronaBrush.Freeze();

        // Outer soft glow
        _outerGlowBrush = new RadialGradientBrush();
        _outerGlowBrush.GradientStops.Add(new GradientStop(Color.FromArgb(60,  80, 0,  160), 0.0));
        _outerGlowBrush.GradientStops.Add(new GradientStop(Color.FromArgb(30,  50, 0,  100), 0.6));
        _outerGlowBrush.GradientStops.Add(new GradientStop(Color.FromArgb(0,    0, 0,    0), 1.0));
        _outerGlowBrush.Freeze();

        // Accretion disk: bright inner ring with purple tint
        _diskPen = new Pen(
            new LinearGradientBrush(
                Color.FromArgb(200, 180, 80, 255),
                Color.FromArgb(80,  100, 40, 200),
                new Point(0, 0), new Point(1, 0)),
            2.5)
        { LineJoin = PenLineJoin.Round };
        _diskPen.Freeze();

        // Faint outer accretion disk ring
        _diskPenFaint = new Pen(new SolidColorBrush(Color.FromArgb(40, 140, 60, 220)), 1.2);
        _diskPenFaint.Freeze();

        // Orbit trail (thin, very faint circle per object)
        _orbitTrailPen = new Pen(new SolidColorBrush(Color.FromArgb(15, 180, 100, 255)), 0.6);
        _orbitTrailPen.Freeze();

        _iconTypeface  = new Typeface("Segoe UI Emoji");
        _labelTypeface = new Typeface(
            new FontFamily("Segoe UI"),
            FontStyles.Normal, FontWeights.Light, FontStretches.Normal);
    }

    // ── Public API: advance pulse for idle animation ───────────────────────────
    public void AdvancePulse(double deltaTime) => _pulsePhase += deltaTime * 1.8;

    // ── Core rendering ────────────────────────────────────────────────────────
    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        if (OrbitalSystem is null) return;

        Point bh = BlackHoleCenter;

        Particles?.Draw(dc, bh);

        // ── 1. Outer glow ────────────────────────────────────────────────────
        double pulseScale = 1.0 + 0.06 * Math.Sin(_pulsePhase);
        double activeBoost = IsActive ? 1.3 : 1.0;
        double outerR = OuterGlowRadius * pulseScale * activeBoost;
        DrawRadialBrush(dc, _outerGlowBrush, bh, outerR);

        // ── 2. Accretion disk (always rendered, behind corona) ────────────────
        DrawAccretionDisk(dc, bh, activeBoost);

        // ── 3. Objects BEHIND black hole (ZDepth > 0) ────────────────────────
        foreach (var obj in OrbitalSystem.GetDrawOrder())
        {
            if (obj.ZDepth > 0)
                DrawOrbitalObject(dc, obj);
        }

        // ── 4. Black hole corona + core ──────────────────────────────────────
        double coronaR = CoronaRadius * pulseScale * activeBoost;
        DrawRadialBrush(dc, _coronaBrush, bh, coronaR);

        double coreR = CoreRadius * (IsActive ? 1.08 : 1.0);
        dc.DrawEllipse(Brushes.Black, null, bh, coreR, coreR);

        // ── 5. Objects IN FRONT of black hole (ZDepth ≤ 0) ──────────────────
        foreach (var obj in OrbitalSystem.GetDrawOrder())
        {
            if (obj.ZDepth <= 0)
                DrawOrbitalObject(dc, obj);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Helper: draw accretion disk as a rotated ellipse
    // ─────────────────────────────────────────────────────────────────────────
    private void DrawAccretionDisk(DrawingContext dc, Point center, double boost)
    {
        // We push a transform to rotate the disk 15° for visual interest
        dc.PushTransform(new RotateTransform(-15, center.X, center.Y));

        double a = DiskA * boost;
        double b = DiskB * boost;

        dc.DrawEllipse(null, _diskPen,      center, a,       b);
        dc.DrawEllipse(null, _diskPenFaint, center, a * 1.3, b * 1.15);

        dc.Pop(); // rotation
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Helper: draw one orbital object at its projected screen position
    // ─────────────────────────────────────────────────────────────────────────
    private void DrawOrbitalObject(DrawingContext dc, OrbitalObject obj)
    {
        double scale   = obj.Scale * (obj.IsHovered ? 1.45 : 1.0);
        double opacity = obj.Opacity * (obj.IsHovered ? 1.0 : 1.0);
        opacity = Math.Clamp(opacity, 0, 1);

        var pos = new Point(obj.ScreenX, obj.ScreenY);

        // Icon size
        double iconSize = PerspectiveProjector.BaseIconSize * scale;

        // Push opacity
        dc.PushOpacity(opacity);

        // ── Glow circle behind icon ──────────────────────────────────────────
        byte glowAlpha = (byte)(obj.IsHovered ? 120 : 60);
        var glowBrush = new RadialGradientBrush(
            Color.FromArgb(glowAlpha, obj.Color.R, obj.Color.G, obj.Color.B),
            Color.FromArgb(0,         obj.Color.R, obj.Color.G, obj.Color.B));
        glowBrush.Freeze();
        dc.DrawEllipse(glowBrush, null, pos, iconSize * 1.1, iconSize * 1.1);

        // ── Icon background pill ─────────────────────────────────────────────
        var bgBrush = new SolidColorBrush(
            Color.FromArgb(obj.IsHovered ? (byte)200 : (byte)140,
                           (byte)(obj.Color.R * 0.15),
                           (byte)(obj.Color.G * 0.15),
                           (byte)(obj.Color.B * 0.15)));
        var borderPen = new Pen(
            new SolidColorBrush(Color.FromArgb(160, obj.Color.R, obj.Color.G, obj.Color.B)),
            0.8 * scale);
        bgBrush.Freeze();
        borderPen.Freeze();

        double r = iconSize * 0.72;
        dc.DrawEllipse(bgBrush, borderPen, pos, r, r);

        // ── Emoji icon ───────────────────────────────────────────────────────
        double fontSize = iconSize * 0.88;
        var ft = new FormattedText(
            obj.Icon,
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            _iconTypeface,
            fontSize,
            Brushes.White,
            VisualTreeHelper.GetDpi(this).PixelsPerDip);

        dc.DrawText(ft, new Point(pos.X - ft.Width / 2, pos.Y - ft.Height / 2));

        // ── Hover label ───────────────────────────────────────────────────────
        if (obj.IsHovered)
        {
            var labelBrush = new SolidColorBrush(
                Color.FromArgb(220, obj.Color.R, obj.Color.G, obj.Color.B));
            labelBrush.Freeze();

            var label = new FormattedText(
                obj.Name,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                _labelTypeface,
                10.5,
                labelBrush,
                VisualTreeHelper.GetDpi(this).PixelsPerDip);

            dc.DrawText(label, new Point(pos.X - label.Width / 2, pos.Y + r + 3));
        }

        dc.Pop(); // opacity
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Helper: draw a circular RadialGradientBrush as an ellipse
    // ─────────────────────────────────────────────────────────────────────────
    private static void DrawRadialBrush(DrawingContext dc, RadialGradientBrush brush, Point center, double radius)
    {
        dc.DrawEllipse(brush, null, center, radius, radius);
    }

    /// <summary>
    /// Hit test: returns which orbital object (if any) is under screen point p.
    /// </summary>
    public OrbitalObject? HitTest(Point p)
    {
        if (OrbitalSystem is null) return null;

        OrbitalObject? closest = null;
        double minDist = double.MaxValue;
        const double hitRadius = 26;

        foreach (var obj in OrbitalSystem.Objects)
        {
            double dx = p.X - obj.ScreenX;
            double dy = p.Y - obj.ScreenY;
            double dist = Math.Sqrt(dx * dx + dy * dy);

            double scaledHit = hitRadius * obj.Scale;
            if (dist < scaledHit && dist < minDist)
            {
                minDist = dist;
                closest = obj;
            }
        }

        return closest;
    }
}