using System;

namespace DynamicWin.Core;

/// <summary>
/// Lightweight perspective projection:
///   Converts 3D world coordinates (X, Y, Z) into 2D screen coordinates,
///   plus a scale factor and opacity value based on depth (Z).
///
/// Coordinate convention:
///   - Center of black hole = (0, 0, 0) in world space
///   - Camera is at (0, 0, -CameraDistance) looking toward +Z
///   - Positive Z → object is BEHIND the black hole (far, small, dim)
///   - Negative Z → object is IN FRONT (close, large, bright)
///
/// Projection formula:
///   perspectiveFactor = FocalLength / (FocalLength + Z + CameraDistance)
///   screenX = centerX + X * perspectiveFactor
///   screenY = centerY + Y * perspectiveFactor
///   scale   = perspectiveFactor  (clamped to [MinScale, MaxScale])
///   opacity = mapped from perspectiveFactor
/// </summary>
public static class PerspectiveProjector
{
    /// <summary>Distance of virtual camera from the scene center.</summary>
    public const double CameraDistance = 300;

    /// <summary>Focal length — controls how aggressive the perspective is.</summary>
    public const double FocalLength = 400;

    /// <summary>Object icon base size before scaling.</summary>
    public const double BaseIconSize = 28;

    public const double MinScale  = 0.35;
    public const double MaxScale  = 1.40;
    public const double MinOpacity = 0.18;
    public const double MaxOpacity = 1.00;

    /// <summary>
    /// Projects a 3D position to screen space relative to a canvas center point.
    /// </summary>
    /// <param name="worldX">World X (horizontal)</param>
    /// <param name="worldY">World Y (vertical, Y+ = down in screen)</param>
    /// <param name="worldZ">World Z (depth, Z+ = behind black hole)</param>
    /// <param name="centerX">Screen X of black hole center</param>
    /// <param name="centerY">Screen Y of black hole center</param>
    /// <returns>Projected result with screen position, scale, opacity, and zDepth.</returns>
    public static ProjectionResult Project(
        double worldX, double worldY, double worldZ,
        double centerX, double centerY)
    {
        // Perspective factor: larger when object is closer (Z is more negative)
        double depth  = FocalLength + worldZ + CameraDistance;
        double factor = depth > 1 ? FocalLength / depth : FocalLength;

        double screenX = centerX + worldX * factor;
        double screenY = centerY + worldY * factor;

        // Remap factor to scale and opacity
        // factor range approximately: FocalLength/(FocalLength+R+Camera) .. FocalLength/(FocalLength-R+Camera)
        double scale = Math.Clamp(factor, MinScale, MaxScale);

        // Normalize factor for opacity (0=back, 1=front)
        double normalizedFactor = (factor - MinScale) / (MaxScale - MinScale);
        double opacity = Math.Clamp(
            MinOpacity + normalizedFactor * (MaxOpacity - MinOpacity),
            MinOpacity, MaxOpacity);

        return new ProjectionResult
        {
            ScreenX  = screenX,
            ScreenY  = screenY,
            Scale    = scale,
            Opacity  = opacity,
            ZDepth   = worldZ,
            IsBehind = worldZ > 0   // positive Z = behind black hole center
        };
    }
}

public struct ProjectionResult
{
    public double ScreenX;
    public double ScreenY;
    public double Scale;
    public double Opacity;
    public double ZDepth;
    public bool   IsBehind;
}