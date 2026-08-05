using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using BFTools.Core.Logger;
using BFTools.Core.Logger.TestUtilities;
using BFTools.Systems.SceneManager;
using Assert = NUnit.Framework.Assert;

namespace BFTools.Systems.SceneManager.Tests
{
    public class BFSceneLoaderTests
    {
        [SetUp]
        public void SetUp()
        {
            GetOperations().Clear();
        }

        [TearDown]
        public void TearDown()
        {
            GetOperations().Clear();
            BFLoggerTestUtility.ResetState();
        }

        private static Dictionary<string, AsyncOperation> GetOperations()
        {
            return (Dictionary<string, AsyncOperation>)typeof(BFSceneLoader)
                .GetField("operations", BindingFlags.NonPublic | BindingFlags.Static)
                .GetValue(null);
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
        public async Task LoadAsync_SceneAlreadyTracked_LogsDebugAndReturnsCompletedTaskWithoutStartingNewLoad()
        {
            SpyLoggerSink spy = InitializeLogging();
            GetOperations()["Level1"] = null;

            Task task = BFSceneLoader.LoadAsync("Level1");
            await task;

            Assert.IsTrue(task.IsCompleted);
            Assert.IsTrue(spy.Entries.Exists(e => e.Message.Contains("already loading or loaded")));
        }

        [Test]
        public void Preload_SceneAlreadyTracked_LogsDebugAndDoesNotStartNewPreload()
        {
            SpyLoggerSink spy = InitializeLogging();
            GetOperations()["Level1"] = null;

            BFSceneLoader.Preload("Level1");

            Assert.IsTrue(spy.Entries.Exists(e => e.Message.Contains("already loading or loaded")));
            Assert.IsNull(GetOperations()["Level1"]);
        }

        [Test]
        public async Task ActivateAsync_SceneNotPreloaded_LogsErrorAndReturnsCompletedTask()
        {
            SpyLoggerSink spy = InitializeLogging();

            Task task = BFSceneLoader.ActivateAsync("Level1");
            await task;

            Assert.IsTrue(task.IsCompleted);
            Assert.IsTrue(spy.Entries.Exists(e => e.Level == LogLevel.Error && e.Message.Contains("not preloaded")));
        }

        [Test]
        public async Task UnloadAsync_SceneNotLoaded_LogsDebugAndSkips()
        {
            SpyLoggerSink spy = InitializeLogging();

            Task task = BFSceneLoader.UnloadAsync("NeverLoadedScene");
            await task;

            Assert.IsTrue(task.IsCompleted);
            Assert.IsTrue(spy.Entries.Exists(e => e.Message.Contains("is not loaded. Skipping unload")));
        }

        [Test]
        public async Task UnloadAsync_RemovesTrackedSceneFromOperationsRegardlessOfLoadState()
        {
            GetOperations()["Level1"] = null;

            await BFSceneLoader.UnloadAsync("Level1");

            Assert.IsFalse(BFSceneLoader.IsTracked("Level1"));
        }

        [Test]
        public void IsTracked_UntrackedScene_ReturnsFalse()
        {
            Assert.IsFalse(BFSceneLoader.IsTracked("Level1"));
        }

        [Test]
        public void IsTracked_TrackedScene_ReturnsTrue()
        {
            GetOperations()["Level1"] = null;

            Assert.IsTrue(BFSceneLoader.IsTracked("Level1"));
        }

        [Test]
        public void GetProgress_UntrackedScene_ReturnsOne()
        {
            Assert.AreEqual(1f, BFSceneLoader.GetProgress("Level1"));
        }
    }
}