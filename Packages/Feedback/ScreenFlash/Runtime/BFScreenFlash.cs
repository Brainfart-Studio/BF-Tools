using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using BFTools.Core.EventBus;
using BFTools.Core.Logger;

namespace BFTools.Feedback.ScreenFlash
{
    public struct BFScreenFlashEvent
    {
        public string eventName;
    }

    public class BFScreenFlash : MonoBehaviour
    {
        private const string LogTag = "ScreenFlash";

        [SerializeField] private BFScreenFlashConfig config;
        [SerializeField] private Image flashImage;

        private Coroutine activeFlash;

        private void OnEnable()
        {
            EventBus<BFScreenFlashEvent>.Subscribe(OnScreenFlashEvent);
        }

        private void OnDisable()
        {
            EventBus<BFScreenFlashEvent>.Unsubscribe(OnScreenFlashEvent);
        }

        private void OnScreenFlashEvent(BFScreenFlashEvent evt)
        {
            if (!config.TryGetEntry(evt.eventName, out BFScreenFlashEntry entry))
            {
                BFLogger.Warning(LogTag, $"No entry found for event '{evt.eventName}'");
                return;
            }

            if (activeFlash != null)
                StopCoroutine(activeFlash);

            activeFlash = StartCoroutine(FlashRoutine(entry));
        }

        private IEnumerator FlashRoutine(BFScreenFlashEntry entry)
        {
            for (int i = 0; i < entry.flashCount; i++)
            {
                float t = 0f;
                while (t < entry.duration)
                {
                    t += Time.deltaTime;
                    float alpha = 1f - (t / entry.duration);
                    flashImage.color = new Color(entry.flashColor.r, entry.flashColor.g, entry.flashColor.b, alpha);
                    yield return null;
                }
            }

            flashImage.color = new Color(entry.flashColor.r, entry.flashColor.g, entry.flashColor.b, 0f);
            activeFlash = null;
        }
    }
}