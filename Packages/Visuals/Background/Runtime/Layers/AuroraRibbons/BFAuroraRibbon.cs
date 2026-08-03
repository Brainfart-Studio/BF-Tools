using UnityEngine;

namespace BFTools.Visuals.Background
{
    public class BFAuroraRibbon
    {
        private const float SecondaryFrequencyRatio = 2.75f;

        private readonly BFAuroraRibbonsLayerConfig config;
        private readonly int index;
        private readonly float phase;
        private readonly float freqSeed;
        private readonly float speedSeed;
        private readonly float ampSeed;
        private readonly float thicknessSeed;

        private float freq;
        private float speedMult;
        private float ampMult;

        public Color Color { get; private set; }
        public float CurrentThickness { get; private set; }

        public BFAuroraRibbon(int index, BFAuroraRibbonsLayerConfig config)
        {
            this.index = index;
            this.config = config;

            phase = Random.Range(0f, Mathf.PI * 2f);
            freqSeed = Random.Range(0f, 1f);
            speedSeed = Random.Range(0f, 1f);
            ampSeed = Random.Range(0f, 1f);
            thicknessSeed = Random.Range(0f, 1f);

            Refresh();
        }

        // Re-samples every config-derived trait from the current config, keeping each ribbon's underlying
        // seed fixed so live edits reshape the distribution instead of re-rolling every ribbon.
        public void Refresh()
        {
            Color = config.RibbonColors.Count > 0
                ? config.RibbonColors[index % config.RibbonColors.Count]
                : Color.white;

            freq = Mathf.Lerp(config.MinFrequencyVariance, config.MaxFrequencyVariance, freqSeed);
            speedMult = Mathf.Lerp(config.MinSpeedVariance, config.MaxSpeedVariance, speedSeed);
            ampMult = Mathf.Lerp(config.MinAmplitudeVariance, config.MaxAmplitudeVariance, ampSeed);
            float thicknessMult = Mathf.Lerp(config.MinThicknessVariance, config.MaxThicknessVariance, thicknessSeed);

            CurrentThickness = config.Thickness * thicknessMult;
        }

        public float SampleY(float xNormalized, float elapsedTime, float viewHeight)
        {
            float centerOffset = index - (config.RibbonCount - 1) / 2f;
            float baseYNormalized = 0.5f + centerOffset * config.RibbonSpacing;

            float y = baseYNormalized * viewHeight
                + Mathf.Sin(xNormalized * config.WaveFrequency * freq + elapsedTime * speedMult + phase) * config.Amplitude * ampMult
                + Mathf.Sin(xNormalized * config.WaveFrequency * SecondaryFrequencyRatio * freq + elapsedTime * speedMult * config.SecondaryWaveSpeedScale) * config.Amplitude * ampMult * config.SecondaryWaveStrength;
            return y;
        }
    }
}