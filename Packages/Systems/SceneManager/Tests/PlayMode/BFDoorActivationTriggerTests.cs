using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using BFTools.Core.EventBus;
using BFTools.Core.Logger;
using BFTools.Core.Logger.TestUtilities;
using BFTools.Core.ServiceLocator;
using BFTools.Systems.SceneManager;
using Assert = NUnit.Framework.Assert;

namespace BFTools.Systems.SceneManager.PlayModeTests
{
    public class BFDoorActivationTriggerTests
    {
        private List<Object> createdObjects;
        private BFSceneTransitionController controller;

        [SetUp]
        public void SetUp()
        {
            createdObjects = new List<Object>();
        }

        [TearDown]
        public void TearDown()
        {
            // OnTriggerEnter2D can start BFSceneTransitionController's real transition coroutine, which
            // eventually calls into BFSceneLoader with scene names that aren't in Build Settings. Stop it
            // here so it never resumes past this test into a scene load that would throw or bleed into a
            // later test.
            if (controller != null)
                controller.StopAllCoroutines();

            EventBus<BFSceneTransitionStartedEvent>.Clear();
            BFServiceLocator.Unregister<BFSceneTransitionController>();

            foreach (Object obj in createdObjects)
                if (obj != null)
                    Object.Destroy(obj);

            BFLoggerTestUtility.ResetState();
        }

        private BFSceneTransitionController CreateController()
        {
            GameObject go = new GameObject("SceneTransitionController");
            go.SetActive(false);
            controller = go.AddComponent<BFSceneTransitionController>();

            GameObject fadeGo = new GameObject("FadeTransition");
            fadeGo.transform.SetParent(go.transform);
            CanvasGroup canvasGroup = fadeGo.AddComponent<CanvasGroup>();
            BFFadeTransition fadeTransition = fadeGo.AddComponent<BFFadeTransition>();
            typeof(BFFadeTransition)
                .GetField("canvasGroup", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(fadeTransition, canvasGroup);

            typeof(BFSceneTransitionController)
                .GetField("transition", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(controller, fadeTransition);

            go.SetActive(true);
            createdObjects.Add(go);
            return controller;
        }

        private BFDoorActivationTrigger CreateDoorTrigger(string playerTag = "Player")
        {
            GameObject go = new GameObject("DoorTrigger");
            go.SetActive(false);
            BFDoorActivationTrigger trigger = go.AddComponent<BFDoorActivationTrigger>();
            typeof(BFDoorActivationTrigger)
                .GetField("playerTag", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(trigger, playerTag);
            go.SetActive(true);
            createdObjects.Add(go);
            return trigger;
        }

        private static void SetSceneName(BFDoorActivationTrigger trigger, string sceneName)
        {
            object request = typeof(BFDoorActivationTrigger)
                .GetField("request", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(trigger);

            request.GetType()
                .GetField("sceneName", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(request, sceneName);
        }

        private BFSceneLoadRequestAsset CreateSharedRequestAsset(string sceneName)
        {
            BFSceneLoadRequestAsset asset = ScriptableObject.CreateInstance<BFSceneLoadRequestAsset>();
            object request = typeof(BFSceneLoadRequestAsset)
                .GetField("request", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(asset);

            request.GetType()
                .GetField("sceneName", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(request, sceneName);

            createdObjects.Add(asset);
            return asset;
        }

        private static void SetSharedRequest(BFDoorActivationTrigger trigger, BFSceneLoadRequestAsset asset)
        {
            typeof(BFDoorActivationTrigger)
                .GetField("sharedRequest", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(trigger, asset);
        }

        private Collider2D CreateCollider(string tag)
        {
            GameObject go = new GameObject("Collider");
            go.tag = tag;
            Collider2D collider = go.AddComponent<BoxCollider2D>();
            createdObjects.Add(go);
            return collider;
        }

        private static void InvokeOnTriggerEnter2D(BFDoorActivationTrigger trigger, Collider2D other)
        {
            typeof(BFDoorActivationTrigger)
                .GetMethod("OnTriggerEnter2D", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(trigger, new object[] { other });
        }

        private static void InvokeOnTriggerExit2D(BFDoorActivationTrigger trigger, Collider2D other)
        {
            typeof(BFDoorActivationTrigger)
                .GetMethod("OnTriggerExit2D", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(trigger, new object[] { other });
        }

        private static bool GetSuppressed(BFDoorActivationTrigger trigger)
        {
            return (bool)typeof(BFDoorActivationTrigger)
                .GetField("suppressed", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(trigger);
        }

        private static SpyLoggerSink InitializeLogging()
        {
            BFLoggerConfig config = ScriptableObject.CreateInstance<BFLoggerConfig>();
            typeof(BFLoggerConfig)
                .GetField("globalMinimumLevel", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(config, LogLevel.Trace);

            SpyLoggerSink spy = new SpyLoggerSink();
            BFLogger.Initialize(config, spy);
            return spy;
        }

        [Test]
        public void OnTriggerEnter2D_PlayerTag_BeginsTransitionWithConfiguredRequest()
        {
            SpyLoggerSink spy = InitializeLogging();
            CreateController();
            BFDoorActivationTrigger trigger = CreateDoorTrigger();
            SetSceneName(trigger, "Level1");
            Collider2D player = CreateCollider("Player");

            string startedScene = null;
            EventBus<BFSceneTransitionStartedEvent>.Subscribe(e => startedScene = e.sceneName);

            InvokeOnTriggerEnter2D(trigger, player);

            Assert.AreEqual("Level1", startedScene);
            Assert.IsTrue(spy.Entries.Exists(e => e.Message.Contains("Door entered by")));
        }

        [Test]
        public void OnTriggerEnter2D_NonPlayerTag_DoesNotBeginTransition()
        {
            CreateController();
            BFDoorActivationTrigger trigger = CreateDoorTrigger();
            SetSceneName(trigger, "Level1");
            Collider2D other = CreateCollider("Untagged");

            bool started = false;
            EventBus<BFSceneTransitionStartedEvent>.Subscribe(_ => started = true);

            InvokeOnTriggerEnter2D(trigger, other);

            Assert.IsFalse(started);
        }

        [Test]
        public void OnTriggerEnter2D_CalledTwiceWithoutExit_SuppressesSecondTrigger()
        {
            CreateController();
            BFDoorActivationTrigger trigger = CreateDoorTrigger();
            SetSceneName(trigger, "Level1");
            Collider2D player = CreateCollider("Player");

            int startedCount = 0;
            EventBus<BFSceneTransitionStartedEvent>.Subscribe(_ => startedCount++);

            InvokeOnTriggerEnter2D(trigger, player);
            InvokeOnTriggerEnter2D(trigger, player);

            Assert.AreEqual(1, startedCount);
        }

        [Test]
        public void OnTriggerExit2D_PlayerTag_ClearsSuppressionAllowingReentry()
        {
            CreateController();
            BFDoorActivationTrigger trigger = CreateDoorTrigger();
            SetSceneName(trigger, "Level1");
            Collider2D player = CreateCollider("Player");

            InvokeOnTriggerEnter2D(trigger, player);
            Assert.IsTrue(GetSuppressed(trigger));

            InvokeOnTriggerExit2D(trigger, player);
            Assert.IsFalse(GetSuppressed(trigger));
        }

        [Test]
        public void OnTriggerEnter2D_NoSceneNameConfigured_LogsErrorAndDoesNotBeginTransition()
        {
            SpyLoggerSink spy = InitializeLogging();
            CreateController();
            BFDoorActivationTrigger trigger = CreateDoorTrigger();
            Collider2D player = CreateCollider("Player");

            bool started = false;
            EventBus<BFSceneTransitionStartedEvent>.Subscribe(_ => started = true);

            InvokeOnTriggerEnter2D(trigger, player);

            Assert.IsFalse(started);
            Assert.IsTrue(spy.Entries.Exists(e => e.Level == LogLevel.Error && e.Message.Contains("No scene name configured")));
        }

        [Test]
        public void OnTriggerEnter2D_SharedRequestAndInlineRequestBothSet_PrefersSharedRequest()
        {
            CreateController();
            BFDoorActivationTrigger trigger = CreateDoorTrigger();
            SetSceneName(trigger, "Level1");
            SetSharedRequest(trigger, CreateSharedRequestAsset("Level2"));
            Collider2D player = CreateCollider("Player");

            string startedScene = null;
            EventBus<BFSceneTransitionStartedEvent>.Subscribe(e => startedScene = e.sceneName);

            InvokeOnTriggerEnter2D(trigger, player);

            Assert.AreEqual("Level2", startedScene);
        }
    }
}