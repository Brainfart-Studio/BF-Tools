using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
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
            lookup = new Dictionary<string, BFHapticsEntry>();
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
            StartCoroutine(StopAfter(duration));
        }

        private IEnumerator StopAfter(float duration)
        {
            yield return new WaitForSeconds(duration);
            Gamepad.current?.SetMotorSpeeds(0f, 0f);
        }
    }
}