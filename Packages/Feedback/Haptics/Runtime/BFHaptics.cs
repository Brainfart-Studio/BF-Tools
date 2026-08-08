using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using BFTools.Core.ConfigLookup;
using BFTools.Core.EventBus;
using BFTools.Core.Logger;

namespace BFTools.Feedback.Haptics
{
    public struct BFHapticsEvent
    {
        public string eventName;
    }

    public class BFHaptics : MonoBehaviour
    {
        private const string LogTag = "Haptics";

        [SerializeField] private List<BFHapticsConfig> configs = new List<BFHapticsConfig>();
        private Dictionary<string, BFHapticsEntry> lookup;
        private Coroutine activeTrigger;

        private void OnEnable()
        {
            BuildLookup();
            EventBus<BFHapticsEvent>.Subscribe(OnHapticsEvent);
        }

        private void OnDisable()
        {
            EventBus<BFHapticsEvent>.Unsubscribe(OnHapticsEvent);
        }

        private void BuildLookup()
        {
            lookup = BFConfigLookupBuilder.Merge(MergedEntries(), entry => entry.eventName, LogTag, name, context: this);
        }

        private IEnumerable<BFHapticsEntry> MergedEntries()
        {
            foreach (var cfg in configs)
            {
                if (cfg == null)
                    continue;
                foreach (var entry in cfg.Entries)
                    yield return entry;
            }
        }

        private void OnHapticsEvent(BFHapticsEvent evt)
        {
            if (lookup == null || !lookup.TryGetValue(evt.eventName, out BFHapticsEntry entry))
            {
                BFLogger.Trace(LogTag, $"No haptics entry found for eventName '{evt.eventName}'.", this);
                return;
            }
            Trigger(entry.intensity, entry.duration);
        }

        private void Trigger(float intensity, float duration)
        {
            var gamepad = Gamepad.current;
            if (gamepad == null)
            {
                BFLogger.Trace(LogTag, "No gamepad connected, skipping haptics trigger.", this);
                return;
            }
            gamepad.SetMotorSpeeds(intensity, intensity);
            BFLogger.Trace(LogTag, $"Triggered haptics intensity={intensity} duration={duration}", this);

            if (activeTrigger != null)
                StopCoroutine(activeTrigger);
            activeTrigger = StartCoroutine(StopAfter(duration));
        }

        private IEnumerator StopAfter(float duration)
        {
            yield return new WaitForSeconds(duration);
            activeTrigger = null;
            Gamepad.current?.SetMotorSpeeds(0f, 0f);
        }
    }
}