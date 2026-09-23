using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace DynamicWin.Core;

public sealed class ParticleEngine
{
    private readonly List<Particle> _particles = new();
    private readonly Random _random = new();
    public IReadOnlyList<Particle> Particles => _particles;

    public ParticleEngine(int count = 2500)
    {
        for(int i=0;i<count;i++) _particles.Add(Create());
    }

    private Particle Create()
    {
        double a=_random.NextDouble()*Math.PI*2;
        double r=45+_random.NextDouble()*240;
        return new Particle{
            Angle=a, Radius=r,
            Speed=.2+_random.NextDouble()*1.2,
            Size=.4+_random.NextDouble()*2.2,
            Alpha=(byte)(50+_random.Next(150))};
    }

    public void Update(double dt)
    {
        foreach(var p in _particles)
        {
            p.Angle += dt*p.Speed;
            p.Radius -= dt*2;
            if(p.Radius<35) { p.Radius=220; }
        }
    }

    public void Draw(DrawingContext dc, Point center)
    {
        foreach(var p in _particles)
        {
            double x=center.X+Math.Cos(p.Angle)*p.Radius;
            double y=center.Y+Math.Sin(p.Angle)*p.Radius*.28;
            var b=new SolidColorBrush(Color.FromArgb(p.Alpha,220,180,255));
            b.Freeze();
            dc.DrawEllipse(b,null,new Point(x,y),p.Size,p.Size);
        }
    }
}

public sealed class Particle
{
    public double Angle;
    public double Radius;
    public double Speed;
    public double Size;
    public byte Alpha;
}