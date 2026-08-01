using UnityEngine;

namespace BFTools.Visuals.Background
{
    public class BFTwinklingStar
    {
        private readonly BFTwinklingStarsLayerConfig config;

        private float phase;
        private float alphaBase;

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
        }

        public void Tick(float dt)
        {
            phase += dt * 0.003f;
            Alpha = config.Twinkle ? alphaBase * (0.6f + 0.4f * Mathf.Sin(phase)) : alphaBase;
        }
    }
}