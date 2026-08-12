using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using BFTools.Core.EventBus;
using BFTools.Core.Logger;
using BFTools.Core.Logger.TestUtilities;
using BFTools.Core.ServiceLocator;
using BFTools.Core.SingletonGuard;
using BFTools.Systems.SceneManager;
using Assert = NUnit.Framework.Assert;

namespace BFTools.Systems.SceneManager.PlayModeTests
{
    public class BFSceneTransitionControllerTests
    {
        private GameObject go;
        private BFSceneTransitionController controller;
        private List<Object> createdObjects;

        [SetUp]
        public void SetUp()
        {
            createdObjects = new List<Object>();
        }

        [TearDown]
        public void TearDown()
        {
            // BeginTransition starts a real coroutine that eventually calls into BFSceneLoader with
            // scene names that aren't in Build Settings. Stop it here so it never resumes past this
            // test into a scene load that would throw or bleed into a later test.
            if (controller != null)
                controller.StopAllCoroutines();

            EventBus<BFSceneTransitionStartedEvent>.Clear();
            EventBus<BFSceneLoadedEvent>.Clear();
            EventBus<BFSceneTransitionCompleteEvent>.Clear();
            BFServiceLocator.Unregister<BFSceneTransitionController>();
            BFActiveInstanceGuard<BFSceneTransitionController>.ResetState();

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
            return controller;
        }

        private BFSceneTransitionController CreateSecondaryController()
        {
            GameObject secondaryGo = new GameObject("SecondarySceneTransitionController");
            secondaryGo.SetActive(false);

            BFSceneTransitionController secondaryController = secondaryGo.AddComponent<BFSceneTransitionController>();

            secondaryGo.SetActive(true);
            createdObjects.Add(secondaryGo);
            return secondaryController;
        }

        private static BFSceneLoadRequest CreateRequest(string sceneName, bool showLoadingScreen = false)
        {
            BFSceneLoadRequest request = new BFSceneLoadRequest();
            typeof(BFSceneLoadRequest)
                .GetField("sceneName", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(request, sceneName);
            typeof(BFSceneLoadRequest)
                .GetField("showLoadingScreen", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(request, showLoadingScreen);

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
            controller = CreateController();

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
        public void Awake_DuplicateInstance_LogsWarningAndDoesNotBecomeActiveInstance()
        {
            SpyLoggerSink spy = InitializeLogging();
            controller = CreateController();

            BFSceneTransitionController duplicate = CreateSecondaryController();

            Assert.IsTrue(spy.Entries.Exists(e => e.Level == LogLevel.Warning && e.Message.Contains("Duplicate BFSceneTransitionController")));
            Assert.IsFalse(BFActiveInstanceGuard<BFSceneTransitionController>.IsActive(duplicate));
        }

        [Test]
        public void Awake_DuplicateInstance_OriginalRemainsRegistered()
        {
            controller = CreateController();

            CreateSecondaryController();

            Assert.AreSame(controller, BFServiceLocator.Get<BFSceneTransitionController>());
        }

        [Test]
        public void BeginTransition_NotAlreadyTransitioning_FiresStartedEventSynchronouslyWithRequestData()
        {
            controller = CreateController();
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
            controller = CreateController();
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