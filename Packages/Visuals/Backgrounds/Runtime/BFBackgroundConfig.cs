using UnityEngine;

namespace BFTools.Visuals.Background
{
    public abstract class BFBackgroundConfig : ScriptableObject
    {
        [SerializeField] private Color baseTop = new Color(0.016f, 0.024f, 0.067f);
        [SerializeField] private Color baseBottom = new Color(0.039f, 0.047f, 0.118f);

        internal Color BaseTop => baseTop;
        internal Color BaseBottom => baseBottom;

        public abstract IBFBackgroundEffect CreateEffect();
    }
}