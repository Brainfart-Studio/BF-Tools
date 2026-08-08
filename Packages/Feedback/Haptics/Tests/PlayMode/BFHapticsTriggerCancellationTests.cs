using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;
using BFTools.Core.EventBus;
using BFTools.Feedback.Haptics;
using Assert = NUnit.Framework.Assert;

namespace BFTools.Feedback.Haptics.PlayModeTests
{
    public class BFHapticsTriggerCancellationTests
    {
        private GameObject go;
        private Gamepad gamepad;
        private List<Object> createdAssets;

        [SetUp]
        public void SetUp()
        {
            createdAssets = new List<Object>();
            gamepad = InputSystem.AddDevice<Gamepad>();
        }

        [TearDown]
        public void TearDown()
        {
            EventBus<BFHapticsEvent>.Clear();

            if (gamepad != null)
                InputSystem.RemoveDevice(gamepad);

            if (go != null)
                Object.Destroy(go);

            foreach (Object asset in createdAssets)
                Object.Destroy(asset);
        }

        private BFHapticsConfig CreateConfig(params (string eventName, float intensity, float duration)[] entries)
        {
            BFHapticsConfig config = ScriptableObject.CreateInstance<BFHapticsConfig>();
            List<BFHapticsEntry> list = new List<BFHapticsEntry>();
            foreach (var e in entries)
                list.Add(new BFHapticsEntry { eventName = e.eventName, intensity = e.intensity, duration = e.duration });

            typeof(BFHapticsConfig)
                .GetField("entries", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(config, list);

            createdAssets.Add(config);
            return config;
        }

        private BFHaptics CreateHaptics(params BFHapticsConfig[] configs)
        {
            go = new GameObject("Haptics");
            go.SetActive(false);

            BFHaptics haptics = go.AddComponent<BFHaptics>();
            typeof(BFHaptics)
                .GetField("configs", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(haptics, new List<BFHapticsConfig>(configs));

            go.SetActive(true);
            return haptics;
        }

        private static object GetActiveTrigger(BFHaptics haptics)
        {
            return typeof(BFHaptics)
                .GetField("activeTrigger", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(haptics);
        }

        [UnityTest]
        public IEnumerator Fire_SecondTriggerBeforeFirstCompletes_CancelsFirstInsteadOfBeingCutShort()
        {
            BFHapticsConfig config = CreateConfig(("Short", 1f, 0.15f), ("Long", 0.3f, 0.5f));
            BFHaptics haptics = CreateHaptics(config);

            EventBus<BFHapticsEvent>.Fire(new BFHapticsEvent { eventName = "Short" });
            yield return new WaitForSecondsRealtime(0.05f);
            EventBus<BFHapticsEvent>.Fire(new BFHapticsEvent { eventName = "Long" });

            yield return new WaitForSecondsRealtime(0.2f);

            Assert.IsNotNull(GetActiveTrigger(haptics),
                "The 'Long' trigger's coroutine should still be running after 'Short's original duration elapsed; " +
                "'Short's stop-motors coroutine must have been cancelled when 'Long' fired, not left to run and clear the active trigger early.");

            yield return new WaitForSecondsRealtime(0.4f);

            Assert.IsNull(GetActiveTrigger(haptics));
        }
    }
}