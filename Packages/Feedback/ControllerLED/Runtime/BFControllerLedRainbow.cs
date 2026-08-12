using UnityEngine;

namespace BFTools.Feedback.ControllerLED
{
    public class BFControllerLedRainbow : MonoBehaviour
    {
        [SerializeField] private BFControllerLedManager ledManager;
        [SerializeField] private float cyclesPerSecond = 0.2f;

        private float hue;

        private void Update()
        {
            hue = (hue + cyclesPerSecond * Time.deltaTime) % 1f;
            ledManager.SetColor(Color.HSVToRGB(hue, 1f, 1f));
        }
    }
}