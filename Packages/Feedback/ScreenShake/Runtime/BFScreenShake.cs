using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using BFTools.Core.EventBus;
namespace BFTools.Feedback.ScreenShake
{
    public struct BFScreenShakeEvent
    {
        public string eventName;
    }
    public class BFScreenShake : MonoBehaviour
    {
        [SerializeField] private BFScreenShakeConfig config;
        private Transform target;
        private Vector3 originalPosition;
        private Coroutine activeShake;
        private void OnEnable()
        {
            EventBus<BFScreenShakeEvent>.Subscribe(OnScreenShakeEvent);
            SceneManager.sceneLoaded += OnSceneLoaded;
            ResolveTarget();
        }
        private void OnDisable()
        {
            EventBus<BFScreenShakeEvent>.Unsubscribe(OnScreenShakeEvent);
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ResolveTarget();
        }
        private void ResolveTarget()
        {
            target = Camera.main != null ? Camera.main.transform : null;
        }
        private void OnScreenShakeEvent(BFScreenShakeEvent evt)
        {
            if (config == null || target == null)
                return;
            if (config.TryGetEntry(evt.eventName, out BFScreenShakeEntry entry))
            {
                Trigger(entry.amplitude, entry.duration);
            }
        }
        private void Trigger(float amplitude, float duration)
        {
            if (activeShake != null)
                StopCoroutine(activeShake);
            activeShake = StartCoroutine(ShakeRoutine(amplitude, duration));
        }
        private IEnumerator ShakeRoutine(float amplitude, float duration)
        {
            originalPosition = target.localPosition;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float x = (Random.value * 2f - 1f) * amplitude;
                float y = (Random.value * 2f - 1f) * amplitude;
                target.localPosition = originalPosition + new Vector3(x, y, 0f);
                elapsed += Time.deltaTime;
                yield return null;
            }
            target.localPosition = originalPosition;
            activeShake = null;
        }
    }
}