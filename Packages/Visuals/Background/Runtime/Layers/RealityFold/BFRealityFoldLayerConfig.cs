using UnityEngine;

namespace BFTools.Visuals.Background
{
    public class BFRealityFoldLayerConfig : BFBackgroundLayerConfig
    {
        [Header("Color")]
        [SerializeField]
        private Gradient colorGradient = CreateDefaultGradient();

        [SerializeField, Range(0f, 1f)]
        [Tooltip("Overall brightness of the effect.")]
        private float brightness = 1f;

        [SerializeField, Range(0f, 1f)]
        [Tooltip("Overall opacity.")]
        private float opacity = 1f;

        [Header("Resolution")]
        [SerializeField, Range(16, 64)]
        [Tooltip("Internal texture width. Lower values are substantially cheaper.")]
        private int textureWidth = 32;

        [SerializeField, Range(16, 64)]
        [Tooltip("Internal texture height. Lower values are substantially cheaper.")]
        private int textureHeight = 32;

        [SerializeField, Range(0.02f, 0.25f)]
        [Tooltip("Minimum time between texture updates in seconds.")]
        private float updateInterval = 0.05f;

        [Header("Folds")]
        [SerializeField, Range(1, 5)]
        [Tooltip("Number of moving reality folds.")]
        private int foldCount = 3;

        [SerializeField, Range(0.1f, 1f)]
        [Tooltip("Size of each fold.")]
        private float foldSize = 0.42f;

        [SerializeField, Range(0f, 3f)]
        [Tooltip("Strength of the fold distortion.")]
        private float foldStrength = 1.4f;

        [SerializeField, Range(0.1f, 8f)]
        [Tooltip("How tightly the folds bend the color field.")]
        private float foldFrequency = 2.5f;

        [Header("Motion")]
        [SerializeField, Range(0f, 2f)]
        [Tooltip("Speed at which the folds orbit.")]
        private float movementSpeed = 0.25f;

        [SerializeField, Range(0f, 0.5f)]
        [Tooltip("Distance the folds travel.")]
        private float movementAmount = 0.18f;

        [SerializeField, Range(0f, 2f)]
        [Tooltip("Speed of the internal color motion.")]
        private float colorSpeed = 0.2f;

        [Header("Chaos")]
        [SerializeField, Range(0f, 1f)]
        [Tooltip("Adds a second distorted layer to the fold pattern.")]
        private float chaos = 0.35f;

        [SerializeField, Range(0f, 1f)]
        [Tooltip("How strongly the fold centers periodically collapse together.")]
        private float collapse = 0.35f;

        [SerializeField, Range(0f, 1f)]
        [Tooltip("How strongly fold boundaries brighten.")]
        private float edgeGlow = 0.35f;

        internal Gradient ColorGradient => colorGradient;
        internal float Brightness => brightness;
        internal float Opacity => opacity;

        internal int TextureWidth => textureWidth;
        internal int TextureHeight => textureHeight;
        internal float UpdateInterval => updateInterval;

        internal int FoldCount => foldCount;
        internal float FoldSize => foldSize;
        internal float FoldStrength => foldStrength;
        internal float FoldFrequency => foldFrequency;

        internal float MovementSpeed => movementSpeed;
        internal float MovementAmount => movementAmount;
        internal float ColorSpeed => colorSpeed;

        internal float Chaos => chaos;
        internal float Collapse => collapse;
        internal float EdgeGlow => edgeGlow;

        public override IBFBackgroundLayer CreateLayer()
        {
            return new BFRealityFoldLayer(this);
        }

        private static Gradient CreateDefaultGradient()
        {
            Gradient gradient = new Gradient();

            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(
                        new Color(0.02f, 0.01f, 0.08f),
                        0f),

                    new GradientColorKey(
                        new Color(0.08f, 0.02f, 0.20f),
                        0.28f),

                    new GradientColorKey(
                        new Color(0.02f, 0.35f, 0.55f),
                        0.52f),

                    new GradientColorKey(
                        new Color(0.55f, 0.04f, 0.42f),
                        0.75f),

                    new GradientColorKey(
                        new Color(0.01f, 0.01f, 0.04f),
                        1f)
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