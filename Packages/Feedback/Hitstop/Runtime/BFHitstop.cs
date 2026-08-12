using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BFTools.Core.ConfigLookup;
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
            lookup = BFConfigLookupBuilder.Merge(MergedEntries(), entry => entry.eventName, LogTag, name, "eventName", this);
        }

        private IEnumerable<BFHitstopEntry> MergedEntries()
        {
            foreach (var cfg in configs)
            {
                if (cfg == null)
                    continue;
                foreach (var entry in cfg.Entries)
                    yield return entry;
            }
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