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
            twinkleDepth = Random.Range(0.3f, 0.5f);
            ColorT = Random.Range(0f, 1f);

            float noiseX = Position.x * NoiseScale;
            float noiseY = Position.y * NoiseScale;
            float sizeNoise = Mathf.PerlinNoise(noiseX, noiseY);
            float alphaNoise = Mathf.PerlinNoise(noiseX + 100f, noiseY + 100f);
            float speedNoise = Mathf.PerlinNoise(noiseX + 200f, noiseY + 200f);

            Size = Mathf.Lerp(0.6f, 1.8f, sizeNoise);
            alphaBase = Mathf.Lerp(0.3f, 0.9f, alphaNoise);
            twinkleSpeed = Mathf.Lerp(0.002f, 0.004f, speedNoise);
        }

        public void Tick(float dt)
        {
            phase += dt * twinkleSpeed;
            Alpha = config.Twinkle ? alphaBase * (1f - twinkleDepth + twinkleDepth * Mathf.Sin(phase)) : alphaBase;
        }
    }
}