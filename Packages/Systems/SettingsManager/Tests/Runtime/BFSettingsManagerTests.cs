using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using BFTools.Core.Logger;
using BFTools.Core.Logger.TestUtilities;
using BFTools.Systems.SettingsManager;
using Assert = NUnit.Framework.Assert;

namespace BFTools.Systems.SettingsManager.Tests
{
    public class BFSettingsManagerTests
    {
        private class FakeState
        {
            public int value;
        }

        private class FakeSettingsProvider : ISettingsProvider
        {
            public FakeState State = new FakeState();

            public Type StateType => typeof(FakeState);
            public object CaptureState() => State;
            public void RestoreState(object state) => State = (FakeState)state;
        }

        private class OtherFakeSettingsProvider : ISettingsProvider
        {
            public FakeState State = new FakeState();

            public Type StateType => typeof(FakeState);
            public object CaptureState() => State;
            public void RestoreState(object state) => State = (FakeState)state;
        }

        private string scratchDir;

        [SetUp]
        public void SetUp()
        {
            ResetState();

            scratchDir = Path.Combine(Path.GetTempPath(), "BFSettingsManagerTests");
            if (Directory.Exists(scratchDir))
                Directory.Delete(scratchDir, true);
            Directory.CreateDirectory(scratchDir);
        }

        [TearDown]
        public void TearDown()
        {
            ResetState();

            if (Directory.Exists(scratchDir))
                Directory.Delete(scratchDir, true);

            BFLoggerTestUtility.ResetState();
        }

        private static void ResetState()
        {
            Type managerType = typeof(BFSettingsManager);

            List<ISettingsProvider> providers = (List<ISettingsProvider>)managerType
                .GetField("providers", BindingFlags.NonPublic | BindingFlags.Static)
                .GetValue(null);
            providers.Clear();

            HashSet<Type> registeredStateTypes = (HashSet<Type>)managerType
                .GetField("registeredStateTypes", BindingFlags.NonPublic | BindingFlags.Static)
                .GetValue(null);
            registeredStateTypes.Clear();
        }

        private static int GetProvidersCount()
        {
            List<ISettingsProvider> providers = (List<ISettingsProvider>)typeof(BFSettingsManager)
                .GetField("providers", BindingFlags.NonPublic | BindingFlags.Static)
                .GetValue(null);
            return providers.Count;
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
        public void Register_AddsProvider_Unregister_RemovesProvider()
        {
            FakeSettingsProvider provider = new FakeSettingsProvider();

            BFSettingsManager.Register(provider);
            Assert.AreEqual(1, GetProvidersCount());

            BFSettingsManager.Unregister(provider);
            Assert.AreEqual(0, GetProvidersCount());
        }

        [Test]
        public void Register_SameProviderTwice_OnlyAddedOnce()
        {
            FakeSettingsProvider provider = new FakeSettingsProvider();

            BFSettingsManager.Register(provider);
            BFSettingsManager.Register(provider);

            Assert.AreEqual(1, GetProvidersCount());
        }

        [Test]
        public async Task SaveAsync_LoadAsync_RoundTrip_PersistsAndRestoresRegisteredProviderState()
        {
            FakeSettingsProvider provider = new FakeSettingsProvider { State = new FakeState { value = 42 } };
            BFSettingsManager.Register(provider);

            await BFSettingsManager.SaveAsync(scratchDir);
            provider.State = new FakeState { value = 0 };

            bool loaded = await BFSettingsManager.LoadAsync(scratchDir);

            Assert.IsTrue(loaded);
            Assert.AreEqual(42, provider.State.value);
        }

        [Test]
        public async Task LoadAsync_NoSettingsFileExists_ReturnsFalseAndLogsTrace()
        {
            SpyLoggerSink spy = InitializeLogging();

            bool loaded = await BFSettingsManager.LoadAsync(scratchDir);

            Assert.IsFalse(loaded);
            Assert.IsTrue(spy.Entries.Exists(e => e.Message.Contains("No settings file found")));
        }

        [Test]
        public async Task LoadAsync_ProviderNotPresentInSavedFile_LeavesStateAsIsAndReturnsTrue()
        {
            FakeSettingsProvider savedProvider = new FakeSettingsProvider { State = new FakeState { value = 5 } };
            BFSettingsManager.Register(savedProvider);
            await BFSettingsManager.SaveAsync(scratchDir);
            BFSettingsManager.Unregister(savedProvider);

            OtherFakeSettingsProvider otherProvider = new OtherFakeSettingsProvider { State = new FakeState { value = 99 } };
            BFSettingsManager.Register(otherProvider);

            bool loaded = await BFSettingsManager.LoadAsync(scratchDir);

            Assert.IsTrue(loaded);
            Assert.AreEqual(99, otherProvider.State.value);
        }
    }
}