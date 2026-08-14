using BFTools.Core.ConfigLookup;
using BFTools.Core.EventBus;
using BFTools.Core.Logger;
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

        [SerializeField] private List<BFVignetteConfig> configs = new List<BFVignetteConfig>();
        [SerializeField] private Image vignetteImage;

        private Dictionary<string, BFVignetteEntry> lookup;
        private Coroutine activeVignette;
        private BFVignetteLayer layer;

        private void OnEnable()
        {
            BuildLookup();
            EventBus<BFVignetteEvent>.Subscribe(OnVignetteEvent);

            if (vignetteImage == null)
                BFLogger.Warning(LogTag, "No vignetteImage assigned; unable to render vignettes.", this);
            else
                layer = new BFVignetteLayer(vignetteImage);
        }

        private void OnDisable()
        {
            EventBus<BFVignetteEvent>.Unsubscribe(OnVignetteEvent);
            CancelActiveVignette();
        }

        private void OnDestroy()
        {
            layer?.DestroyBakedAssets();
        }

        private void CancelActiveVignette()
        {
            if (activeVignette == null)
                return;

            StopCoroutine(activeVignette);
            activeVignette = null;
            layer?.Cancel();
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
            if (layer == null)
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

            CancelActiveVignette();
            activeVignette = StartCoroutine(layer.Play(entry, current => ResolveLiveEntry(evt.eventName, current)));
        }

        private BFVignetteEntry ResolveLiveEntry(string eventName, BFVignetteEntry fallback)
        {
            return TryGetLiveEntry(eventName, out BFVignetteEntry liveEntry) ? liveEntry : fallback;
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
    }
}