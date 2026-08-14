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

        [SerializeField] private List<BFVignetteConfig> configs = new List<BFVignetteConfig>();
        [SerializeField] private List<Image> vignetteImages = new List<Image>();

        private Dictionary<string, BFVignetteEntry> lookup;
        private List<LayerSlot> slots;

        private void OnEnable()
        {
            BuildLookup();
            EventBus<BFVignetteEvent>.Subscribe(OnVignetteEvent);
            BuildSlots();
        }

        private void OnDisable()
        {
            EventBus<BFVignetteEvent>.Unsubscribe(OnVignetteEvent);
            CancelAllLayers();
        }

        private void OnDestroy()
        {
            if (slots == null)
                return;

            foreach (var slot in slots)
                slot.Layer.DestroyBakedAssets();
        }

        private void BuildSlots()
        {
            slots = new List<LayerSlot>();

            foreach (var image in vignetteImages)
            {
                if (image != null)
                    slots.Add(new LayerSlot(new BFVignetteLayer(image)));
            }

            if (slots.Count == 0)
                BFLogger.Warning(LogTag, "No vignette layer images assigned; unable to render vignettes.", this);
        }

        private void CancelAllLayers()
        {
            if (slots == null)
                return;

            foreach (var slot in slots)
            {
                if (slot.Routine == null)
                    continue;

                StopCoroutine(slot.Routine);
                slot.Routine = null;
                slot.Layer.Cancel();
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
            if (slots == null || slots.Count == 0)
            {
                BFLogger.Trace(LogTag, $"No vignette layers configured, skipping vignette trigger for eventName '{evt.eventName}'.", this);
                return;
            }

            if (lookup == null || !lookup.TryGetValue(evt.eventName, out BFVignetteEntry entry))
            {
                BFLogger.Trace(LogTag, $"No vignette entry found for eventName '{evt.eventName}'.", this);
                return;
            }

            if (entry.blendMode != BFVignetteBlendMode.AlphaBlend)
                BFLogger.Warning(LogTag, $"Blend mode '{entry.blendMode}' is not implemented yet; falling back to AlphaBlend for eventName '{evt.eventName}'.", this);

            LayerSlot slot = FindFreeOrOldestSlot();

            if (slot.Routine != null)
            {
                StopCoroutine(slot.Routine);
                slot.Layer.Cancel();
            }

            slot.StartTime = Time.time;
            slot.Routine = StartCoroutine(PlaySlot(slot, evt.eventName, entry));
        }

        private LayerSlot FindFreeOrOldestSlot()
        {
            LayerSlot oldest = null;

            foreach (var slot in slots)
            {
                if (slot.Routine == null)
                    return slot;

                if (oldest == null || slot.StartTime < oldest.StartTime)
                    oldest = slot;
            }

            return oldest;
        }

        private IEnumerator PlaySlot(LayerSlot slot, string eventName, BFVignetteEntry entry)
        {
            yield return slot.Layer.Play(entry, current => ResolveLiveEntry(eventName, current));
            slot.Routine = null;
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

        private class LayerSlot
        {
            public readonly BFVignetteLayer Layer;
            public Coroutine Routine;
            public float StartTime;

            public LayerSlot(BFVignetteLayer layer)
            {
                Layer = layer;
            }
        }
    }
}