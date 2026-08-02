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

            Size = Mathf.Lerp(config.MinSize, config.MaxSize, sizeNoise);
            alphaBase = Mathf.Lerp(config.MinAlpha, config.MaxAlpha, alphaNoise);
            twinkleSpeed = Mathf.Lerp(config.MinTwinkleSpeed, config.MaxTwinkleSpeed, speedNoise);
        }

        public void Tick(float dt)
        {
            phase += dt * twinkleSpeed;
            Alpha = config.Twinkle ? alphaBase * (1f - twinkleDepth + twinkleDepth * Mathf.Sin(phase)) : alphaBase;
        }
    }
}