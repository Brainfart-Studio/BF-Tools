using UnityEngine;

namespace BFTools.Visuals.Background
{
    public class BFTwinklingStarsLayerConfig : BFBackgroundLayerConfig
    {
        [SerializeField, Range(0, 300)] private int starCount = 120;
        [SerializeField] private bool twinkle = true;

        [Header("Color")]
        [SerializeField] private Gradient colorGradient = CreateDefaultGradient();

        internal int StarCount => starCount;
        internal bool Twinkle => twinkle;
        internal Gradient ColorGradient => colorGradient;

        public override IBFBackgroundLayer CreateLayer()
        {
            return new BFTwinklingStarsLayer(this);
        }

        private static Gradient CreateDefaultGradient()
        {
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(Color.white, 1f)
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