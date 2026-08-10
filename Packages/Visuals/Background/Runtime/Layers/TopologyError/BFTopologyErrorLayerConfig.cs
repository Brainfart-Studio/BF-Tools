using UnityEngine;

namespace BFTools.Visuals.Background
{
    public class BFTopologyErrorLayerConfig : BFBackgroundLayerConfig
    {
        [Header("Color")]
        [SerializeField]
        private Gradient colorGradient = CreateDefaultGradient();

        [SerializeField, Range(0f, 1f)]
        [Tooltip("Overall opacity.")]
        private float opacity = 1f;

        [Header("Topology")]
        [SerializeField, Range(3, 16)]
        [Tooltip("Number of colored territories.")]
        private int territoryCount = 8;

        [SerializeField, Range(16, 128)]
        [Tooltip("Horizontal resolution of the generated field.")]
        private int resolutionX = 64;

        [SerializeField, Range(9, 72)]
        [Tooltip("Vertical resolution of the generated field.")]
        private int resolutionY = 36;

        [SerializeField, Range(0f, 1f)]
        [Tooltip("How strongly neighboring territories distort each other.")]
        private float irregularity = 0.35f;

        [Header("Living Geometry")]
        [SerializeField, Range(0f, 1f)]
        [Tooltip("Amount of continuous territorial movement.")]
        private float driftAmount = 0.15f;

        [SerializeField, Range(0f, 1f)]
        [Tooltip("Speed of continuous territorial movement.")]
        private float driftSpeed = 0.15f;

        [Header("Reality Breaks")]
        [SerializeField, Range(0.1f, 10f)]
        [Tooltip("Time between topology mutations.")]
        private float mutationInterval = 2f;

        [SerializeField, Range(0.1f, 4f)]
        [Tooltip("Duration of a topology mutation.")]
        private float mutationDuration = 1f;

        [SerializeField, Range(0f, 1f)]
        [Tooltip("Strength of each topology mutation.")]
        private float mutationStrength = 0.8f;

        [SerializeField, Range(0f, 1f)]
        [Tooltip("Chance that a mutation creates an empty hole.")]
        private float holeChance = 0.25f;

        [SerializeField, Range(0f, 1f)]
        [Tooltip("Chance that a territory splits into another territory.")]
        private float splitChance = 0.25f;

        [SerializeField, Range(0f, 1f)]
        [Tooltip("Chance that two territories exchange identities.")]
        private float swapChance = 0.2f;

        [SerializeField, Range(0f, 1f)]
        [Tooltip("Chance that one territory appears inside another.")]
        private float nestingChance = 0.2f;

        [Header("Boundaries")]
        [SerializeField, Range(0f, 1f)]
        [Tooltip("Darkness of territory boundaries.")]
        private float boundaryDarkness = 0.2f;

        [SerializeField, Range(0f, 1f)]
        [Tooltip("Brightness of active mutation zones.")]
        private float mutationGlow = 0.7f;

        internal Gradient ColorGradient => colorGradient;
        internal float Opacity => opacity;

        internal int TerritoryCount => territoryCount;
        internal int ResolutionX => resolutionX;
        internal int ResolutionY => resolutionY;
        internal float Irregularity => irregularity;

        internal float DriftAmount => driftAmount;
        internal float DriftSpeed => driftSpeed;

        internal float MutationInterval => mutationInterval;
        internal float MutationDuration => mutationDuration;
        internal float MutationStrength => mutationStrength;
        internal float HoleChance => holeChance;
        internal float SplitChance => splitChance;
        internal float SwapChance => swapChance;
        internal float NestingChance => nestingChance;

        internal float BoundaryDarkness => boundaryDarkness;
        internal float MutationGlow => mutationGlow;

        public override IBFBackgroundLayer CreateLayer()
        {
            return new BFTopologyErrorLayer(this);
        }

        private static Gradient CreateDefaultGradient()
        {
            Gradient gradient = new Gradient();

            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(
                        new Color(0.03f, 0.02f, 0.12f),
                        0f),

                    new GradientColorKey(
                        new Color(0.02f, 0.25f, 0.65f),
                        0.25f),

                    new GradientColorKey(
                        new Color(0.45f, 0.04f, 0.7f),
                        0.5f),

                    new GradientColorKey(
                        new Color(0.95f, 0.05f, 0.35f),
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