using UnityEngine;

namespace BFTools.Visuals.BackgroundStacks
{
    public abstract class BFBackgroundLayerConfig : ScriptableObject
    {
        public abstract IBFBackgroundLayer CreateLayer();
    }
}