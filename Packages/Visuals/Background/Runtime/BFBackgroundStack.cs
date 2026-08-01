using System.Collections.Generic;
using BFTools.Core.Logger;
using UnityEngine;

namespace BFTools.Visuals.Background
{
    public class BFBackgroundStack
    {
        private const string LogTag = "Background";

        private readonly BFBackgroundStackConfig config;
        private readonly List<IBFBackgroundLayer> layers = new List<IBFBackgroundLayer>();

        public int LayerCount => layers.Count;

        public BFBackgroundStack(BFBackgroundStackConfig config)
        {
            this.config = config;
        }

        public void Init(Transform parent, int startingSortingOrder)
        {
            layers.Clear();

            IReadOnlyList<BFBackgroundLayerConfig> layerConfigs = config.Layers;
            int sortingOrder = startingSortingOrder;
            for (int i = 0; i < layerConfigs.Count; i++)
            {
                BFBackgroundLayerConfig layerConfig = layerConfigs[i];
                if (layerConfig == null)
                {
                    BFLogger.Warning(LogTag, "BFBackgroundStack: skipping empty layer slot in stack config.", config);
                    continue;
                }

                IBFBackgroundLayer layer = layerConfig.CreateLayer();
                layer.Init(parent, sortingOrder);
                layers.Add(layer);
                sortingOrder++;
            }

            BFLogger.Info(LogTag, $"BFBackgroundStack: initialized with {layers.Count} layer(s).", config);
        }

        public void Tick(float dt)
        {
            for (int i = 0; i < layers.Count; i++)
                layers[i].Tick(dt);
        }

        public void Cleanup()
        {
            BFLogger.Info(LogTag, $"BFBackgroundStack: cleaning up {layers.Count} layer(s).", config);

            for (int i = 0; i < layers.Count; i++)
                layers[i].Cleanup();

            layers.Clear();
        }
    }
}