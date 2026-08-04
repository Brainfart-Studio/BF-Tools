using UnityEngine;

namespace BFTools.Visuals.Parallax
{
    public abstract class BFParallaxLayerConfig : ScriptableObject
    {
        public abstract IBFParallaxLayer CreateLayer();
    }
}