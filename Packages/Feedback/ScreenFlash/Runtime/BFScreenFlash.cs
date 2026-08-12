using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BFTools.Core.ConfigLookup;
using BFTools.Core.EventBus;
using BFTools.Core.Logger;

namespace BFTools.Feedback.ScreenFlash
{
    public struct BFScreenFlashEvent
    {
        public string eventName;
    }

    public class BFScreenFlash : MonoBehaviour
    {
        private const string LogTag = "ScreenFlash";

        [SerializeField] private List<BFScreenFlashConfig> configs = new List<BFScreenFlashConfig>();
        [SerializeField] private Image flashImage;

        private Dictionary<string, BFScreenFlashEntry> lookup;
        private Coroutine activeFlash;

        private void OnEnable()
        {
            BuildLookup();
            EventBus<BFScreenFlashEvent>.Subscribe(OnScreenFlashEvent);

            if (flashImage == null)
                BFLogger.Warning(LogTag, "No flashImage assigned; unable to render flashes.", this);
        }

        private void OnDisable()
        {
            EventBus<BFScreenFlashEvent>.Unsubscribe(OnScreenFlashEvent);
            CancelActiveFlash();
        }

        private void CancelActiveFlash()
        {
            if (activeFlash == null)
                return;

            StopCoroutine(activeFlash);
            activeFlash = null;
            if (flashImage != null)
            {
                Color c = flashImage.color;
                flashImage.color = new Color(c.r, c.g, c.b, 0f);
            }
        }

        private void BuildLookup()
        {
            lookup = BFConfigLookupBuilder.Merge(MergedEntries(), entry => entry.eventName, LogTag, name, "eventName", this);
        }

        private IEnumerable<BFScreenFlashEntry> MergedEntries()
        {
            foreach (var cfg in configs)
            {
                if (cfg == null)
                    continue;

                foreach (var entry in cfg.Entries)
                    yield return entry;
            }
        }

        private void OnScreenFlashEvent(BFScreenFlashEvent evt)
        {
            if (flashImage == null)
            {
                BFLogger.Trace(LogTag, $"No flashImage assigned, skipping screen flash trigger for eventName '{evt.eventName}'.", this);
                return;
            }

            if (lookup == null || !lookup.TryGetValue(evt.eventName, out BFScreenFlashEntry entry))
            {
                BFLogger.Trace(LogTag, $"No screen flash entry found for eventName '{evt.eventName}'.", this);
                return;
            }

            CancelActiveFlash();
            activeFlash = StartCoroutine(FlashRoutine(entry));
        }

        private IEnumerator FlashRoutine(BFScreenFlashEntry entry)
        {
            for (int i = 0; i < entry.flashCount; i++)
            {
                float t = 0f;
                while (t < entry.duration)
                {
                    t += Time.deltaTime;
                    float alpha = 1f - (t / entry.duration);
                    flashImage.color = new Color(entry.flashColor.r, entry.flashColor.g, entry.flashColor.b, alpha);
                    yield return null;
                }
            }

            flashImage.color = new Color(entry.flashColor.r, entry.flashColor.g, entry.flashColor.b, 0f);
            activeFlash = null;
        }
    }
}