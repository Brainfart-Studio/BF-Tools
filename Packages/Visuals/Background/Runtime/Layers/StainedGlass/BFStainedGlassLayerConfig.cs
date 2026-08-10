using UnityEngine;

namespace BFTools.Visuals.Background
{
    public class BFStainedGlassLayerConfig : BFBackgroundLayerConfig
    {
        [Header("Glass")]
        [SerializeField]
        private Gradient colorGradient = CreateDefaultGradient();

        [SerializeField, Range(0f, 1f)]
        private float opacity = 1f;

        [SerializeField, Range(4, 96)]
        private int paneCount = 32;

        [SerializeField, Range(24, 160)]
        private int resolutionX = 96;

        [SerializeField, Range(14, 90)]
        private int resolutionY = 54;

        [SerializeField, Range(0f, 1f)]
        private float irregularity = 0.65f;

        [Header("Lead")]
        [SerializeField, Range(0f, 0.2f)]
        private float leadThickness = 0.04f;

        [SerializeField, Range(0f, 1f)]
        private float leadDarkness = 0.8f;

        [Header("Animation")]
        [SerializeField, Range(0f, 2f)]
        private float animationSpeed = 0.2f;

        [SerializeField, Range(0f, 0.5f)]
        private float lightMovement = 0.08f;

        internal Gradient ColorGradient => colorGradient;
        internal float Opacity => opacity;

        internal int PaneCount => paneCount;
        internal int ResolutionX => resolutionX;
        internal int ResolutionY => resolutionY;
        internal float Irregularity => irregularity;

        internal float LeadThickness => leadThickness;
        internal float LeadDarkness => leadDarkness;

        internal float AnimationSpeed => animationSpeed;
        internal float LightMovement => lightMovement;

        public override IBFBackgroundLayer CreateLayer()
        {
            return new BFStainedGlassLayer(this);
        }

        private static Gradient CreateDefaultGradient()
        {
            Gradient gradient = new Gradient();

            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(
                        new Color(0.03f, 0.12f, 0.45f),
                        0f),

                    new GradientColorKey(
                        new Color(0.02f, 0.55f, 0.75f),
                        0.25f),

                    new GradientColorKey(
                        new Color(0.35f, 0.05f, 0.65f),
                        0.5f),

                    new GradientColorKey(
                        new Color(0.9f, 0.08f, 0.3f),
                        0.75f),

                    new GradientColorKey(
                        new Color(1f, 0.45f, 0.05f),
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