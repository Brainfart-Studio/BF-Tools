using UnityEngine;

namespace BFTools.Visuals.Background
{
    public class BFContradictionFieldLayerConfig
        : BFBackgroundLayerConfig
    {
        [Header("Color")]
        [SerializeField]
        private Gradient colorGradient = CreateDefaultGradient();

        [SerializeField, Range(0f, 1f)]
        [Tooltip("Overall opacity of the contradiction field.")]
        private float opacity = 0.65f;

        [Header("Claims")]
        [SerializeField, Range(2, 12)]
        [Tooltip("Number of independent geometric claims.")]
        private int claimCount = 7;

        [SerializeField, Range(32, 220)]
        [Tooltip("Points used to construct each claim.")]
        private int resolution = 120;

        [SerializeField, Range(0.005f, 0.08f)]
        [Tooltip("Base thickness of each claim.")]
        private float thickness = 0.018f;

        [Header("Geometry")]
        [SerializeField, Range(0f, 1f)]
        [Tooltip("Large-scale curvature of the claims.")]
        private float curvature = 0.42f;

        [SerializeField, Range(0f, 1f)]
        [Tooltip("How strongly claims react to nearby contradictions.")]
        private float contradictionStrength = 0.8f;

        [SerializeField, Range(0f, 1f)]
        [Tooltip("How much claims try to avoid their own previous paths.")]
        private float selfContradiction = 0.35f;

        [SerializeField, Range(0f, 1f)]
        [Tooltip("How strongly old contradictions affect new geometry.")]
        private float memoryInfluence = 0.4f;

        [Header("Motion")]
        [SerializeField, Range(0f, 2f)]
        [Tooltip("Base speed of the geometric argument.")]
        private float speed = 0.18f;

        [SerializeField, Range(0f, 2f)]
        [Tooltip("Speed of the internal phase disagreement.")]
        private float disagreementSpeed = 0.23f;

        [SerializeField, Range(-1f, 1f)]
        [Tooltip("Biases the entire field toward rotational or counter-rotational motion.")]
        private float rotationalBias = 0.1f;

        [Header("Contradictions")]
        [SerializeField, Range(0.05f, 0.5f)]
        [Tooltip("Distance at which two claims begin contradicting one another.")]
        private float contradictionRadius = 0.18f;

        [SerializeField, Range(0.1f, 3f)]
        [Tooltip("Sharpness of contradiction events.")]
        private float contradictionSharpness = 1.4f;

        [SerializeField, Range(0f, 1f)]
        [Tooltip("How strongly contradiction events alter brightness.")]
        private float contradictionGlow = 0.7f;

        [Header("Memory")]
        [SerializeField, Range(0f, 1f)]
        [Tooltip("How quickly geometric memories disappear.")]
        private float memoryDecay = 0.08f;

        [SerializeField, Range(0f, 1f)]
        [Tooltip("How strongly a contradiction writes to the field's memory.")]
        private float memoryWriteStrength = 0.65f;

        internal Gradient ColorGradient => colorGradient;
        internal float Opacity => opacity;

        internal int ClaimCount => claimCount;
        internal int Resolution => resolution;
        internal float Thickness => thickness;

        internal float Curvature => curvature;
        internal float ContradictionStrength => contradictionStrength;
        internal float SelfContradiction => selfContradiction;
        internal float MemoryInfluence => memoryInfluence;

        internal float Speed => speed;
        internal float DisagreementSpeed => disagreementSpeed;
        internal float RotationalBias => rotationalBias;

        internal float ContradictionRadius => contradictionRadius;
        internal float ContradictionSharpness => contradictionSharpness;
        internal float ContradictionGlow => contradictionGlow;

        internal float MemoryDecay => memoryDecay;
        internal float MemoryWriteStrength => memoryWriteStrength;

        public override IBFBackgroundLayer CreateLayer()
        {
            return new BFContradictionFieldLayer(this);
        }

        private static Gradient CreateDefaultGradient()
        {
            Gradient gradient = new Gradient();

            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(
                        new Color(0.10f, 0.08f, 0.28f),
                        0f),

                    new GradientColorKey(
                        new Color(0.20f, 0.65f, 1f),
                        0.3f),

                    new GradientColorKey(
                        new Color(0.75f, 0.25f, 1f),
                        0.65f),

                    new GradientColorKey(
                        new Color(1f, 0.25f, 0.48f),
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