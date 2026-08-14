using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace BFTools.Feedback.Vignette
{
    public class BFVignetteLayer
    {
        private const int ProfileSteps = 1024;

        private readonly Image image;

        private Texture2D bakedTexture;
        private Sprite bakedSprite;
        private float bakedRadius = -1f;
        private float bakedSoftness = -1f;
        private float bakedRoundness = -1f;
        private float bakedAspect = -1f;
        private float[] bakedProfileAngles;
        private float[] bakedProfileValues;
        private bool bakedUseGradient;
        private Gradient bakedGradient;

        private float[] profileAngles;
        private float[] profileValues;
        private float rolledFrequency = float.NaN;
        private float rolledWaveCrest = float.NaN;
        private float rolledWaveTrough = float.NaN;
        private float rolledSpacingVariance = float.NaN;
        private float rolledWaveHeightVariance = float.NaN;
        private float rolledJaggedness = float.NaN;
        private int waveSeed;

        public BFVignetteLayer(Image image)
        {
            this.image = image;
        }

        public IEnumerator Play(BFVignetteEntry entry, Func<BFVignetteEntry, BFVignetteEntry> resolveLiveEntry)
        {
            waveSeed = UnityEngine.Random.Range(0, 1000000);
            RollWaveProfile(entry);
            ApplyMask(entry);

            float t = 0f;
            while (t < entry.duration)
            {
                entry = resolveLiveEntry(entry);

                ApplyMask(entry);

                t += Time.deltaTime;
                float curveValue = entry.intensityCurve != null && entry.intensityCurve.length > 0
                    ? entry.intensityCurve.Evaluate(t / entry.duration)
                    : 1f;
                float alpha = entry.intensity * Mathf.Clamp01(curveValue);
                image.color = TintFor(entry, alpha);
                yield return null;
            }

            image.color = TintFor(entry, 0f);
        }

        public void Cancel()
        {
            if (image == null)
                return;

            Color c = image.color;
            image.color = new Color(c.r, c.g, c.b, 0f);
        }

        public void DestroyBakedAssets()
        {
            if (bakedSprite != null)
                UnityEngine.Object.Destroy(bakedSprite);

            if (bakedTexture != null)
                UnityEngine.Object.Destroy(bakedTexture);

            bakedSprite = null;
            bakedTexture = null;
        }

        private void RollWaveProfile(BFVignetteEntry entry)
        {
            int frequency = Mathf.Max(1, Mathf.RoundToInt(entry.frequency));
            bool perfectWave = Mathf.Approximately(entry.spacingVariance, 0f) && Mathf.Approximately(entry.waveHeightVariance, 0f) && Mathf.Approximately(entry.jaggedness, 0f);

            profileAngles = new float[ProfileSteps];
            profileValues = new float[ProfileSteps];

            float topBase = entry.waveCrest * 0.25f;
            float bottomBase = entry.waveTrough * 0.25f;
            float midBase = (topBase + bottomBase) * 0.5f;
            float halfSpreadBase = (topBase - bottomBase) * 0.5f;

            for (int i = 0; i < ProfileSteps; i++)
            {
                float theta = (i / (float)ProfileSteps) * Mathf.PI * 2f;
                profileAngles[i] = theta;
                profileValues[i] = perfectWave
                    ? midBase + Mathf.Sin(theta * frequency) * halfSpreadBase
                    : EvaluateVariableWave(theta, frequency, entry, topBase, bottomBase);
            }

            rolledFrequency = entry.frequency;
            rolledWaveCrest = entry.waveCrest;
            rolledWaveTrough = entry.waveTrough;
            rolledSpacingVariance = entry.spacingVariance;
            rolledWaveHeightVariance = entry.waveHeightVariance;
            rolledJaggedness = entry.jaggedness;
        }

        private float EvaluateVariableWave(float theta, int frequency, BFVignetteEntry entry, float topBase, float bottomBase)
        {
            var spacing = new float[frequency];
            var heightScales = new float[frequency];
            float total = 0f;

            for (int i = 0; i < frequency; i++)
            {
                float spacingValue = 1f;
                if (entry.spacingVariance > 0f)
                    spacingValue += (SeededRandom(i * 133 + frequency * 7 + waveSeed) - 0.5f) * entry.spacingVariance * 1.5f;

                spacing[i] = Mathf.Max(0.2f, spacingValue);
                total += spacing[i];

                float heightScaleValue = 1f;
                if (entry.waveHeightVariance > 0f)
                    heightScaleValue += (SeededRandom(i * 317 + frequency * 13 + waveSeed) - 0.5f) * entry.waveHeightVariance * 0.9f;

                heightScales[i] = Mathf.Max(0.1f, heightScaleValue);
            }

            float scale = frequency / total;
            float cursor = 0f;

            for (int i = 0; i < frequency; i++)
            {
                float span = (Mathf.PI * 2f / frequency) * spacing[i] * scale;
                if (theta >= cursor && theta < cursor + span)
                {
                    float progress = (theta - cursor) / span;

                    float currentTop = topBase * heightScales[i];
                    float currentBottom = bottomBase + (topBase - currentTop);
                    float mid = (currentTop + currentBottom) * 0.5f;
                    float halfSpread = (currentTop - currentBottom) * 0.5f;

                    float wave = mid + Mathf.Sin(progress * Mathf.PI * 2f) * halfSpread;

                    if (entry.jaggedness > 0f)
                    {
                        float noise = SeededRandom(Mathf.FloorToInt(theta * 79f) + frequency * 11 + waveSeed);
                        wave += (noise - 0.5f) * entry.jaggedness * Mathf.Abs(halfSpread);
                    }

                    return wave;
                }

                cursor += span;
            }

            return 0f;
        }

        private static float SeededRandom(int seed)
        {
            float value = Mathf.Sin(seed * 12.9898f) * 43758.5453f;
            return value - Mathf.Floor(value);
        }

        private bool WaveInputsChanged(BFVignetteEntry entry)
        {
            return profileAngles == null ||
                !Mathf.Approximately(rolledFrequency, entry.frequency) ||
                !Mathf.Approximately(rolledWaveCrest, entry.waveCrest) ||
                !Mathf.Approximately(rolledWaveTrough, entry.waveTrough) ||
                !Mathf.Approximately(rolledSpacingVariance, entry.spacingVariance) ||
                !Mathf.Approximately(rolledWaveHeightVariance, entry.waveHeightVariance) ||
                !Mathf.Approximately(rolledJaggedness, entry.jaggedness);
        }

        private void ApplyMask(BFVignetteEntry entry)
        {
            float aspect = (float)Screen.width / Mathf.Max(Screen.height, 1);

            if (WaveInputsChanged(entry))
                RollWaveProfile(entry);

            bool maskChanged = bakedTexture == null ||
                !Mathf.Approximately(bakedRadius, entry.radius) ||
                !Mathf.Approximately(bakedSoftness, entry.softness) ||
                !Mathf.Approximately(bakedRoundness, entry.roundness) ||
                !Mathf.Approximately(bakedAspect, aspect) ||
                !ReferenceEquals(bakedProfileAngles, profileAngles) ||
                !ReferenceEquals(bakedProfileValues, profileValues) ||
                bakedUseGradient != entry.useGradient ||
                (entry.useGradient && !ReferenceEquals(bakedGradient, entry.colorGradient));

            if (!maskChanged)
                return;

            DestroyBakedAssets();

            bakedTexture = BFVignetteTextureBaker.Bake(entry.radius, entry.softness, entry.roundness, aspect, profileAngles, profileValues, entry.useGradient, entry.colorGradient);
            bakedSprite = Sprite.Create(bakedTexture, new Rect(0f, 0f, bakedTexture.width, bakedTexture.height), Vector2.one * 0.5f);
            bakedRadius = entry.radius;
            bakedSoftness = entry.softness;
            bakedRoundness = entry.roundness;
            bakedAspect = aspect;
            bakedProfileAngles = profileAngles;
            bakedProfileValues = profileValues;
            bakedUseGradient = entry.useGradient;
            bakedGradient = entry.colorGradient;

            image.sprite = bakedSprite;
        }

        private static Color TintFor(BFVignetteEntry entry, float alpha)
        {
            return entry.useGradient
                ? new Color(1f, 1f, 1f, alpha)
                : new Color(entry.color.r, entry.color.g, entry.color.b, alpha);
        }
    }
}