using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
#if UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_WSA
using UnityEngine.InputSystem.DualShock;
#endif
using BFTools.Core.ConfigLookup;
using BFTools.Core.EventBus;
using BFTools.Core.Logger;

namespace BFTools.Feedback.ControllerLED
{
    public struct BFControllerLedEvent
    {
        public string eventName;
    }

    public class BFControllerLedManager : MonoBehaviour
    {
        private const string LogTag = "ControllerLED";

        [SerializeField] private List<BFControllerLedConfig> configs = new List<BFControllerLedConfig>();

        private Dictionary<string, BFControllerLedEntry> lookup;
        private IBFControllerLed activeLed;

        private void OnEnable()
        {
            BuildLookup();
            InputSystem.onDeviceChange += OnDeviceChange;
            EventBus<BFControllerLedEvent>.Subscribe(OnControllerLedEvent);
            ResolveLed(Gamepad.current);
        }

        private void OnDisable()
        {
            InputSystem.onDeviceChange -= OnDeviceChange;
            EventBus<BFControllerLedEvent>.Unsubscribe(OnControllerLedEvent);
        }

        private void BuildLookup()
        {
            lookup = BFConfigLookupBuilder.Merge(MergedEntries(), entry => entry.eventName, LogTag, name, "eventName", this);
        }

        private IEnumerable<BFControllerLedEntry> MergedEntries()
        {
            foreach (var cfg in configs)
            {
                if (cfg == null)
                    continue;
                foreach (var entry in cfg.Entries)
                    yield return entry;
            }
        }

        private void OnDeviceChange(InputDevice device, InputDeviceChange change)
        {
            if (change != InputDeviceChange.Added && change != InputDeviceChange.Removed)
                return;

            ResolveLed(Gamepad.current);
        }

        private void OnControllerLedEvent(BFControllerLedEvent evt)
        {
            if (lookup == null || !lookup.TryGetValue(evt.eventName, out BFControllerLedEntry entry))
            {
                BFLogger.Trace(LogTag, $"No ControllerLED entry found for eventName '{evt.eventName}'.", this);
                return;
            }
            SetColor(entry.color);
        }

        private void ResolveLed(Gamepad gamepad)
        {
#if UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_WSA
            if (gamepad is DualShock4GamepadHID dualShock)
            {
                activeLed = new BFDualShockLed(dualShock);
                BFLogger.Trace(LogTag, "Resolved DualShock4 LED support.", this);
                return;
            }
#endif
            activeLed = new BFNoOpLed();
            BFLogger.Trace(LogTag, "No supported LED controller found, using no-op.", this);
        }

        public void SetColor(Color color)
        {
            activeLed?.SetColor(color);
        }

        public void TurnOff()
        {
            activeLed?.TurnOff();
        }
    }
}