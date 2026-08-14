using BFTools.Core.ConfigLookup;
using BFTools.Core.EventBus;
using BFTools.Core.Logger;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace BFTools.Feedback.Vignette
{
    public struct BFVignetteEvent
    {
        public string eventName;
    }

    public class BFVignette : MonoBehaviour
    {
        private const string LogTag = "Vignette";

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

            ApplyMask(entry);

            CancelActiveVignette();
            activeVignette = StartCoroutine(VignetteRoutine(entry));
        }

        private void ApplyMask(BFVignetteEntry entry)
        {
            float aspect = (float)Screen.width / Mathf.Max(Screen.height, 1);

            bool maskChanged = bakedTexture == null ||
                !Mathf.Approximately(bakedRadius, entry.radius) ||
                !Mathf.Approximately(bakedSoftness, entry.softness) ||
                !Mathf.Approximately(bakedRoundness, entry.roundness) ||
                !Mathf.Approximately(bakedAspect, aspect);

            if (!maskChanged)
                return;

            DestroyBakedAssets();

            bakedTexture = BFVignetteTextureBaker.Bake(entry.radius, entry.softness, entry.roundness, aspect);
            bakedSprite = Sprite.Create(bakedTexture, new Rect(0f, 0f, bakedTexture.width, bakedTexture.height), Vector2.one * 0.5f);
            bakedRadius = entry.radius;
            bakedSoftness = entry.softness;
            bakedRoundness = entry.roundness;
            bakedAspect = aspect;

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

        private IEnumerator VignetteRoutine(BFVignetteEntry entry)
        {
            float t = 0f;
            while (t < entry.duration)
            {
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