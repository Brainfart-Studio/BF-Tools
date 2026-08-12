using UnityEngine;

namespace BFTools.Feedback.ControllerLED
{
    public interface IBFControllerLed
    {
        bool IsSupported { get; }
        void SetColor(Color color);
        void TurnOff();
    }
}