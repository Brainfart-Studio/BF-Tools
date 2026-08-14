using BFTools.Core.ConfigLookup;
using BFTools.Core.EventBus;
using BFTools.Core.Logger;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BFTools.Feedback.Vignette
{
    public struct BFVignetteEvent
    {
        public string eventName;
    }

    public class BFVignette : MonoBehaviour
    {
        private const string LogTag = "Vignette";
        private const int ProfileSteps = 1024;

        [SerializeField] private List<BFVignetteConfig> configs = new List<BFVignetteConfig>();
        [SerializeField] private Image vignetteImage;

        private Dictionary<string, BFVignetteEntry> lookup;
        private Coroutine activeVignette;

        private Texture2D bakedTexture;
        private Sprite bakedSprite;
        private float bakedRadius = -1f;
        private float bakedSoftness = -1f;
        private float bakedRoundness = -1f;
        private float bakedAspect = -1f;
        private float[] bakedProfileAngles;
        private float[] bakedProfileValues;

        private float[] profileAngles;
        private float[] profileValues;
        private float rolledFrequency = float.NaN;
        private float rolledAmplitude = float.NaN;
        private float rolledSpacingVariance = float.NaN;
        private float rolledAmplitudeVariance = float.NaN;
        private float rolledJaggedness = float.NaN;

        private void OnEnable()
        {
            BuildLookup();
            EventBus<BFVignetteEvent>.Subscribe(OnVignetteEvent);

            if (vignetteImage == null)
                BFLogger.Warning(LogTag, "No vignetteImage assigned; unable to render vignettes.", this);
        }

        private void OnDisable()
        {
            EventBus<BFVignetteEvent>.Unsubscribe(OnVignetteEvent);
            CancelActiveVignette();
        }

        private void OnDestroy()
        {
            DestroyBakedAssets();
        }

        private void CancelActiveVignette()
        {
            if (activeVignette == null)
                return;

            StopCoroutine(activeVignette);
            activeVignette = null;
            if (vignetteImage != null)
            {
                Color c = vignetteImage.color;
                vignetteImage.color = new Color(c.r, c.g, c.b, 0f);
            }
        }

        private void BuildLookup()
        {
            lookup = BFConfigLookupBuilder.Merge(MergedEntries(), entry => entry.eventName, LogTag, name, "eventName", this);
        }

        private IEnumerable<BFVignetteEntry> MergedEntries()
        {
            foreach (var cfg in configs)
            {
                if (cfg == null)
                    continue;

                foreach (var entry in cfg.Entries)
                    yield return entry;
            }
        }

        private void OnVignetteEvent(BFVignetteEvent evt)
        {
            if (vignetteImage == null)
            {
                BFLogger.Trace(LogTag, $"No vignetteImage assigned, skipping vignette trigger for eventName '{evt.eventName}'.", this);
                return;
            }

            if (lookup == null || !lookup.TryGetValue(evt.eventName, out BFVignetteEntry entry))
            {
                BFLogger.Trace(LogTag, $"No vignette entry found for eventName '{evt.eventName}'.", this);
                return;
            }

            if (entry.blendMode != BFVignetteBlendMode.AlphaBlend)
                BFLogger.Warning(LogTag, $"Blend mode '{entry.blendMode}' is not implemented yet; falling back to AlphaBlend for eventName '{evt.eventName}'.", this);

            RollWaveProfile(entry);
            ApplyMask(entry);

            CancelActiveVignette();
            activeVignette = StartCoroutine(VignetteRoutine(evt.eventName, entry));
        }

        private bool TryGetLiveEntry(string eventName, out BFVignetteEntry entry)
        {
            entry = default;
            bool found = false;

            foreach (var cfg in configs)
            {
                if (cfg != null && cfg.TryGetEntry(eventName, out BFVignetteEntry candidate))
                {
                    entry = candidate;
                    found = true;
                }
            }

            return found;
        }

        private void RollWaveProfile(BFVignetteEntry entry)
        {
            int frequency = Mathf.Max(1, Mathf.RoundToInt(entry.frequency));
            bool perfectWave = Mathf.Approximately(entry.spacingVariance, 0f) && Mathf.Approximately(entry.amplitudeVariance, 0f) && Mathf.Approximately(entry.jaggedness, 0f);

            profileAngles = new float[ProfileSteps];
            profileValues = new float[ProfileSteps];

            for (int i = 0; i < ProfileSteps; i++)
            {
                float theta = (i / (float)ProfileSteps) * Mathf.PI * 2f;
                profileAngles[i] = theta;
                profileValues[i] = perfectWave
                    ? Mathf.Sin(theta * frequency) * (entry.amplitude * 0.25f)
                    : EvaluateVariableWave(theta, frequency, entry);
            }

            rolledFrequency = entry.frequency;
            rolledAmplitude = entry.amplitude;
            rolledSpacingVariance = entry.spacingVariance;
            rolledAmplitudeVariance = entry.amplitudeVariance;
            rolledJaggedness = entry.jaggedness;
        }

        private float EvaluateVariableWave(float theta, int frequency, BFVignetteEntry entry)
        {
            var spacing = new float[frequency];
            var amplitudes = new float[frequency];
            float total = 0f;

            for (int i = 0; i < frequency; i++)
            {
                float spacingValue = 1f;
                if (entry.spacingVariance > 0f)
                    spacingValue += (SeededRandom(i * 133 + frequency * 7) - 0.5f) * entry.spacingVariance * 1.5f;

                spacing[i] = Mathf.Max(0.2f, spacingValue);
                total += spacing[i];

                float amplitudeValue = 1f;
                if (entry.amplitudeVariance > 0f)
                    amplitudeValue += (SeededRandom(i * 317 + frequency * 13) - 0.5f) * entry.amplitudeVariance * 1.8f;

                amplitudes[i] = Mathf.Max(0f, amplitudeValue);
            }

            float scale = frequency / total;
            float cursor = 0f;

            for (int i = 0; i < frequency; i++)
            {
                float span = (Mathf.PI * 2f / frequency) * spacing[i] * scale;
                if (theta >= cursor && theta < cursor + span)
                {
                    float progress = (theta - cursor) / span;
                    float wave = Mathf.Sin(progress * Mathf.PI * 2f) * entry.amplitude * 0.25f * amplitudes[i];

                    if (entry.jaggedness > 0f)
                    {
                        float noise = SeededRandom(Mathf.FloorToInt(theta * 79f) + frequency * 11);
                        wave += (noise - 0.5f) * entry.jaggedness * entry.amplitude * 0.35f;
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
                !Mathf.Approximately(rolledAmplitude, entry.amplitude) ||
                !Mathf.Approximately(rolledSpacingVariance, entry.spacingVariance) ||
                !Mathf.Approximately(rolledAmplitudeVariance, entry.amplitudeVariance) ||
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
                !ReferenceEquals(bakedProfileValues, profileValues);

            if (!maskChanged)
                return;

            DestroyBakedAssets();

            bakedTexture = BFVignetteTextureBaker.Bake(entry.radius, entry.softness, entry.roundness, aspect, profileAngles, profileValues);
            bakedSprite = Sprite.Create(bakedTexture, new Rect(0f, 0f, bakedTexture.width, bakedTexture.height), Vector2.one * 0.5f);
            bakedRadius = entry.radius;
            bakedSoftness = entry.softness;
            bakedRoundness = entry.roundness;
            bakedAspect = aspect;
            bakedProfileAngles = profileAngles;
            bakedProfileValues = profileValues;

            vignetteImage.sprite = bakedSprite;
        }

        private void DestroyBakedAssets()
        {
            if (bakedSprite != null)
                Destroy(bakedSprite);

            if (bakedTexture != null)
                Destroy(bakedTexture);

            bakedSprite = null;
            bakedTexture = null;
        }

        private IEnumerator VignetteRoutine(string eventName, BFVignetteEntry entry)
        {
            float t = 0f;
            while (t < entry.duration)
            {
                if (TryGetLiveEntry(eventName, out BFVignetteEntry liveEntry))
                    entry = liveEntry;

                ApplyMask(entry);

                t += Time.deltaTime;
                float curveValue = entry.intensityCurve != null && entry.intensityCurve.length > 0
                    ? entry.intensityCurve.Evaluate(t / entry.duration)
                    : 1f;
                float alpha = entry.intensity * Mathf.Clamp01(curveValue);
                vignetteImage.color = new Color(entry.color.r, entry.color.g, entry.color.b, alpha);
                yield return null;
            }

            vignetteImage.color = new Color(entry.color.r, entry.color.g, entry.color.b, 0f);
            activeVignette = null;
        }
    }
}