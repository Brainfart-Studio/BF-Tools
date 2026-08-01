using UnityEngine;

namespace BFTools.Visuals.Background
{
    public class BFGradientLayerConfig : BFBackgroundLayerConfig
    {
        [SerializeField] private Color topColor = new Color(0.016f, 0.024f, 0.067f);
        [SerializeField] private Color bottomColor = new Color(0.039f, 0.047f, 0.118f);

        internal Color TopColor => topColor;
        internal Color BottomColor => bottomColor;

        public override IBFBackgroundLayer CreateLayer()
        {
            return new BFGradientLayer(this);
        }
    }
}