using System;
using System.Windows;
using System.Windows.Input;
using DynamicWin.Core;

namespace DynamicWin;

/// <summary>
/// MainWindow code-behind.
///
/// Responsibilities:
///  1. Position the window top-center of the primary screen.
///  2. Wire up AnimationManager → OrbitalSystem → OrbitalCanvas each frame.
///  3. Handle mouse proximity to toggle active/idle states.
///  4. Handle hover detection on orbital objects.
///  5. Allow dragging the window (click+drag on empty canvas area).
/// </summary>
public partial class MainWindow : Window
{
    // ── Core systems ──────────────────────────────────────────────────────────
    private readonly AnimationManager _animationManager = new();
    private readonly OrbitalSystem    _orbitalSystem    = new();

    // ── Constants ──────────────────────────────────────────────────────────────
    private const double ActivationRadius = 180; // px from BH center to activate

    // ── State ─────────────────────────────────────────────────────────────────
    private OrbitalObject? _hoveredObject;

    public MainWindow()
    {
        InitializeComponent();

        // Position window top-center after layout is complete
        Loaded += OnLoaded;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Startup
    // ─────────────────────────────────────────────────────────────────────────
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        PositionWindowTopCenter();

        // Black hole center = canvas center horizontally, ~90px from top
        double bhX = OrbitalCanvas.Width  / 2;
        double bhY = 90;
        OrbitalCanvas.BlackHoleCenter = new Point(bhX, bhY);
        OrbitalCanvas.OrbitalSystem   = _orbitalSystem;

        // Start animation loop
        _animationManager.Tick += OnAnimationTick;
        _animationManager.Start();
    }

    private void PositionWindowTopCenter()
    {
        var screen = SystemParameters.WorkArea;
        Left = screen.Left + (screen.Width  - Width)  / 2;
        Top  = screen.Top;   // flush to top edge
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Animation loop
    // ─────────────────────────────────────────────────────────────────────────
    private void OnAnimationTick(double deltaTime)
    {
        Point bhCenter = OrbitalCanvas.BlackHoleCenter;

        _orbitalSystem.Update(deltaTime, bhCenter.X, bhCenter.Y);
        OrbitalCanvas.AdvancePulse(deltaTime);
        OrbitalCanvas.InvalidateVisual();   // triggers OnRender
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Mouse interaction
    // ─────────────────────────────────────────────────────────────────────────
    private void RootCanvas_MouseMove(object sender, MouseEventArgs e)
    {
        Point mousePos = e.GetPosition(OrbitalCanvas);
        Point bh       = OrbitalCanvas.BlackHoleCenter;

        double dx   = mousePos.X - bh.X;
        double dy   = mousePos.Y - bh.Y;
        double dist = Math.Sqrt(dx * dx + dy * dy);

        bool isActive = dist < ActivationRadius;

        // Smoothly transition expansion / speed
        double targetExpansion  = isActive ? 1.30 : 1.00;
        double targetSpeed      = isActive ? 1.50 : 1.00;

        _orbitalSystem.ExpansionFactor = Lerp(_orbitalSystem.ExpansionFactor, targetExpansion, 0.06);
        _orbitalSystem.SpeedMultiplier = Lerp(_orbitalSystem.SpeedMultiplier, targetSpeed,     0.05);

        OrbitalCanvas.IsActive = isActive;

        // Hover detection
        var hitObj = OrbitalCanvas.HitTest(mousePos);

        if (hitObj != _hoveredObject)
        {
            if (_hoveredObject is not null) _hoveredObject.IsHovered = false;
            _hoveredObject = hitObj;
            if (_hoveredObject is not null) _hoveredObject.IsHovered = true;
        }

        // Allow window drag on empty canvas
        if (e.LeftButton == MouseButtonState.Pressed && _hoveredObject is null)
            DragMove();
    }

    private void RootCanvas_MouseLeave(object sender, MouseEventArgs e)
    {
        _orbitalSystem.ExpansionFactor = 1.0;
        _orbitalSystem.SpeedMultiplier = 1.0;
        OrbitalCanvas.IsActive = false;

        if (_hoveredObject is not null)
        {
            _hoveredObject.IsHovered = false;
            _hoveredObject = null;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Utilities
    // ─────────────────────────────────────────────────────────────────────────
    private static double Lerp(double a, double b, double t) => a + (b - a) * t;

    protected override void OnClosed(EventArgs e)
    {
        _animationManager.Stop();
        base.OnClosed(e);
    }
}