using BFTools.Core.LayerStack;
using UnityEngine;

namespace BFTools.Visuals.Background
{
    public abstract class BFBackgroundLayerConfig : ScriptableObject, IBFLayerConfig<IBFBackgroundLayer>
    {
        public abstract IBFBackgroundLayer CreateLayer();
    }
}