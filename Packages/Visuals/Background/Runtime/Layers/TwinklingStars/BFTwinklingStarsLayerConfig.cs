using UnityEngine;

namespace BFTools.Visuals.Background
{
    public class BFTwinklingStarsLayerConfig : BFBackgroundLayerConfig
    {
        [Header("Color")]
        [SerializeField, Tooltip("Palette stars sample their tint from, keyed by each star's random position along the ramp.")]
        private Gradient colorGradient = CreateDefaultGradient();

        [Header("Layout")]
        [SerializeField, Range(0, 300), Tooltip("How many stars to place on screen.")]
        private int starCount = 120;

        [Header("Size")]
        [SerializeField, Range(0.1f, 5f), Tooltip("Smallest star size, in pixels.")]
        private float minSize = 0.6f;

        [SerializeField, Range(0.1f, 5f), Tooltip("Largest star size, in pixels.")]
        private float maxSize = 1.8f;

        [SerializeField, Range(0.1f, 5f), Tooltip("Where most star sizes cluster, between Min Size and Max Size.")]
        private float averageSize = 1.2f;

        [SerializeField, Range(0f, 1f), Tooltip("How many stars land out near Min/Max Size instead of clustering around Average Size. 0 groups sizes tightly around the average; 1 spreads them out toward the extremes.")]
        private float sizeOutliers = 0.5f;

        [Header("Brightness")]
        [SerializeField, Range(0f, 1f), Tooltip("Dimmest a star's base brightness can be.")]
        private float minAlpha = 0.3f;

        [SerializeField, Range(0f, 1f), Tooltip("Brightest a star's base brightness can be.")]
        private float maxAlpha = 0.9f;

        [SerializeField, Range(0f, 1f), Tooltip("Where most star brightness clusters, between Min Brightness and Max Brightness.")]
        private float averageAlpha = 0.6f;

        [SerializeField, Range(0f, 1f), Tooltip("How many stars land out near Min/Max Brightness instead of clustering around Average Brightness. 0 groups brightness tightly around the average; 1 spreads it out toward the extremes.")]
        private float alphaOutliers = 0.5f;

        [Header("Twinkle")]
        [SerializeField, Tooltip("Whether stars pulse in brightness over time.")]
        private bool twinkle = true;

        [SerializeField, Range(0f, 0.02f), Tooltip("Slowest twinkle speed a star can have.")]
        private float minTwinkleSpeed = 0.002f;

        [SerializeField, Range(0f, 0.02f), Tooltip("Fastest twinkle speed a star can have.")]
        private float maxTwinkleSpeed = 0.004f;

        [SerializeField, Range(0f, 1f), Tooltip("Shallowest a star's twinkle dip can be. 0 keeps brightness essentially constant.")]
        private float minTwinkleDepth = 0.3f;

        [SerializeField, Range(0f, 1f), Tooltip("Deepest a star's twinkle dip can be.")]
        private float maxTwinkleDepth = 0.5f;

        internal Gradient ColorGradient => colorGradient;
        internal int StarCount => starCount;
        internal float MinSize => minSize;
        internal float MaxSize => maxSize;
        internal float AverageSize => averageSize;
        internal float SizeOutliers => sizeOutliers;
        internal float MinAlpha => minAlpha;
        internal float MaxAlpha => maxAlpha;
        internal float AverageAlpha => averageAlpha;
        internal float AlphaOutliers => alphaOutliers;
        internal bool Twinkle => twinkle;
        internal float MinTwinkleSpeed => minTwinkleSpeed;
        internal float MaxTwinkleSpeed => maxTwinkleSpeed;
        internal float MinTwinkleDepth => minTwinkleDepth;
        internal float MaxTwinkleDepth => maxTwinkleDepth;

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