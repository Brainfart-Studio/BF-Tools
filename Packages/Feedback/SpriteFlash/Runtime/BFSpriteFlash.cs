using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BFTools.Core.ConfigLookup;
using BFTools.Core.EventBus;
using BFTools.Core.Logger;

namespace BFTools.Feedback.SpriteFlash
{
    public struct BFSpriteFlashEvent
    {
        public string eventName;
    }

    [RequireComponent(typeof(SpriteRenderer))]
    public class BFSpriteFlash : MonoBehaviour
    {
        private const string LogTag = "SpriteFlash";

        [SerializeField] private List<BFSpriteFlashConfig> configs = new List<BFSpriteFlashConfig>();

        private SpriteRenderer targetRenderer;
        private Dictionary<string, BFSpriteFlashEntry> lookup;
        private Coroutine activeFlash;

        private void Awake()
        {
            targetRenderer = GetComponent<SpriteRenderer>();
        }

        private void OnEnable()
        {
            BuildLookup();
            EventBus<BFSpriteFlashEvent>.Subscribe(OnSpriteFlashEvent);
        }

        private void OnDisable()
        {
            EventBus<BFSpriteFlashEvent>.Unsubscribe(OnSpriteFlashEvent);
            CancelActiveFlash();
        }

        private void CancelActiveFlash()
        {
            if (activeFlash == null)
                return;

            StopCoroutine(activeFlash);
            activeFlash = null;
            GetTargetRenderer().enabled = true;
        }

        private void BuildLookup()
        {
            lookup = BFConfigLookupBuilder.Merge(MergedEntries(), entry => entry.eventName, LogTag, name, "eventName", this);
        }

        private IEnumerable<BFSpriteFlashEntry> MergedEntries()
        {
            foreach (var cfg in configs)
            {
                if (cfg == null)
                    continue;

                foreach (var entry in cfg.Entries)
                    yield return entry;
            }
        }

        private void OnSpriteFlashEvent(BFSpriteFlashEvent evt)
        {
            if (lookup == null || !lookup.TryGetValue(evt.eventName, out BFSpriteFlashEntry entry))
            {
                BFLogger.Trace(LogTag, $"No sprite flash entry found for eventName '{evt.eventName}'.", this);
                return;
            }

            CancelActiveFlash();
            activeFlash = StartCoroutine(FlashRoutine(entry));
        }

        private IEnumerator FlashRoutine(BFSpriteFlashEntry entry)
        {
            SpriteRenderer renderer = GetTargetRenderer();

            for (int i = 0; i < entry.flashCount; i++)
            {
                renderer.enabled = false;
                yield return new WaitForSeconds(entry.interval);
                renderer.enabled = true;
                yield return new WaitForSeconds(entry.interval);
            }

            activeFlash = null;
        }

        private SpriteRenderer GetTargetRenderer()
        {
            if (targetRenderer == null)
                targetRenderer = GetComponent<SpriteRenderer>();

            return targetRenderer;
        }
    }
}