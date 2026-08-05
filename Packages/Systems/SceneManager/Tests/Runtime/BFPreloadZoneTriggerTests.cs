using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using BFTools.Core.Logger;
using BFTools.Core.Logger.TestUtilities;
using BFTools.Systems.SceneManager;
using Assert = NUnit.Framework.Assert;

namespace BFTools.Systems.SceneManager.Tests
{
    public class BFPreloadZoneTriggerTests
    {
        private List<Object> createdObjects;

        [SetUp]
        public void SetUp()
        {
            createdObjects = new List<Object>();
            GetOperations().Clear();
        }

        [TearDown]
        public void TearDown()
        {
            GetOperations().Clear();

            foreach (Object obj in createdObjects)
                if (obj != null)
                    Object.DestroyImmediate(obj);

            BFLoggerTestUtility.ResetState();
        }

        private static Dictionary<string, AsyncOperation> GetOperations()
        {
            return (Dictionary<string, AsyncOperation>)typeof(BFSceneLoader)
                .GetField("operations", BindingFlags.NonPublic | BindingFlags.Static)
                .GetValue(null);
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

        private BFPreloadZoneTrigger CreateTrigger(BFSceneLoadRequest request, string playerTag = "Player")
        {
            GameObject go = new GameObject("PreloadZoneTrigger");
            go.SetActive(false);
            BFPreloadZoneTrigger trigger = go.AddComponent<BFPreloadZoneTrigger>();
            typeof(BFPreloadZoneTrigger)
                .GetField("request", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(trigger, request);
            typeof(BFPreloadZoneTrigger)
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

        private static void InvokeOnTriggerEnter2D(BFPreloadZoneTrigger trigger, Collider2D other)
        {
            typeof(BFPreloadZoneTrigger)
                .GetMethod("OnTriggerEnter2D", BindingFlags.NonPublic | BindingFlags.Instance)
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
        public void OnTriggerEnter2D_NonPlayerTag_DoesNotPreload()
        {
            SpyLoggerSink spy = InitializeLogging();
            BFSceneLoadRequest request = CreateRequest("Level1");
            BFPreloadZoneTrigger trigger = CreateTrigger(request);
            Collider2D other = CreateCollider("Untagged");

            InvokeOnTriggerEnter2D(trigger, other);

            Assert.IsFalse(BFSceneLoader.IsTracked("Level1"));
            Assert.IsFalse(spy.Entries.Exists(e => e.Message.Contains("Preload zone entered")));
        }

        [Test]
        public void OnTriggerEnter2D_PlayerTag_SceneAlreadyTracked_DelegatesToSceneLoaderPreloadAndLogsBoth()
        {
            SpyLoggerSink spy = InitializeLogging();
            GetOperations()["Level1"] = null;
            BFSceneLoadRequest request = CreateRequest("Level1");
            BFPreloadZoneTrigger trigger = CreateTrigger(request);
            Collider2D player = CreateCollider("Player");

            InvokeOnTriggerEnter2D(trigger, player);

            Assert.IsTrue(spy.Entries.Exists(e => e.Message.Contains("Preload zone entered by") && e.Message.Contains("Level1")));
            Assert.IsTrue(spy.Entries.Exists(e => e.Message.Contains("already loading or loaded")));
        }
    }
}