using UnityEngine;
using UnityEngine.InputSystem;
#if UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_WSA
using UnityEngine.InputSystem.DualShock;
#endif
using BFTools.Core.Logger;

namespace BFTools.Feedback.ControllerLED
{
    public class BFControllerLedManager : MonoBehaviour
    {
        private const string LogTag = "ControllerLED";

        private IBFControllerLed activeLed;

        private void OnEnable()
        {
            InputSystem.onDeviceChange += OnDeviceChange;
            ResolveLed(Gamepad.current);
        }

        private void OnDisable()
        {
            InputSystem.onDeviceChange -= OnDeviceChange;
        }

        private void OnDeviceChange(InputDevice device, InputDeviceChange change)
        {
            if (change != InputDeviceChange.Added && change != InputDeviceChange.Removed)
                return;

            ResolveLed(Gamepad.current);
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