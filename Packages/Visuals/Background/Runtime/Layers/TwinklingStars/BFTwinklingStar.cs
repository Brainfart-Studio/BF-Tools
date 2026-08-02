using UnityEngine;

namespace BFTools.Visuals.Background
{
    public class BFTwinklingStar
    {
        private const float NoiseScale = 12f;

        private readonly BFTwinklingStarsLayerConfig config;

        private float phase;
        private float alphaBase;
        private float twinkleSpeed;
        private float twinkleDepth;

        public Vector2 Position { get; private set; }
        public float Size { get; private set; }
        public float Alpha { get; private set; }
        public float ColorT { get; private set; }

        public BFTwinklingStar(BFTwinklingStarsLayerConfig config)
        {
            this.config = config;

            Position = new Vector2(Random.Range(0f, 1f), Random.Range(0f, 1f));
            phase = Random.Range(0f, Mathf.PI * 2f);
            twinkleDepth = Random.Range(config.MinTwinkleDepth, config.MaxTwinkleDepth);
            ColorT = Random.Range(0f, 1f);

            float noiseX = Position.x * NoiseScale;
            float noiseY = Position.y * NoiseScale;
            float sizeNoise = Mathf.PerlinNoise(noiseX, noiseY);
            float alphaNoise = Mathf.PerlinNoise(noiseX + 100f, noiseY + 100f);
            float speedNoise = Mathf.PerlinNoise(noiseX + 200f, noiseY + 200f);

            float sizeT = Shape(sizeNoise, config.MinSize, config.MaxSize, config.AverageSize, config.SizeOutliers);
            float alphaT = Shape(alphaNoise, config.MinAlpha, config.MaxAlpha, config.AverageAlpha, config.AlphaOutliers);

            Size = Mathf.Lerp(config.MinSize, config.MaxSize, sizeT);
            alphaBase = Mathf.Lerp(config.MinAlpha, config.MaxAlpha, alphaT);
            twinkleSpeed = Mathf.Lerp(config.MinTwinkleSpeed, config.MaxTwinkleSpeed, speedNoise);
        }

        public void Tick(float dt)
        {
            phase += dt * twinkleSpeed;
            Alpha = config.Twinkle ? alphaBase * (1f - twinkleDepth + twinkleDepth * Mathf.Sin(phase)) : alphaBase;
        }

        private static float Shape(float t, float min, float max, float average, float outliers)
        {
            float averageT = Mathf.InverseLerp(min, max, average);
            return Bias(Gain(t, outliers), averageT);
        }

        // Schlick's bias curve: keeps 0 -> 0 and 1 -> 1, remaps 0.5 -> b.
        private static float Bias(float t, float b)
        {
            b = Mathf.Clamp(b, 0.0001f, 0.9999f);
            return t / ((1f / b - 2f) * (1f - t) + 1f);
        }

        // Schlick's gain curve: g = 0.5 is neutral, g > 0.5 pushes values toward 0/1 (more outliers),
        // g < 0.5 pulls values toward 0.5 (tighter clustering).
        private static float Gain(float t, float g)
        {
            if (t < 0.5f)
                return Bias(t * 2f, g) * 0.5f;
            return Bias(t * 2f - 1f, 1f - g) * 0.5f + 0.5f;
        }
    }
}