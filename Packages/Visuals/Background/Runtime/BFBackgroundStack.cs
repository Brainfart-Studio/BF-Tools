using System.Collections.Generic;
using UnityEngine;

namespace BFTools.Visuals.Background
{
    public class BFBackgroundStack
    {
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
                    Debug.LogWarning("BFBackgroundStack: skipping empty layer slot in stack config.", config);
                    continue;
                }

                IBFBackgroundLayer layer = layerConfig.CreateLayer();
                layer.Init(parent, sortingOrder);
                layers.Add(layer);
                sortingOrder++;
            }
        }

        public void Tick(float dt)
        {
            for (int i = 0; i < layers.Count; i++)
                layers[i].Tick(dt);
        }

        public void Cleanup()
        {
            for (int i = 0; i < layers.Count; i++)
                layers[i].Cleanup();

            layers.Clear();
        }
    }
}