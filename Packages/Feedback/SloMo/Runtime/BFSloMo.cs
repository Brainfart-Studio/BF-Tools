using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BFTools.Core.ConfigLookup;
using BFTools.Core.EventBus;
using BFTools.Core.Logger;
using UnityEngine.UIElements;

namespace BFTools.Feedback.SloMo
{
    public struct BFSloMoEvent
    {
        public string eventName;
    }

    public class BFSloMo : MonoBehaviour
    {
        private const string LogTag = "SloMo";

        [SerializeField] private List<BFSloMoConfig> configs = new List<BFSloMoConfig>();
        private Dictionary<string, BFSloMoEntry> lookup;
        private Coroutine activeSloMo;

        private void OnEnable()
        {
            BuildLookup();
            EventBus<BFSloMoEvent>.Subscribe(OnSloMoEvent);
        }

        private void OnDisable()
        {
            EventBus<BFSloMoEvent>.Unsubscribe(OnSloMoEvent);
            CancelActiveSloMo();
        }

        private void CancelActiveSloMo()
        {
            if (activeSloMo == null)
                return;

            StopCoroutine(activeSloMo);
            activeSloMo = null;
            Time.timeScale = 1f;
        }

        private void BuildLookup()
        {
            lookup = BFConfigLookupBuilder.Merge(MergedEntries(), entry => entry.eventName, LogTag, name, "eventName", this);
        }

        private IEnumerable<BFSloMoEntry> MergedEntries()
        {
            foreach (var cfg in configs)
            {
                if (cfg == null)
                    continue;
                foreach (var entry in cfg.Entries)
                    yield return entry;
            }
        }

        private void OnSloMoEvent(BFSloMoEvent evt)
        {
            if (lookup == null || !lookup.TryGetValue(evt.eventName, out BFSloMoEntry entry))
            {
                BFLogger.Trace(LogTag, $"No slo-mo entry found for eventName '{evt.eventName}'.", this);
                return;
            }
            Trigger(entry);
        }

        private void Trigger(BFSloMoEntry entry)
        {
            CancelActiveSloMo();

            BFLogger.Trace(LogTag, $"Triggered slo-mo eventName={entry.eventName} duration={entry.duration}", this);
            activeSloMo = StartCoroutine(RunCurve(entry));
        }

        private IEnumerator RunCurve(BFSloMoEntry entry)
        {
            float elapsed = 0f;
            while (elapsed < entry.duration)
            {
                Time.timeScale = entry.curve.Evaluate(elapsed);
                yield return null;
                elapsed += Time.unscaledDeltaTime;
            }

            Time.timeScale = 1f;
            activeSloMo = null;
        }
    }
}