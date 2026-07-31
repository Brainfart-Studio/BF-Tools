using UnityEngine;

namespace BFTools.Visuals.Background
{
    public abstract class BFBackgroundConfig : ScriptableObject
    {
        public abstract IBFBackgroundEffect CreateEffect();
    }
}