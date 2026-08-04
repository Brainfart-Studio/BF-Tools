using System.Collections.Generic;
using UnityEngine;

namespace BFTools.Visuals.Parallax
{
    public class BFParallaxStackConfig : ScriptableObject
    {
        [SerializeField] private List<BFParallaxLayerConfig> layers = new List<BFParallaxLayerConfig>();

        internal IReadOnlyList<BFParallaxLayerConfig> Layers => layers;
    }
}