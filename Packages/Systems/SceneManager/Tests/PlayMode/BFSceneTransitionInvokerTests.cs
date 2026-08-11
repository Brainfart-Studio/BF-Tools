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
    public class BFSceneTransitionInvokerTests
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
            // Invoke can start BFSceneTransitionController's real transition coroutine, which eventually
            // calls into BFSceneLoader with scene names that aren't in Build Settings. Stop it here so it
            // never resumes past this test into a scene load that would throw or bleed into a later test.
            if (controller != null)
                controller.StopAllCoroutines();

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

        private BFSceneTransitionInvoker CreateInvoker(BFSceneLoadRequest request)
        {
            GameObject go = new GameObject("SceneTransitionInvoker");
            go.SetActive(false);
            BFSceneTransitionInvoker invoker = go.AddComponent<BFSceneTransitionInvoker>();
            typeof(BFSceneTransitionInvoker)
                .GetField("request", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(invoker, request);
            go.SetActive(true);
            createdObjects.Add(go);
            return invoker;
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
        public void Invoke_RequestAssigned_BeginsTransitionWithConfiguredRequest()
        {
            SpyLoggerSink spy = InitializeLogging();
            CreateController();
            BFSceneLoadRequest request = CreateRequest("Level1");
            BFSceneTransitionInvoker invoker = CreateInvoker(request);

            string startedScene = null;
            EventBus<BFSceneTransitionStartedEvent>.Subscribe(e => startedScene = e.sceneName);

            invoker.Invoke();

            Assert.AreEqual("Level1", startedScene);
            Assert.IsTrue(spy.Entries.Exists(e => e.Message.Contains("Invoked")));
        }

        [Test]
        public void Invoke_NoRequestAssigned_LogsErrorAndDoesNotBeginTransition()
        {
            SpyLoggerSink spy = InitializeLogging();
            CreateController();
            BFSceneTransitionInvoker invoker = CreateInvoker(null);

            bool started = false;
            EventBus<BFSceneTransitionStartedEvent>.Subscribe(_ => started = true);

            invoker.Invoke();

            Assert.IsFalse(started);
            Assert.IsTrue(spy.Entries.Exists(e => e.Level == LogLevel.Error && e.Message.Contains("No BFSceneLoadRequest assigned")));
        }
    }
}