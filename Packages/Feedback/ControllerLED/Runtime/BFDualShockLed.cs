#if UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_WSA
using UnityEngine;
using UnityEngine.InputSystem.DualShock;

namespace BFTools.Feedback.ControllerLED
{
    public class BFDualShockLed : IBFControllerLed
    {
        private readonly DualShock4GamepadHID gamepad;

        public BFDualShockLed(DualShock4GamepadHID gamepad)
        {
            this.gamepad = gamepad;
        }

        public bool IsSupported => gamepad != null;

        public void SetColor(Color color)
        {
            gamepad?.SetLightBarColor(color);
        }

        public void TurnOff()
        {
            gamepad?.SetLightBarColor(Color.black);
        }
    }
}
#endif