using BFTools.Core.LayerStack;
using UnityEngine;

namespace BFTools.Visuals.Parallax
{
    public class BFParallaxStack : BFLayerStackBase<BFParallaxLayerConfig, IBFParallaxLayer>
    {
        private const string LogTag = "Parallax";

        public BFParallaxStack(BFParallaxStackConfig config)
            : base(LogTag, nameof(BFParallaxStack), config.Layers, config)
        {
        }

        public void Tick(Vector2 cameraDisplacement, float dt)
        {
            for (int i = 0; i < layers.Count; i++)
                layers[i].Tick(cameraDisplacement, dt);
        }
    }
}