using System.Collections.Generic;
using UnityEngine;

namespace BFTools.Visuals.Background
{
    public class BFAuroraRibbonsLayerConfig : BFBackgroundLayerConfig
    {
        [Header("Color")]
        [SerializeField, Tooltip("Palette each ribbon draws its color from, cycling through in order if there are more ribbons than colors.")]
        private List<Color> ribbonColors = new List<Color>
        {
            new Color(0.5f, 1f, 0.831f),
            new Color(0.5f, 0.608f, 1f),
            new Color(0.765f, 0.5f, 1f),
            new Color(1f, 0.5f, 0.722f)
        };

        [SerializeField, Range(0f, 1f), Tooltip("Multiplies every ribbon's final alpha, on top of each color's own alpha. Use this to fade all ribbons at once.")]
        private float overallOpacity = 1f;

        [Header("Layout")]
        [SerializeField, Range(1, 8), Tooltip("How many aurora ribbons to render.")]
        private int ribbonCount = 4;

        [SerializeField, Range(0f, 0.5f), Tooltip("Vertical distance between each ribbon's baseline, independent of thickness. 0 stacks ribbons on top of each other; higher values spread them farther apart.")]
        private float ribbonSpacing = 0.125f;

        [SerializeField, Range(-180f, 180f), Tooltip("Rotates the whole ribbon band. 0 runs horizontal, 90 runs vertical.")]
        private float angle;

        [Header("Thickness")]
        [SerializeField, Range(1f, 100f), Tooltip("Base ribbon thickness, in pixels.")]
        private float thickness = 3f;

        [SerializeField, Range(0.1f, 3f), Tooltip("Smallest random thickness multiplier applied per ribbon, relative to Thickness.")]
        private float minThicknessVariance = 0.8f;

        [SerializeField, Range(0.1f, 3f), Tooltip("Largest random thickness multiplier applied per ribbon, relative to Thickness.")]
        private float maxThicknessVariance = 1.3f;

        [Header("Wave Shape")]
        [SerializeField, Range(20f, 260f), Tooltip("How far each ribbon's wave swings, in pixels.")]
        private float amplitude = 110f;

        [SerializeField, Range(0.1f, 3f), Tooltip("Smallest random amplitude multiplier applied per ribbon, relative to Amplitude.")]
        private float minAmplitudeVariance = 0.7f;

        [SerializeField, Range(0.1f, 3f), Tooltip("Largest random amplitude multiplier applied per ribbon, relative to Amplitude.")]
        private float maxAmplitudeVariance = 1.2f;

        [SerializeField, Range(0f, 8f), Tooltip("How many wave cycles appear across the screen width.")]
        private float waveFrequency = 4f;

        [SerializeField, Range(0.1f, 3f), Tooltip("Smallest random frequency multiplier applied per ribbon, relative to Wave Frequency.")]
        private float minFrequencyVariance = 0.8f;

        [SerializeField, Range(0.1f, 3f), Tooltip("Largest random frequency multiplier applied per ribbon, relative to Wave Frequency.")]
        private float maxFrequencyVariance = 1.4f;

        [SerializeField, Range(0f, 1f), Tooltip("How strongly a second, smaller wave layers on top of the primary wave, relative to Amplitude. 0 disables it.")]
        private float secondaryWaveStrength = 0.35f;

        [SerializeField, Range(0f, 2f), Tooltip("How fast the secondary wave animates, relative to Wave Speed.")]
        private float secondaryWaveSpeedScale = 0.6f;

        [Header("Wave Animation")]
        [SerializeField, Range(0.05f, 1.5f), Tooltip("How fast the waves animate.")]
        private float waveSpeed = 0.35f;

        [SerializeField, Range(0.1f, 3f), Tooltip("Smallest random speed multiplier applied per ribbon, relative to Wave Speed.")]
        private float minSpeedVariance = 0.7f;

        [SerializeField, Range(0.1f, 3f), Tooltip("Largest random speed multiplier applied per ribbon, relative to Wave Speed.")]
        private float maxSpeedVariance = 1.3f;

        [Header("Rotation Animation")]
        [SerializeField, Range(-180f, 180f), Tooltip("Degrees per second the ribbon band's angle rotates. 0 disables rotation; negative values reverse the direction.")]
        private float rotationSpeed;

        [SerializeField, Range(0f, 1f), Tooltip("How fast the rotation oscillates back and forth. 0 disables it.")]
        private float rotationOscillationSpeed;

        [SerializeField, Range(0f, 90f), Tooltip("How far the rotation swings from its current angle when oscillating.")]
        private float rotationOscillationAmplitude;

        [Header("Glow")]
        [SerializeField, Range(0, 60), Tooltip("How strong the additive glow effect is on the ribbons.")]
        private float glow = 18f;

        internal IReadOnlyList<Color> RibbonColors => ribbonColors;
        internal float OverallOpacity => overallOpacity;
        internal int RibbonCount => ribbonCount;
        internal float RibbonSpacing => ribbonSpacing;
        internal float Angle => angle;
        internal float Thickness => thickness;
        internal float MinThicknessVariance => minThicknessVariance;
        internal float MaxThicknessVariance => maxThicknessVariance;
        internal float Amplitude => amplitude;
        internal float MinAmplitudeVariance => minAmplitudeVariance;
        internal float MaxAmplitudeVariance => maxAmplitudeVariance;
        internal float WaveFrequency => waveFrequency;
        internal float MinFrequencyVariance => minFrequencyVariance;
        internal float MaxFrequencyVariance => maxFrequencyVariance;
        internal float SecondaryWaveStrength => secondaryWaveStrength;
        internal float SecondaryWaveSpeedScale => secondaryWaveSpeedScale;
        internal float WaveSpeed => waveSpeed;
        internal float MinSpeedVariance => minSpeedVariance;
        internal float MaxSpeedVariance => maxSpeedVariance;
        internal float RotationSpeed => rotationSpeed;
        internal float RotationOscillationSpeed => rotationOscillationSpeed;
        internal float RotationOscillationAmplitude => rotationOscillationAmplitude;
        internal float Glow => glow;

        public override IBFBackgroundLayer CreateLayer()
        {
            return new BFAuroraRibbonsLayer(this);
        }
    }
}