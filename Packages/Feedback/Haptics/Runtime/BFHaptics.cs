using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using BFTools.Core.EventBus;

namespace BFTools.Feedback.Haptics
{
    public struct BFHapticsEvent
    {
        public string eventName;
    }

    public class BFHaptics : MonoBehaviour
    {
        [SerializeField] private BFHapticsConfig config;

        private void OnEnable()
        {
            EventBus<BFHapticsEvent>.Subscribe(OnHapticsEvent);
        }

        private void OnDisable()
        {
            EventBus<BFHapticsEvent>.Unsubscribe(OnHapticsEvent);
        }

        private void OnHapticsEvent(BFHapticsEvent evt)
        {
            if (config == null)
                return;

            if (config.TryGetEntry(evt.eventName, out BFHapticsEntry entry))
            {
                Trigger(entry.intensity, entry.duration);
            }
        }

        private void Trigger(float intensity, float duration)
        {
            var gamepad = Gamepad.current;
            if (gamepad == null)
                return;

            gamepad.SetMotorSpeeds(intensity, intensity);
            StartCoroutine(StopAfter(duration));
        }

        private IEnumerator StopAfter(float duration)
        {
            yield return new WaitForSeconds(duration);
            Gamepad.current?.SetMotorSpeeds(0f, 0f);
        }
    }
}