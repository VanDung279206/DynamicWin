using System;
using System.Windows.Media;

namespace DynamicWin.Core;

/// <summary>
/// Represents a single object orbiting the black hole in 3D space.
/// The orbit is computed using trigonometry + rotation matrix for tilt.
/// </summary>
public class OrbitalObject
{
    // ── Identity ──────────────────────────────────────────────────────────────
    public string Name  { get; set; } = "Object";
    public string Icon  { get; set; } = "◆";   // emoji / unicode glyph as icon
    public Color  Color { get; set; } = Colors.White;

    // ── Orbit parameters ──────────────────────────────────────────────────────
    /// <summary>Radius of the orbit ellipse (pixels in world space).</summary>
    public double OrbitRadius { get; set; } = 120;

    /// <summary>Current angle around the orbit in radians.</summary>
    public double OrbitAngle  { get; set; } = 0;

    /// <summary>Angular velocity in radians per second.</summary>
    public double OrbitSpeed  { get; set; } = 0.8;

    /// <summary>
    /// Tilt of the orbital plane in radians.
    /// 0 = flat XZ plane, Math.PI/2 = vertical (XY plane).
    /// Different tilts per object → different orbital planes → 3D look.
    /// </summary>
    public double OrbitTilt   { get; set; } = 0.4;

    /// <summary>Phase offset so objects don't start at the same position.</summary>
    public double OrbitPhase  { get; set; } = 0;

    // ── Computed 3D position (updated each frame by OrbitalSystem) ────────────
    public double X { get; private set; }
    public double Y { get; private set; }
    public double Z { get; private set; }

    // ── Projected 2D values (filled by PerspectiveProjector) ─────────────────
    public double ScreenX   { get; set; }
    public double ScreenY   { get; set; }
    public double Scale     { get; set; } = 1.0;
    public double Opacity   { get; set; } = 1.0;
    public double ZDepth    { get; set; }   // raw Z for depth-sorting

    // ── Interaction state ─────────────────────────────────────────────────────
    public bool   IsHovered   { get; set; }
    public bool   IsSwallowed { get; set; }

    // ── Swallow animation fields ──────────────────────────────────────────────
    public double SwallowProgress { get; set; } = 0;   // 0..1

    /// <summary>
    /// Advance the orbit angle and recompute 3D position.
    /// 
    /// Math:
    ///   Local orbit (flat XZ plane):
    ///     lx = R * cos(angle + phase)
    ///     lz = R * sin(angle + phase)
    ///     ly = 0
    ///
    ///   Apply tilt rotation around X axis by OrbitTilt:
    ///     x =  lx
    ///     y =  ly * cos(tilt) - lz * sin(tilt)
    ///     z =  ly * sin(tilt) + lz * cos(tilt)
    /// </summary>
    public void Update(double deltaTime)
    {
        OrbitAngle += OrbitSpeed * deltaTime;

        double angle = OrbitAngle + OrbitPhase;
        double lx =  OrbitRadius * Math.Cos(angle);
        double lz =  OrbitRadius * Math.Sin(angle);
        double ly =  0;

        double tilt = OrbitTilt;
        X =  lx;
        Y =  ly * Math.Cos(tilt) - lz * Math.Sin(tilt);
        Z =  ly * Math.Sin(tilt) + lz * Math.Cos(tilt);
    }
}