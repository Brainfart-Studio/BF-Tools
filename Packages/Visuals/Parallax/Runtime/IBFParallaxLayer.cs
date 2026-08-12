using BFTools.Core.LayerStack;
using UnityEngine;

namespace BFTools.Visuals.Parallax
{
    public interface IBFParallaxLayer : IBFStackLayer
    {
        void Tick(Vector2 cameraDisplacement, float dt);
    }
}