using BFTools.Core.LayerStack;
using UnityEngine;

namespace BFTools.Visuals.Parallax
{
    public abstract class BFParallaxLayerConfig : ScriptableObject, IBFLayerConfig<IBFParallaxLayer>
    {
        public abstract IBFParallaxLayer CreateLayer();
    }
}