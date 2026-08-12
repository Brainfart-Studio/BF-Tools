using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using BFTools.Core.ConfigLookup;
using BFTools.Core.EventBus;
using BFTools.Core.Logger;

namespace BFTools.Feedback.ScreenShake
{
    public struct BFScreenShakeEvent
    {
        public string eventName;
    }

    public class BFScreenShake : MonoBehaviour
    {
        private const string LogTag = "ScreenShake";

        [SerializeField] private List<BFScreenShakeConfig> configs = new List<BFScreenShakeConfig>();
        private Dictionary<string, BFScreenShakeEntry> lookup;
        private Transform target;
        private Vector3 originalPosition;
        private Coroutine activeShake;

        private void OnEnable()
        {
            BuildLookup();
            EventBus<BFScreenShakeEvent>.Subscribe(OnScreenShakeEvent);
            SceneManager.sceneLoaded += OnSceneLoaded;
            ResolveTarget();
        }

        private void OnDisable()
        {
            EventBus<BFScreenShakeEvent>.Unsubscribe(OnScreenShakeEvent);
            SceneManager.sceneLoaded -= OnSceneLoaded;
            CancelActiveShake();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ResolveTarget();
        }

        private void ResolveTarget()
        {
            CancelActiveShake();

            target = Camera.main != null ? Camera.main.transform : null;
            if (target == null)
                BFLogger.Warning(LogTag, "No main camera found to shake.", this);
        }

        private void CancelActiveShake()
        {
            if (activeShake == null)
                return;

            StopCoroutine(activeShake);
            activeShake = null;
            if (target != null)
                target.localPosition = originalPosition;
        }

        private void BuildLookup()
        {
            lookup = BFConfigLookupBuilder.Merge(MergedEntries(), entry => entry.eventName, LogTag, name, "eventName", this);
        }

        private IEnumerable<BFScreenShakeEntry> MergedEntries()
        {
            foreach (var cfg in configs)
            {
                if (cfg == null)
                    continue;
                foreach (var entry in cfg.Entries)
                    yield return entry;
            }
        }

        private void OnScreenShakeEvent(BFScreenShakeEvent evt)
        {
            if (target == null)
            {
                BFLogger.Trace(LogTag, $"No camera to shake for eventName '{evt.eventName}'.", this);
                return;
            }

            if (lookup == null || !lookup.TryGetValue(evt.eventName, out BFScreenShakeEntry entry))
            {
                BFLogger.Trace(LogTag, $"No screen shake entry found for eventName '{evt.eventName}'.", this);
                return;
            }

            Trigger(entry.amplitude, entry.duration);
        }

        private void Trigger(float amplitude, float duration)
        {
            BFLogger.Trace(LogTag, $"Triggered shake amplitude={amplitude} duration={duration}", this);

            CancelActiveShake();
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