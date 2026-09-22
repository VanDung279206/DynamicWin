using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace DynamicWin.Core;

/// <summary>
/// Manages the collection of OrbitalObjects.
/// Responsible for:
///  1. Seeding the 5 default orbital objects with varied parameters.
///  2. Updating all objects each frame (advancing orbit angles).
///  3. Running perspective projection to fill ScreenX/Y/Scale/Opacity.
///  4. Providing Z-sorted list for proper depth ordering when drawing.
/// </summary>
public class OrbitalSystem
{
    public List<OrbitalObject> Objects { get; } = new();

    // Expansion factor when user hovers (1.0 = idle, 1.35 = active)
    public double ExpansionFactor { get; set; } = 1.0;

    // Speed multiplier (increased on hover)
    public double SpeedMultiplier { get; set; } = 1.0;

    public OrbitalSystem()
    {
        SeedDefaultObjects();
    }

    /// <summary>
    /// Creates 5 orbital objects with different radii, speeds, tilts, and phases
    /// so they occupy genuinely different orbital planes and feel 3D.
    /// </summary>
    private void SeedDefaultObjects()
    {
        // ─────────────────────────────────────────────────────────────────────
        // Object 1 – ChatGPT
        //   Outer orbit, moderately tilted, slow
        // ─────────────────────────────────────────────────────────────────────
        Objects.Add(new OrbitalObject
        {
            Name        = "ChatGPT",
            Icon        = "🤖",
            Color       = Color.FromRgb(0x10, 0xA3, 0x7F),  // OpenAI green
            OrbitRadius = 130,
            OrbitSpeed  = 0.55,
            OrbitTilt   = Math.PI * 0.18,    // ~32°
            OrbitPhase  = 0,
        });

        // ─────────────────────────────────────────────────────────────────────
        // Object 2 – Browser
        //   Medium orbit, steep tilt (almost vertical), faster
        // ─────────────────────────────────────────────────────────────────────
        Objects.Add(new OrbitalObject
        {
            Name        = "Browser",
            Icon        = "🌐",
            Color       = Color.FromRgb(0x42, 0x85, 0xF4),  // Google blue
            OrbitRadius = 110,
            OrbitSpeed  = 0.80,
            OrbitTilt   = Math.PI * 0.42,    // ~76°
            OrbitPhase  = Math.PI * 0.6,
        });

        // ─────────────────────────────────────────────────────────────────────
        // Object 3 – Calculator
        //   Inner orbit, nearly flat, quick spin
        // ─────────────────────────────────────────────────────────────────────
        Objects.Add(new OrbitalObject
        {
            Name        = "Calculator",
            Icon        = "🧮",
            Color       = Color.FromRgb(0xFF, 0x95, 0x00),  // orange
            OrbitRadius = 90,
            OrbitSpeed  = 1.10,
            OrbitTilt   = Math.PI * 0.08,    // ~14°
            OrbitPhase  = Math.PI * 1.2,
        });

        // ─────────────────────────────────────────────────────────────────────
        // Object 4 – Timer
        //   Medium-outer, perpendicular tilt to Browser
        // ─────────────────────────────────────────────────────────────────────
        Objects.Add(new OrbitalObject
        {
            Name        = "Timer",
            Icon        = "⏱",
            Color       = Color.FromRgb(0xE0, 0x40, 0xFB),  // purple
            OrbitRadius = 120,
            OrbitSpeed  = 0.70,
            OrbitTilt   = Math.PI * 0.30,    // ~54°
            OrbitPhase  = Math.PI * 1.8,
        });

        // ─────────────────────────────────────────────────────────────────────
        // Object 5 – Media
        //   Outer, retrograde orbit (negative speed), high tilt
        // ─────────────────────────────────────────────────────────────────────
        Objects.Add(new OrbitalObject
        {
            Name        = "Media",
            Icon        = "🎵",
            Color       = Color.FromRgb(0xFF, 0x40, 0x81),  // pink
            OrbitRadius = 140,
            OrbitSpeed  = -0.50,             // retrograde!
            OrbitTilt   = Math.PI * 0.55,    // ~99°
            OrbitPhase  = Math.PI * 0.9,
        });
    }

    /// <summary>
    /// Called every frame. Updates orbital angles and projects to screen space.
    /// </summary>
    /// <param name="deltaTime">Elapsed seconds since last frame.</param>
    /// <param name="canvasCenterX">Black hole center X on canvas.</param>
    /// <param name="canvasCenterY">Black hole center Y on canvas.</param>
    public void Update(double deltaTime, double canvasCenterX, double canvasCenterY)
    {
        double dt = deltaTime * SpeedMultiplier;

        foreach (var obj in Objects)
        {
            if (obj.IsSwallowed) continue;

            // Scale orbit radius with expansion factor
            double savedRadius = obj.OrbitRadius;
            obj.OrbitRadius *= ExpansionFactor;

            obj.Update(dt);

            // Restore so expansion doesn't compound each frame
            obj.OrbitRadius = savedRadius;

            // Project 3D → 2D
            var proj = PerspectiveProjector.Project(
                obj.X, obj.Y, obj.Z,
                canvasCenterX, canvasCenterY);

            obj.ScreenX  = proj.ScreenX;
            obj.ScreenY  = proj.ScreenY;
            obj.Scale    = proj.Scale;
            obj.Opacity  = proj.Opacity;
            obj.ZDepth   = proj.ZDepth;
        }
    }

    /// <summary>
    /// Returns all non-swallowed objects sorted by ZDepth descending
    /// (farthest first → painted first → appears behind closer objects).
    /// </summary>
    public IEnumerable<OrbitalObject> GetDrawOrder()
        => Objects
            .Where(o => !o.IsSwallowed)
            .OrderByDescending(o => o.ZDepth);
}