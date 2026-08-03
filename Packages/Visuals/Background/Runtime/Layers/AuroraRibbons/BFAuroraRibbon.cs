using UnityEngine;

namespace BFTools.Visuals.Background
{
    public class BFAuroraRibbon
    {
        private const float SecondaryFrequencyRatio = 2.75f;

        private readonly BFAuroraRibbonsLayerConfig config;

        private float baseYNormalized;
        private float phase;
        private float freq;
        private float speedMult;
        private float ampMult;
        private float thicknessMult;

        public Color Color { get; private set; }
        public float CurrentThickness { get; private set; }

        public BFAuroraRibbon(int index, BFAuroraRibbonsLayerConfig config)
        {
            this.config = config;

            Color = config.RibbonColors.Count > 0
                ? config.RibbonColors[index % config.RibbonColors.Count]
                : Color.white;
            float centerOffset = index - (config.RibbonCount - 1) / 2f;
            baseYNormalized = 0.5f + centerOffset * config.RibbonSpacing;
            phase = Random.Range(0f, Mathf.PI * 2f);
            freq = Random.Range(config.MinFrequencyVariance, config.MaxFrequencyVariance);
            speedMult = Random.Range(config.MinSpeedVariance, config.MaxSpeedVariance);
            ampMult = Random.Range(config.MinAmplitudeVariance, config.MaxAmplitudeVariance);
            thicknessMult = Random.Range(config.MinThicknessVariance, config.MaxThicknessVariance);

            CurrentThickness = config.Thickness * thicknessMult;
        }

        public float SampleY(float xNormalized, float elapsedTime, float viewHeight)
        {
            float y = baseYNormalized * viewHeight
                + Mathf.Sin(xNormalized * config.WaveFrequency * freq + elapsedTime * speedMult + phase) * config.Amplitude * ampMult
                + Mathf.Sin(xNormalized * config.WaveFrequency * SecondaryFrequencyRatio * freq + elapsedTime * speedMult * 0.6f) * config.Amplitude * ampMult * 0.35f;
            return y;
        }
    }
}