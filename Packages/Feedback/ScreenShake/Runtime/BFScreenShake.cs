using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
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
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ResolveTarget();
        }

        private void ResolveTarget()
        {
            target = Camera.main != null ? Camera.main.transform : null;
            if (target == null)
                BFLogger.Warning(LogTag, "No main camera found to shake.", this);
        }

        private void BuildLookup()
        {
            lookup = new Dictionary<string, BFScreenShakeEntry>();
            foreach (var cfg in configs)
            {
                if (cfg == null)
                    continue;
                foreach (var entry in cfg.Entries)
                {
                    if (lookup.ContainsKey(entry.eventName))
                    {
                        BFLogger.Warning(LogTag,
                            $"Duplicate eventName '{entry.eventName}' across assigned configs on '{name}'. Last one wins.",
                            this);
                    }
                    lookup[entry.eventName] = entry;
                }
            }

            BFLogger.Debug(LogTag, $"Built lookup with {lookup.Count} entrie(s) on '{name}'.", this);
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