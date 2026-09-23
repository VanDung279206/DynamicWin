using System;
using System.Windows.Media;
namespace DynamicWin.Core;
/// <summary>
/// Drives the animation loop using CompositionTarget.Rendering,
/// which fires once per display frame (typically 60 Hz).
///
/// Calculates accurate deltaTime between frames so orbit speeds
/// are frame-rate independent (seconds-based, not tick-based).
/// </summary>
public class AnimationManager
{
    private TimeSpan _lastRenderTime = TimeSpan.Zero;
    private bool     _isRunning;
    /// <summary>Raised every frame with elapsed seconds since last frame.</summary>
    public event Action<double>? Tick;
    public void Start()
    {
        if (_isRunning) return;
        _isRunning = true;
        CompositionTarget.Rendering += OnRendering;
    }
    public void Stop()
    {
        if (!_isRunning) return;
        _isRunning = false;
        CompositionTarget.Rendering -= OnRendering;
        _lastRenderTime = TimeSpan.Zero;
    }
    private void OnRendering(object? sender, EventArgs e)
    {
        var args = (RenderingEventArgs)e;
        TimeSpan current = args.RenderingTime;
        if (_lastRenderTime == TimeSpan.Zero)
        {
            _lastRenderTime = current;
            return;
        }
        double deltaTime = (current - _lastRenderTime).TotalSeconds;
        _lastRenderTime  = current;
        // Clamp delta to avoid huge jumps after window minimise / sleep
        deltaTime = Math.Min(deltaTime, 0.1);
        Tick?.Invoke(deltaTime);
    }
}