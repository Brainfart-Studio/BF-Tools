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

        private BFPreloadZoneTrigger CreateTrigger(string playerTag = "Player")
        {
            GameObject go = new GameObject("PreloadZoneTrigger");
            go.SetActive(false);
            BFPreloadZoneTrigger trigger = go.AddComponent<BFPreloadZoneTrigger>();
            typeof(BFPreloadZoneTrigger)
                .GetField("playerTag", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(trigger, playerTag);
            go.SetActive(true);
            createdObjects.Add(go);
            return trigger;
        }

        private static void SetSceneName(BFPreloadZoneTrigger trigger, string sceneName)
        {
            object request = typeof(BFPreloadZoneTrigger)
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

        private static void SetSharedRequest(BFPreloadZoneTrigger trigger, BFSceneLoadRequestAsset asset)
        {
            typeof(BFPreloadZoneTrigger)
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
            BFPreloadZoneTrigger trigger = CreateTrigger();
            SetSceneName(trigger, "Level1");
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
            BFPreloadZoneTrigger trigger = CreateTrigger();
            SetSceneName(trigger, "Level1");
            Collider2D player = CreateCollider("Player");

            InvokeOnTriggerEnter2D(trigger, player);

            Assert.IsTrue(spy.Entries.Exists(e => e.Message.Contains("Preload zone entered by") && e.Message.Contains("Level1")));
            Assert.IsTrue(spy.Entries.Exists(e => e.Message.Contains("already loading or loaded")));
        }

        [Test]
        public void OnTriggerEnter2D_NoSceneNameConfigured_LogsErrorAndDoesNotPreload()
        {
            SpyLoggerSink spy = InitializeLogging();
            BFPreloadZoneTrigger trigger = CreateTrigger();
            Collider2D player = CreateCollider("Player");

            InvokeOnTriggerEnter2D(trigger, player);

            Assert.AreEqual(0, GetOperations().Count);
            Assert.IsTrue(spy.Entries.Exists(e => e.Level == LogLevel.Error && e.Message.Contains("No scene name configured")));
        }

        [Test]
        public void OnTriggerEnter2D_SharedRequestAndInlineRequestBothSet_PrefersSharedRequest()
        {
            BFPreloadZoneTrigger trigger = CreateTrigger();
            SetSceneName(trigger, "Level1");
            SetSharedRequest(trigger, CreateSharedRequestAsset("Level2"));
            Collider2D player = CreateCollider("Player");

            InvokeOnTriggerEnter2D(trigger, player);

            Assert.IsTrue(BFSceneLoader.IsTracked("Level2"));
            Assert.IsFalse(BFSceneLoader.IsTracked("Level1"));
        }
    }
}