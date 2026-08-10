using UnityEngine;

namespace BFTools.Visuals.Background
{
    public class BFImpossibleWeaveLayerConfig : BFBackgroundLayerConfig
    {
        [Header("Color")]
        [SerializeField]
        private Gradient colorGradient = CreateDefaultGradient();

        [SerializeField, Range(0f, 1f)]
        [Tooltip("Overall opacity of the weave.")]
        private float opacity = 0.75f;

        [Header("Weave")]
        [SerializeField, Range(2, 16)]
        [Tooltip("Number of independent seams running through the field.")]
        private int seamCount = 8;

        [SerializeField, Range(80, 320)]
        [Tooltip("Number of points used to construct each seam.")]
        private int resolution = 180;

        [SerializeField, Range(0.1f, 1f)]
        [Tooltip("Distance between the major fold centers, relative to screen size.")]
        private float foldSpacing = 0.42f;

        [SerializeField, Range(0.01f, 0.25f)]
        [Tooltip("Thickness of each luminous seam in pixels.")]
        private float thickness = 0.035f;

        [SerializeField, Range(0f, 1f)]
        [Tooltip("How strongly seams distort around fold centers.")]
        private float foldStrength = 0.7f;

        [SerializeField, Range(0f, 1f)]
        [Tooltip("How sharply seams pinch at intersections.")]
        private float pinchStrength = 0.55f;

        [Header("Shape")]
        [SerializeField, Range(0f, 1f)]
        [Tooltip("Large-scale curvature of the weave.")]
        private float curvature = 0.45f;

        [SerializeField, Range(0f, 1f)]
        [Tooltip("Secondary deformation layered over the primary shape.")]
        private float secondaryDeformation = 0.35f;

        [SerializeField, Range(0f, 8f)]
        [Tooltip("Number of large folds across the screen.")]
        private float foldFrequency = 2.5f;

        [Header("Animation")]
        [SerializeField, Range(0f, 2f)]
        [Tooltip("Speed at which the geometry folds.")]
        private float foldSpeed = 0.25f;

        [SerializeField, Range(0f, 2f)]
        [Tooltip("Speed of the secondary deformation.")]
        private float deformationSpeed = 0.17f;

        [SerializeField, Range(0f, 2f)]
        [Tooltip("Slow rotation of the entire mathematical field.")]
        private float rotationSpeed = 0.08f;

        [SerializeField, Range(0f, 1f)]
        [Tooltip("Speed of the global breathing motion.")]
        private float breathingSpeed = 0.12f;

        [SerializeField, Range(0f, 0.25f)]
        [Tooltip("Amount of global breathing.")]
        private float breathingAmount = 0.08f;

        [Header("Intersections")]
        [SerializeField, Range(0f, 3f)]
        [Tooltip("Brightness multiplier at weave intersections.")]
        private float intersectionGlow = 1.4f;

        [SerializeField, Range(0f, 1f)]
        [Tooltip("How much intersection brightness moves through the field.")]
        private float intersectionMotion = 0.25f;

        internal Gradient ColorGradient => colorGradient;
        internal float Opacity => opacity;
        internal int SeamCount => seamCount;
        internal int Resolution => resolution;
        internal float FoldSpacing => foldSpacing;
        internal float Thickness => thickness;
        internal float FoldStrength => foldStrength;
        internal float PinchStrength => pinchStrength;
        internal float Curvature => curvature;
        internal float SecondaryDeformation => secondaryDeformation;
        internal float FoldFrequency => foldFrequency;
        internal float FoldSpeed => foldSpeed;
        internal float DeformationSpeed => deformationSpeed;
        internal float RotationSpeed => rotationSpeed;
        internal float BreathingSpeed => breathingSpeed;
        internal float BreathingAmount => breathingAmount;
        internal float IntersectionGlow => intersectionGlow;
        internal float IntersectionMotion => intersectionMotion;

        public override IBFBackgroundLayer CreateLayer()
        {
            return new BFImpossibleWeaveLayer(this);
        }

        private static Gradient CreateDefaultGradient()
        {
            Gradient gradient = new Gradient();

            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(
                        new Color(0.06f, 0.12f, 0.30f),
                        0f),

                    new GradientColorKey(
                        new Color(0.20f, 0.85f, 0.95f),
                        0.35f),

                    new GradientColorKey(
                        new Color(0.65f, 0.25f, 1f),
                        0.7f),

                    new GradientColorKey(
                        new Color(1f, 0.35f, 0.65f),
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