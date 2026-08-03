using System.Collections.Generic;
using UnityEngine;

namespace BFTools.Visuals.Background
{
    public class BFAuroraRibbonsLayerConfig : BFBackgroundLayerConfig
    {
        [SerializeField]
        private List<Color> ribbonColors = new List<Color>
        {
            new Color(0.5f, 1f, 0.831f),
            new Color(0.5f, 0.608f, 1f),
            new Color(0.765f, 0.5f, 1f),
            new Color(1f, 0.5f, 0.722f)
        };
        [SerializeField, Range(1, 8)] private int ribbonCount = 4;
        [SerializeField, Range(0f, 0.5f)] private float ribbonSpacing = 0.125f;
        [SerializeField, Range(0.05f, 1.5f)] private float waveSpeed = 0.35f;
        [SerializeField, Range(0.1f, 3f)] private float minSpeedVariance = 0.7f;
        [SerializeField, Range(0.1f, 3f)] private float maxSpeedVariance = 1.3f;
        [SerializeField, Range(20f, 260f)] private float amplitude = 110f;
        [SerializeField, Range(0.1f, 3f)] private float minAmplitudeVariance = 0.7f;
        [SerializeField, Range(0.1f, 3f)] private float maxAmplitudeVariance = 1.2f;
        [SerializeField, Range(0f, 8f)] private float waveFrequency = 4f;
        [SerializeField, Range(0.1f, 3f)] private float minFrequencyVariance = 0.8f;
        [SerializeField, Range(0.1f, 3f)] private float maxFrequencyVariance = 1.4f;
        [SerializeField, Range(1f, 100f)] private float thickness = 3f;
        [SerializeField, Range(0.1f, 3f)] private float minThicknessVariance = 0.8f;
        [SerializeField, Range(0.1f, 3f)] private float maxThicknessVariance = 1.3f;
        [SerializeField, Range(0, 60)] private float glow = 18f;
        [SerializeField, Range(0f, 1f)] private float overallOpacity = 1f;
        [SerializeField, Range(-180f, 180f)] private float angle;

        internal IReadOnlyList<Color> RibbonColors => ribbonColors;
        internal int RibbonCount => ribbonCount;
        internal float RibbonSpacing => ribbonSpacing;
        internal float WaveSpeed => waveSpeed;
        internal float MinSpeedVariance => minSpeedVariance;
        internal float MaxSpeedVariance => maxSpeedVariance;
        internal float Amplitude => amplitude;
        internal float MinAmplitudeVariance => minAmplitudeVariance;
        internal float MaxAmplitudeVariance => maxAmplitudeVariance;
        internal float WaveFrequency => waveFrequency;
        internal float MinFrequencyVariance => minFrequencyVariance;
        internal float MaxFrequencyVariance => maxFrequencyVariance;
        internal float Thickness => thickness;
        internal float MinThicknessVariance => minThicknessVariance;
        internal float MaxThicknessVariance => maxThicknessVariance;
        internal float Glow => glow;
        internal float OverallOpacity => overallOpacity;
        internal float Angle => angle;

        public override IBFBackgroundLayer CreateLayer()
        {
            return new BFAuroraRibbonsLayer(this);
        }
    }
}