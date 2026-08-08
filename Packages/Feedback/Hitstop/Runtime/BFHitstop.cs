using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BFTools.Core.EventBus;
using BFTools.Core.Logger;

namespace BFTools.Feedback.Hitstop
{
    public struct BFHitstopEvent
    {
        public string eventName;
    }

    public class BFHitstop : MonoBehaviour
    {
        private const string LogTag = "Hitstop";

        [SerializeField] private List<BFHitstopConfig> configs = new List<BFHitstopConfig>();
        private Dictionary<string, BFHitstopEntry> lookup;
        private Coroutine activeHitstop;

        private void OnEnable()
        {
            BuildLookup();
            EventBus<BFHitstopEvent>.Subscribe(OnHitstopEvent);
        }

        private void OnDisable()
        {
            EventBus<BFHitstopEvent>.Unsubscribe(OnHitstopEvent);
            CancelActiveHitstop();
        }

        private void CancelActiveHitstop()
        {
            if (activeHitstop == null)
                return;

            StopCoroutine(activeHitstop);
            activeHitstop = null;
            Time.timeScale = 1f;
        }

        private void BuildLookup()
        {
            lookup = new Dictionary<string, BFHitstopEntry>();
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

        private void OnHitstopEvent(BFHitstopEvent evt)
        {
            if (lookup == null || !lookup.TryGetValue(evt.eventName, out BFHitstopEntry entry))
            {
                BFLogger.Trace(LogTag, $"No hitstop entry found for eventName '{evt.eventName}'.", this);
                return;
            }
            Trigger(entry.timescale, entry.duration);
        }

        private void Trigger(float timescale, float duration)
        {
            CancelActiveHitstop();

            Time.timeScale = timescale;
            BFLogger.Trace(LogTag, $"Triggered hitstop timescale={timescale} duration={duration}", this);
            activeHitstop = StartCoroutine(RestoreAfter(duration));
        }

        private IEnumerator RestoreAfter(float duration)
        {
            yield return new WaitForSecondsRealtime(duration);
            Time.timeScale = 1f;
            activeHitstop = null;
        }
    }
}