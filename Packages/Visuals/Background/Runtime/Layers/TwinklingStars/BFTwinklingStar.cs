using UnityEngine;

namespace BFTools.Visuals.Background
{
    public class BFTwinklingStar
    {
        private readonly BFTwinklingStarsLayerConfig config;

        private float phase;
        private float alphaBase;
        private float twinkleSpeed;
        private float twinkleDepth;

        public Vector2 Position { get; private set; }
        public float Size { get; private set; }
        public float Alpha { get; private set; }

        public BFTwinklingStar(BFTwinklingStarsLayerConfig config)
        {
            this.config = config;

            Position = new Vector2(Random.Range(0f, 1f), Random.Range(0f, 1f));
            Size = Random.Range(0.6f, 1.8f);
            phase = Random.Range(0f, Mathf.PI * 2f);
            alphaBase = Random.Range(0.3f, 0.9f);
            twinkleSpeed = Random.Range(0.002f, 0.004f);
            twinkleDepth = Random.Range(0.3f, 0.5f);
        }

        public void Tick(float dt)
        {
            phase += dt * twinkleSpeed;
            Alpha = config.Twinkle ? alphaBase * (1f - twinkleDepth + twinkleDepth * Mathf.Sin(phase)) : alphaBase;
        }
    }
}