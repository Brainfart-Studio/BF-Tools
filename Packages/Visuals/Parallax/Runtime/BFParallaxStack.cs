using System.Collections.Generic;
using BFTools.Core.Logger;
using UnityEngine;

namespace BFTools.Visuals.Parallax
{
    public class BFParallaxStack
    {
        private const string LogTag = "Parallax";

        private readonly BFParallaxStackConfig config;
        private readonly List<IBFParallaxLayer> layers = new List<IBFParallaxLayer>();

        public int LayerCount => layers.Count;

        public BFParallaxStack(BFParallaxStackConfig config)
        {
            this.config = config;
        }

        public void Init(Transform parent, int startingSortingOrder)
        {
            layers.Clear();

            IReadOnlyList<BFParallaxLayerConfig> layerConfigs = config.Layers;
            int sortingOrder = startingSortingOrder;
            for (int i = 0; i < layerConfigs.Count; i++)
            {
                BFParallaxLayerConfig layerConfig = layerConfigs[i];
                if (layerConfig == null)
                {
                    BFLogger.Warning(LogTag, "BFParallaxStack: skipping empty layer slot in stack config.", config);
                    continue;
                }

                IBFParallaxLayer layer = layerConfig.CreateLayer();
                layer.Init(parent, sortingOrder);
                layers.Add(layer);
                sortingOrder++;
            }

            BFLogger.Info(LogTag, $"BFParallaxStack: initialized with {layers.Count} layer(s).", config);
        }

        public void Tick(Vector2 cameraDisplacement, float dt)
        {
            for (int i = 0; i < layers.Count; i++)
                layers[i].Tick(cameraDisplacement, dt);
        }

        public void Cleanup()
        {
            BFLogger.Info(LogTag, $"BFParallaxStack: cleaning up {layers.Count} layer(s).", config);

            for (int i = 0; i < layers.Count; i++)
                layers[i].Cleanup();

            layers.Clear();
        }
    }
}