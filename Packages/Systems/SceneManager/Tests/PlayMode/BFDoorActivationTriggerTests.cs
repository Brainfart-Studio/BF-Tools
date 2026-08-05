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

        [SetUp]
        public void SetUp()
        {
            createdObjects = new List<Object>();
        }

        [TearDown]
        public void TearDown()
        {
            EventBus<BFSceneTransitionStartedEvent>.Clear();
            BFServiceLocator.Unregister<BFSceneTransitionController>();

            foreach (Object obj in createdObjects)
                if (obj != null)
                    Object.Destroy(obj);

            BFLoggerTestUtility.ResetState();
        }

        private BFSceneLoadRequest CreateRequest(string sceneName)
        {
            BFSceneLoadRequest request = ScriptableObject.CreateInstance<BFSceneLoadRequest>();
            typeof(BFSceneLoadRequest)
                .GetField("sceneName", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(request, sceneName);
            createdObjects.Add(request);
            return request;
        }

        private BFSceneTransitionController CreateController()
        {
            GameObject go = new GameObject("SceneTransitionController");
            go.SetActive(false);
            BFSceneTransitionController controller = go.AddComponent<BFSceneTransitionController>();

            GameObject fadeGo = new GameObject("FadeTransition");
            fadeGo.transform.SetParent(go.transform);
            CanvasGroup canvasGroup = fadeGo.AddComponent<CanvasGroup>();
            BFFadeTransition fadeTransition = fadeGo.AddComponent<BFFadeTransition>();
            typeof(BFFadeTransition)
                .GetField("canvasGroup", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(fadeTransition, canvasGroup);

            typeof(BFSceneTransitionController)
                .GetField("fadeTransition", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(controller, fadeTransition);

            go.SetActive(true);
            createdObjects.Add(go);
            return controller;
        }

        private BFDoorActivationTrigger CreateDoorTrigger(BFSceneLoadRequest request, string playerTag = "Player")
        {
            GameObject go = new GameObject("DoorTrigger");
            go.SetActive(false);
            BFDoorActivationTrigger trigger = go.AddComponent<BFDoorActivationTrigger>();
            typeof(BFDoorActivationTrigger)
                .GetField("request", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(trigger, request);
            typeof(BFDoorActivationTrigger)
                .GetField("playerTag", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(trigger, playerTag);
            go.SetActive(true);
            createdObjects.Add(go);
            return trigger;
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
            BFSceneLoadRequest request = CreateRequest("Level1");
            BFDoorActivationTrigger trigger = CreateDoorTrigger(request);
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
            BFSceneLoadRequest request = CreateRequest("Level1");
            BFDoorActivationTrigger trigger = CreateDoorTrigger(request);
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
            BFSceneLoadRequest request = CreateRequest("Level1");
            BFDoorActivationTrigger trigger = CreateDoorTrigger(request);
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
            BFSceneLoadRequest request = CreateRequest("Level1");
            BFDoorActivationTrigger trigger = CreateDoorTrigger(request);
            Collider2D player = CreateCollider("Player");

            int startedCount = 0;
            EventBus<BFSceneTransitionStartedEvent>.Subscribe(_ => startedCount++);

            InvokeOnTriggerEnter2D(trigger, player);
            InvokeOnTriggerExit2D(trigger, player);
            InvokeOnTriggerEnter2D(trigger, player);

            Assert.AreEqual(2, startedCount);
        }
    }
}