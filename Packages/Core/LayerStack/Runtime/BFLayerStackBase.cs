using System.Collections.Generic;
using BFTools.Core.Logger;
using UnityEngine;

namespace BFTools.Core.LayerStack
{
    public abstract class BFLayerStackBase<TLayerConfig, TLayer>
        where TLayerConfig : ScriptableObject, IBFLayerConfig<TLayer>
        where TLayer : IBFStackLayer
    {
        private readonly string logTag;
        private readonly string typeName;
        private readonly Object context;
        private readonly IReadOnlyList<TLayerConfig> layerConfigs;

        protected readonly List<TLayer> layers = new List<TLayer>();

        public int LayerCount => layers.Count;

        protected BFLayerStackBase(string logTag, string typeName, IReadOnlyList<TLayerConfig> layerConfigs, Object context)
        {
            this.logTag = logTag;
            this.typeName = typeName;
            this.layerConfigs = layerConfigs;
            this.context = context;
        }

        public void Init(Transform parent, int startingSortingOrder)
        {
            layers.Clear();

            int sortingOrder = startingSortingOrder;
            for (int i = 0; i < layerConfigs.Count; i++)
            {
                TLayerConfig layerConfig = layerConfigs[i];
                if (layerConfig == null)
                {
                    BFLogger.Warning(logTag, $"{typeName}: skipping empty layer slot in stack config.", context);
                    continue;
                }

                TLayer layer = layerConfig.CreateLayer();
                layer.Init(parent, sortingOrder);
                layers.Add(layer);
                sortingOrder++;
            }

            BFLogger.Info(logTag, $"{typeName}: initialized with {layers.Count} layer(s).", context);
        }

        public void Cleanup()
        {
            BFLogger.Info(logTag, $"{typeName}: cleaning up {layers.Count} layer(s).", context);

            for (int i = 0; i < layers.Count; i++)
                layers[i].Cleanup();

            layers.Clear();
        }
    }
}