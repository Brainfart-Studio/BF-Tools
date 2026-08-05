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
    public class BFSceneTransitionControllerTests
    {
        private GameObject go;
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
            EventBus<BFSceneLoadedEvent>.Clear();
            EventBus<BFSceneTransitionCompleteEvent>.Clear();
            BFServiceLocator.Unregister<BFSceneTransitionController>();

            if (go != null)
                Object.Destroy(go);

            foreach (Object obj in createdObjects)
                if (obj != null)
                    Object.Destroy(obj);

            BFLoggerTestUtility.ResetState();
        }

        private BFSceneTransitionController CreateController()
        {
            go = new GameObject("SceneTransitionController");
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
            return controller;
        }

        private BFSceneLoadRequest CreateRequest(string sceneName, bool showLoadingScreen = false)
        {
            BFSceneLoadRequest request = ScriptableObject.CreateInstance<BFSceneLoadRequest>();
            typeof(BFSceneLoadRequest)
                .GetField("sceneName", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(request, sceneName);
            typeof(BFSceneLoadRequest)
                .GetField("showLoadingScreen", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(request, showLoadingScreen);

            createdObjects.Add(request);
            return request;
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
        public void Awake_RegistersWithServiceLocator()
        {
            BFSceneTransitionController controller = CreateController();

            Assert.AreSame(controller, BFServiceLocator.Get<BFSceneTransitionController>());
        }

        [Test]
        public void OnDestroy_UnregistersFromServiceLocator()
        {
            CreateController();

            Object.DestroyImmediate(go);
            go = null;

            Assert.Throws<KeyNotFoundException>(() => BFServiceLocator.Get<BFSceneTransitionController>());
        }

        [Test]
        public void BeginTransition_NotAlreadyTransitioning_FiresStartedEventSynchronouslyWithRequestData()
        {
            BFSceneTransitionController controller = CreateController();
            BFSceneLoadRequest request = CreateRequest("Level1", showLoadingScreen: true);

            BFSceneTransitionStartedEvent? received = null;
            EventBus<BFSceneTransitionStartedEvent>.Subscribe(e => received = e);

            controller.BeginTransition(request);

            Assert.IsTrue(received.HasValue);
            Assert.AreEqual("Level1", received.Value.sceneName);
            Assert.IsTrue(received.Value.showLoadingScreen);
        }

        [Test]
        public void BeginTransition_AlreadyTransitioning_LogsWarningAndIgnoresSecondRequest()
        {
            SpyLoggerSink spy = InitializeLogging();
            BFSceneTransitionController controller = CreateController();
            BFSceneLoadRequest requestA = CreateRequest("Level1");
            BFSceneLoadRequest requestB = CreateRequest("Level2");

            int startedCount = 0;
            EventBus<BFSceneTransitionStartedEvent>.Subscribe(_ => startedCount++);

            controller.BeginTransition(requestA);
            controller.BeginTransition(requestB);

            Assert.AreEqual(1, startedCount);
            Assert.IsTrue(spy.Entries.Exists(e => e.Level == LogLevel.Warning && e.Message.Contains("requested while already transitioning")));
        }
    }
}