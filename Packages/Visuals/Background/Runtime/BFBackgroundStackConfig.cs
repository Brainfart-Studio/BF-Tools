using System.Collections.Generic;
using UnityEngine;

namespace BFTools.Visuals.Background
{
    [CreateAssetMenu(fileName = "BFBackgroundStackConfig", menuName = "BFTools/Visuals/Background/Background Stack Config")]
    public class BFBackgroundStackConfig : ScriptableObject
    {
        [SerializeField] private List<BFBackgroundLayerConfig> layers = new List<BFBackgroundLayerConfig>();

        internal IReadOnlyList<BFBackgroundLayerConfig> Layers => layers;
    }
}