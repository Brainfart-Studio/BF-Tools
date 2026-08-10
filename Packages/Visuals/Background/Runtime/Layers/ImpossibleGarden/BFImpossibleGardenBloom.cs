using UnityEngine;

namespace BFTools.Visuals.Background
{
    internal sealed class BFImpossibleGardenBloom
    {
        private readonly BFImpossibleGardenLayerConfig config;
        private readonly int index;
        private readonly float seedA;
        private readonly float seedB;
        private readonly float seedC;
        private readonly float seedD;
        private readonly float seedE;
        private readonly Vector2 basePosition;

        public Vector2 Position { get; private set; }
        public float Radius { get; private set; }
        public Color Color { get; private set; }
        public float Alpha { get; private set; }

        public BFImpossibleGardenBloom(
            int index,
            BFImpossibleGardenLayerConfig config,
            Vector2 normalizedPosition)
        {
            this.index = index;
            this.config = config;
            basePosition = normalizedPosition;

            seedA = Hash01(index * 17 + 3);
            seedB = Hash01(index * 31 + 7);
            seedC = Hash01(index * 47 + 11);
            seedD = Hash01(index * 61 + 19);
            seedE = Hash01(index * 79 + 23);

            Position = normalizedPosition;
            Radius = 0f;
            Color = Color.white;
            Alpha = 1f;
        }

        public void Refresh(float elapsedTime, float screenWidth, float screenHeight)
        {
            float pulsePhase =
                elapsedTime * config.PulseSpeed +
                seedA * Mathf.PI * 2f;

            float pulse =
                1f +
                Mathf.Sin(pulsePhase) *
                config.PulseStrength;

            float driftPhase =
                elapsedTime * config.BloomDrift +
                seedB * Mathf.PI * 2f;

            float drift =
                Mathf.Sin(driftPhase) *
                config.BloomOrbitStrength;

            float radiusT =
                Mathf.Clamp01(seedC + drift * 0.5f);

            Radius =
                Mathf.Lerp(
                    config.MinBloomRadius,
                    config.MaxBloomRadius,
                    radiusT) *
                pulse;

            float orbitPhase =
                seedD * Mathf.PI * 2f +
                elapsedTime * config.OrbitalSpeed;

            float orbitRadius =
                config.OrbitalAmount *
                (0.25f + seedE * 0.75f);

            Vector2 orbit =
                new Vector2(
                    Mathf.Cos(orbitPhase),
                    Mathf.Sin(orbitPhase)) *
                orbitRadius;

            float wobbleX =
                Mathf.Sin(
                    elapsedTime * config.BreathingSpeed +
                    seedA * 9.1f) *
                config.Wobble *
                0.025f;

            float wobbleY =
                Mathf.Cos(
                    elapsedTime * config.BreathingSpeed * 0.73f +
                    seedB * 7.7f) *
                config.Wobble *
                0.025f;

            Position =
                basePosition +
                orbit +
                new Vector2(wobbleX, wobbleY);

            float breathing =
                1f +
                Mathf.Sin(
                    elapsedTime * config.BreathingSpeed +
                    seedC * Mathf.PI * 2f) *
                config.BreathingAmount;

            Alpha =
                Mathf.Clamp01(
                    config.Opacity *
                    breathing);

            Color = EvaluateColor(elapsedTime);
        }

        private Color EvaluateColor(float elapsedTime)
        {
            if (config.BloomColors == null ||
                config.BloomColors.Count == 0)
            {
                return Color.white;
            }

            int count = config.BloomColors.Count;

            float position =
                Mathf.Repeat(
                    seedA +
                    elapsedTime * config.HueDrift * 0.1f,
                    1f);

            float scaled = position * count;

            int a = Mathf.FloorToInt(scaled) % count;
            int b = (a + 1) % count;

            float t = scaled - Mathf.Floor(scaled);

            Color color =
                Color.Lerp(
                    config.BloomColors[a],
                    config.BloomColors[b],
                    t);

            float glow =
                1f +
                Mathf.Clamp01(config.Glow / 60f);

            color *= glow;
            color.a = Alpha;

            return color;
        }

        private static float Hash01(int value)
        {
            unchecked
            {
                uint x = (uint)value;

                x ^= x >> 16;
                x *= 0x7feb352d;
                x ^= x >> 15;
                x *= 0x846ca68b;
                x ^= x >> 16;

                return x / (float)uint.MaxValue;
            }
        }
    }
}