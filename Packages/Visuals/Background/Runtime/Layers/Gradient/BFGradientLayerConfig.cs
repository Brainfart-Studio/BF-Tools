using UnityEngine;

namespace BFTools.Visuals.Background
{
    public class BFGradientLayerConfig : BFBackgroundLayerConfig
    {
        [SerializeField] private Gradient colorGradient = CreateDefaultGradient();

        internal Gradient ColorGradient => colorGradient;

        public override IBFBackgroundLayer CreateLayer()
        {
            return new BFGradientLayer(this);
        }

        private static Gradient CreateDefaultGradient()
        {
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(new Color(0.039f, 0.047f, 0.118f), 0f),
                    new GradientColorKey(new Color(0.016f, 0.024f, 0.067f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(1f, 1f)
                });
            return gradient;
        }
    }
}