using UnityEngine;

namespace BFTools.Feedback.ControllerLED
{
    public class BFNoOpLed : IBFControllerLed
    {
        public bool IsSupported => false;

        public void SetColor(Color color)
        {
        }

        public void TurnOff()
        {
        }
    }
}