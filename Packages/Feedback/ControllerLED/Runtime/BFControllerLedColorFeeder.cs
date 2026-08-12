using UnityEngine;
using BFTools.Core.ScreenColorSampler;

namespace BFTools.Feedback.ControllerLED
{
    public class BFControllerLedColorFeeder : MonoBehaviour
    {
        [SerializeField] private BFScreenColorSampler colorSampler;
        [SerializeField] private BFControllerLedManager ledManager;

        private void OnEnable()
        {
            colorSampler.ColorSampled += OnColorSampled;
        }

        private void OnDisable()
        {
            colorSampler.ColorSampled -= OnColorSampled;
        }

        private void OnColorSampled(Color color)
        {
            ledManager.SetColor(color);
        }
    }
}