using UnityEngine;

namespace BFTools.Visuals.Background.AuroraRibbons
{
    public class BFAuroraRibbon
    {
        private readonly BFAuroraRibbonsConfig config;
        private readonly int index;

        private float baseYNormalized;
        private float phase;
        private float freq;
        private float speedMult;
        private float ampMult;
        private float thicknessMult;

        public Color Color { get; private set; }
        public float CurrentThickness { get; private set; }

        public BFAuroraRibbon(int index, BFAuroraRibbonsConfig config)
        {
            this.index = index;
            this.config = config;

            Color = config.RibbonColors[index % config.RibbonColors.Count];
            baseYNormalized = 0.25f + (index / Mathf.Max((float)config.RibbonCount, 1f)) * 0.5f;
            phase = Random.Range(0f, Mathf.PI * 2f);
            freq = Random.Range(0.8f, 1.4f);
            speedMult = Random.Range(0.7f, 1.3f);
            ampMult = Random.Range(0.7f, 1.2f);
            thicknessMult = Random.Range(0.8f, 1.3f);

            CurrentThickness = config.Thickness * thicknessMult;
        }

        public void Tick(float elapsedTime)
        {
            // Position sampling for rendering happens via SampleY at draw time.
        }

        public float SampleY(float xNormalized, float elapsedTime, float viewHeight)
        {
            float x = xNormalized;
            float y = baseYNormalized * viewHeight
                + Mathf.Sin(x * 0.004f * freq * 1000f + elapsedTime * speedMult + phase) * config.Amplitude * ampMult
                + Mathf.Sin(x * 0.011f * freq * 1000f + elapsedTime * speedMult * 0.6f) * config.Amplitude * ampMult * 0.35f;
            return y;
        }
    }
}