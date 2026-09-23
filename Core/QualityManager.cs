using System;
namespace DynamicWin.Core;
public sealed class QualityManager
{
    public int ParticleCount { get;private set; } = 2500;
    public void Update(double fps)
    {
        if(fps<45) ParticleCount=Math.Max(500,ParticleCount-25);
        else if(fps>90) ParticleCount=Math.Min(10000,ParticleCount+100);
    }
}