using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
        }

        private void OnDisable()
        {
            EventBus<BFScreenFlashEvent>.Unsubscribe(OnScreenFlashEvent);
        }

        private void BuildLookup()
        {
            lookup = new Dictionary<string, BFScreenFlashEntry>();
            foreach (var cfg in configs)
            {
                if (cfg == null)
                    continue;

                foreach (var entry in cfg.Entries)
                {
                    if (lookup.ContainsKey(entry.eventName))
                    {
                        BFLogger.Warning(LogTag,
                            $"Duplicate eventName '{entry.eventName}' across assigned configs on '{name}'. Last one wins.",
                            this);
                    }
                    lookup[entry.eventName] = entry;
                }
            }

            BFLogger.Debug(LogTag, $"Built lookup with {lookup.Count} entrie(s) on '{name}'.", this);
        }

        private void OnScreenFlashEvent(BFScreenFlashEvent evt)
        {
            if (lookup == null || !lookup.TryGetValue(evt.eventName, out BFScreenFlashEntry entry))
            {
                BFLogger.Trace(LogTag, $"No screen flash entry found for eventName '{evt.eventName}'.", this);
                return;
            }

            if (activeFlash != null)
                StopCoroutine(activeFlash);

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