using System.Collections.Generic;
using UnityEngine;

namespace BFTools.Visuals.Background
{
    public class BFBackgroundStackConfig : ScriptableObject
    {
        [SerializeField] private List<BFBackgroundLayerConfig> layers = new List<BFBackgroundLayerConfig>();

        internal IReadOnlyList<BFBackgroundLayerConfig> Layers => layers;
    }
}