using UnityEngine;

namespace BFTools.Visuals.Background
{
    public abstract class BFBackgroundLayerConfig : ScriptableObject
    {
        public abstract IBFBackgroundLayer CreateLayer();
    }
}