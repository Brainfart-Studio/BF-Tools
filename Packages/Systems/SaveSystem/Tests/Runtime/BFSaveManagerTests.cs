using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using BFTools.Core.Logger;
using BFTools.Core.Logger.TestUtilities;
using BFTools.Systems.SaveSystem;
using Assert = NUnit.Framework.Assert;

namespace BFTools.Systems.SaveSystem.Tests
{
    public class BFSaveManagerTests
    {
        private class FakeState
        {
            public int value;
        }

        private class FakeSaveable : ISaveable
        {
            public FakeState State = new FakeState();

            public Type StateType => typeof(FakeState);
            public object CaptureState() => State;
            public void RestoreState(object state) => State = (FakeState)state;
        }

        private class ThrowingCaptureSaveable : ISaveable
        {
            public Type StateType => typeof(FakeState);
            public object CaptureState() => throw new InvalidOperationException("Capture failed");
            public void RestoreState(object state) { }
        }

        private string scratchDir;

        [SetUp]
        public void SetUp()
        {
            ResetState();

            scratchDir = Path.Combine(Path.GetTempPath(), "BFSaveManagerTests");
            if (Directory.Exists(scratchDir))
                Directory.Delete(scratchDir, true);
            Directory.CreateDirectory(scratchDir);
        }

        [TearDown]
        public void TearDown()
        {
            ResetState();
            BFLoggerTestUtility.ResetState();

            if (Directory.Exists(scratchDir))
                Directory.Delete(scratchDir, true);
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

        private static void ResetState()
        {
            Type managerType = typeof(BFSaveManager);

            object saveablesRegistry = managerType
                .GetField("saveables", BindingFlags.NonPublic | BindingFlags.Static)
                .GetValue(null);
            ClearRegistry(saveablesRegistry);

            List<BFSaveSlot> slots = (List<BFSaveSlot>)managerType
                .GetField("slots", BindingFlags.NonPublic | BindingFlags.Static)
                .GetValue(null);
            slots.Clear();

            object slotOperationLocks = managerType
                .GetField("slotOperationLocks", BindingFlags.NonPublic | BindingFlags.Static)
                .GetValue(null);
            slotOperationLocks.GetType().GetMethod("Clear").Invoke(slotOperationLocks, null);

            object playtimeTrackers = managerType
                .GetField("playtimeTrackers", BindingFlags.NonPublic | BindingFlags.Static)
                .GetValue(null);
            playtimeTrackers.GetType().GetMethod("Clear").Invoke(playtimeTrackers, null);
        }

        private static void ClearRegistry(object registry)
        {
            Type registryType = registry.GetType();

            object items = registryType
                .GetField("items", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(registry);
            items.GetType().GetMethod("Clear").Invoke(items, null);

            object registeredStateTypes = registryType
                .GetField("registeredStateTypes", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(registry);
            registeredStateTypes.GetType().GetMethod("Clear").Invoke(registeredStateTypes, null);
        }

        private static int GetSaveablesCount()
        {
            object registry = typeof(BFSaveManager)
                .GetField("saveables", BindingFlags.NonPublic | BindingFlags.Static)
                .GetValue(null);

            object items = registry.GetType()
                .GetField("items", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(registry);

            return (int)items.GetType().GetProperty("Count").GetValue(items);
        }

        [Test]
        public void Register_AddsSaveable_Unregister_RemovesSaveable()
        {
            FakeSaveable saveable = new FakeSaveable();

            BFSaveManager.Register(saveable);
            Assert.AreEqual(1, GetSaveablesCount());

            BFSaveManager.Unregister(saveable);
            Assert.AreEqual(0, GetSaveablesCount());
        }

        [Test]
        public void Register_SameSaveableTwice_OnlyAddedOnce()
        {
            FakeSaveable saveable = new FakeSaveable();

            BFSaveManager.Register(saveable);
            BFSaveManager.Register(saveable);

            Assert.AreEqual(1, GetSaveablesCount());
        }

        [Test]
        public void RegisterSlot_NewSlotName_AddsSlot()
        {
            BFSaveManager.RegisterSlot(new BFSaveSlot { slotName = "Slot1", metadata = default });

            bool found = BFSaveManager.TryGetSlot("Slot1", out BFSaveSlot slot);

            Assert.IsTrue(found);
            Assert.AreEqual("Slot1", slot.slotName);
        }

        [Test]
        public void RegisterSlot_ExistingSlotName_UpdatesInPlace()
        {
            BFSaveManager.RegisterSlot(new BFSaveSlot { slotName = "Slot1", metadata = new BFSaveMetadata { version = 1 } });
            BFSaveManager.RegisterSlot(new BFSaveSlot { slotName = "Slot1", metadata = new BFSaveMetadata { version = 2 } });

            BFSaveManager.TryGetSlot("Slot1", out BFSaveSlot slot);

            Assert.AreEqual(2, slot.metadata.version);
        }

        [Test]
        public void TryGetSlot_UnknownSlotName_ReturnsFalse()
        {
            bool found = BFSaveManager.TryGetSlot("Missing", out _);

            Assert.IsFalse(found);
        }

        [Test]
        public void GetFileNameForSlot_ReturnsExpectedFormat()
        {
            string fileName = BFSaveManager.GetFileNameForSlot("Slot1");

            Assert.AreEqual("save_Slot1.dat", fileName);
        }

        [Test]
        public void GetFileNameForSlot_NullOrEmptySlotName_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => BFSaveManager.GetFileNameForSlot(null));
            Assert.Throws<ArgumentException>(() => BFSaveManager.GetFileNameForSlot(string.Empty));
            Assert.Throws<ArgumentException>(() => BFSaveManager.GetFileNameForSlot("   "));
        }

        [Test]
        public void GetFileNameForSlot_PathTraversalSlotName_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => BFSaveManager.GetFileNameForSlot("../Slot1"));
            Assert.Throws<ArgumentException>(() => BFSaveManager.GetFileNameForSlot("Slot1/../../etc"));
        }

        [Test]
        public void GetFileNameForSlot_SlotNameWithPathSeparators_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => BFSaveManager.GetFileNameForSlot("Sub/Slot1"));
            Assert.Throws<ArgumentException>(() => BFSaveManager.GetFileNameForSlot("Sub\\Slot1"));
        }

        [Test]
        public void SaveAsync_LoadAsync_RoundTrip_PersistsAndRestoresRegisteredSaveableState()
        {
            FakeSaveable saveable = new FakeSaveable { State = new FakeState { value = 42 } };
            BFSaveManager.Register(saveable);

            BFSaveManager.SaveAsync("Slot1", scratchDir).GetAwaiter().GetResult();
            saveable.State = new FakeState { value = 0 };

            bool loaded = BFSaveManager.LoadAsync("Slot1", scratchDir).GetAwaiter().GetResult();

            Assert.IsTrue(loaded);
            Assert.AreEqual(42, saveable.State.value);
            Assert.IsTrue(BFSaveManager.TryGetSlot("Slot1", out BFSaveSlot slot));
            Assert.AreEqual(BFSaveVersionMigrator.CurrentVersion, slot.metadata.version);
        }

        [Test]
        public void LoadAsync_NoSaveFileExists_ReturnsFalse()
        {
            bool loaded = BFSaveManager.LoadAsync("MissingSlot", scratchDir).GetAwaiter().GetResult();

            Assert.IsFalse(loaded);
        }

        [Test]
        public void LoadAsync_ChecksumMismatch_LogsErrorLevelInsteadOfWarning()
        {
            BFSaveManager.Register(new FakeSaveable());
            BFSaveManager.SaveAsync("Slot1", scratchDir).GetAwaiter().GetResult();

            string dataPath = Path.Combine(scratchDir, BFSaveManager.GetFileNameForSlot("Slot1"));
            byte[] dataBytes = File.ReadAllBytes(dataPath);
            dataBytes[dataBytes.Length - 1] ^= 0xFF;
            File.WriteAllBytes(dataPath, dataBytes);

            SpyLoggerSink spy = InitializeLogging();

            bool loaded = BFSaveManager.LoadAsync("Slot1", scratchDir).GetAwaiter().GetResult();

            Assert.IsFalse(loaded);
            Assert.IsTrue(spy.Entries.Exists(e => e.Level == LogLevel.Error && e.Message.Contains("Checksum mismatch")), "Tamper/corruption detection should log at Error, not Warning.");
        }

        [Test]
        public void LoadAsync_MissingCompanionFile_LogsErrorLevelInsteadOfWarning()
        {
            BFSaveManager.Register(new FakeSaveable());
            BFSaveManager.SaveAsync("Slot1", scratchDir).GetAwaiter().GetResult();

            string checksumPath = Path.Combine(scratchDir, BFSaveManager.GetFileNameForSlot("Slot1")) + ".chk";
            File.Delete(checksumPath);

            SpyLoggerSink spy = InitializeLogging();

            bool loaded = BFSaveManager.LoadAsync("Slot1", scratchDir).GetAwaiter().GetResult();

            Assert.IsFalse(loaded);
            Assert.IsTrue(spy.Entries.Exists(e => e.Level == LogLevel.Error && e.Message.Contains("missing its")), "An interrupted/incomplete save should log at Error, not Warning.");
        }

        [Test]
        public void SaveAsync_CalledTwiceForSameSlot_PlaytimeSecondsIncreasesInsteadOfStayingZero()
        {
            BFSaveManager.Register(new FakeSaveable());

            BFSaveManager.SaveAsync("Slot1", scratchDir).GetAwaiter().GetResult();
            BFSaveManager.TryGetSlot("Slot1", out BFSaveSlot firstSave);

            BFSaveManager.SaveAsync("Slot1", scratchDir).GetAwaiter().GetResult();
            BFSaveManager.TryGetSlot("Slot1", out BFSaveSlot secondSave);

            Assert.Greater(secondSave.metadata.playtimeSeconds, firstSave.metadata.playtimeSeconds);
        }

        [Test]
        public void SaveAsync_AfterLoadAsync_PlaytimeSecondsContinuesFromLoadedValueInsteadOfResettingToZero()
        {
            BFSaveManager.Register(new FakeSaveable());
            BFSaveManager.SaveAsync("Slot1", scratchDir).GetAwaiter().GetResult();
            BFSaveManager.TryGetSlot("Slot1", out BFSaveSlot savedSlot);

            BFSaveManager.LoadAsync("Slot1", scratchDir).GetAwaiter().GetResult();
            BFSaveManager.SaveAsync("Slot1", scratchDir).GetAwaiter().GetResult();
            BFSaveManager.TryGetSlot("Slot1", out BFSaveSlot resavedSlot);

            Assert.GreaterOrEqual(resavedSlot.metadata.playtimeSeconds, savedSlot.metadata.playtimeSeconds);
        }

        [Test]
        public void GetSlotNamesOnDisk_DirectoryDoesNotExist_ReturnsEmptyArray()
        {
            string missingDir = Path.Combine(scratchDir, "does-not-exist");

            string[] slotNames = BFSaveManager.GetSlotNamesOnDisk(missingDir);

            Assert.IsEmpty(slotNames);
        }

        [Test]
        public void GetSlotNamesOnDisk_NoSavesYet_ReturnsEmptyArray()
        {
            string[] slotNames = BFSaveManager.GetSlotNamesOnDisk(scratchDir);

            Assert.IsEmpty(slotNames);
        }

        [Test]
        public void GetSlotNamesOnDisk_AfterSavingMultipleSlots_ReturnsAllSlotNames()
        {
            BFSaveManager.Register(new FakeSaveable());
            BFSaveManager.SaveAsync("Slot1", scratchDir).GetAwaiter().GetResult();
            BFSaveManager.SaveAsync("Slot2", scratchDir).GetAwaiter().GetResult();

            string[] slotNames = BFSaveManager.GetSlotNamesOnDisk(scratchDir);

            CollectionAssert.AreEquivalent(new[] { "Slot1", "Slot2" }, slotNames);
        }

        [Test]
        public void GetSlotNamesOnDisk_IgnoresChecksumAndKeyFiles()
        {
            BFSaveManager.Register(new FakeSaveable());
            BFSaveManager.SaveAsync("Slot1", scratchDir).GetAwaiter().GetResult();
            File.WriteAllBytes(Path.Combine(scratchDir, "save.key"), new byte[] { 1 });

            string[] slotNames = BFSaveManager.GetSlotNamesOnDisk(scratchDir);

            CollectionAssert.AreEquivalent(new[] { "Slot1" }, slotNames);
        }

        [Test]
        public void DeleteSlot_ExistingSlot_RemovesFilesAndSlotEntryAndReturnsTrue()
        {
            BFSaveManager.Register(new FakeSaveable());
            BFSaveManager.SaveAsync("Slot1", scratchDir).GetAwaiter().GetResult();

            bool deleted = BFSaveManager.DeleteSlot("Slot1", scratchDir).GetAwaiter().GetResult();

            Assert.IsTrue(deleted);
            Assert.IsFalse(BFSaveManager.TryGetSlot("Slot1", out _));
            Assert.IsEmpty(BFSaveManager.GetSlotNamesOnDisk(scratchDir));

            bool loaded = BFSaveManager.LoadAsync("Slot1", scratchDir).GetAwaiter().GetResult();
            Assert.IsFalse(loaded);
        }

        [Test]
        public void DeleteSlot_UnknownSlot_ReturnsFalse()
        {
            bool deleted = BFSaveManager.DeleteSlot("MissingSlot", scratchDir).GetAwaiter().GetResult();

            Assert.IsFalse(deleted);
        }

        [Test]
        public void SaveAsync_SaveableCaptureStateThrows_ReturnsFalseInsteadOfThrowing()
        {
            BFSaveManager.Register(new ThrowingCaptureSaveable());

            bool saved = BFSaveManager.SaveAsync("Slot1", scratchDir).GetAwaiter().GetResult();

            Assert.IsFalse(saved);
        }

        [Test]
        public void SaveAsync_ConcurrentCallsForSameSlot_AllSucceedAndLeaveAConsistentFile()
        {
            FakeSaveable saveable = new FakeSaveable();
            BFSaveManager.Register(saveable);

            Task<bool>[] saveTasks = new Task<bool>[8];
            for (int i = 0; i < saveTasks.Length; i++)
            {
                saveable.State = new FakeState { value = i };
                saveTasks[i] = BFSaveManager.SaveAsync("Slot1", scratchDir);
            }

            bool[] results = Task.WhenAll(saveTasks).GetAwaiter().GetResult();

            Assert.IsTrue(Array.TrueForAll(results, result => result), "Every concurrent SaveAsync call for the same slot should succeed instead of losing the race on the shared temp file.");

            bool loaded = BFSaveManager.LoadAsync("Slot1", scratchDir).GetAwaiter().GetResult();

            Assert.IsTrue(loaded, "The save data and checksum files should remain a consistent pair after concurrent writes.");
        }

        [Test]
        public void LoadAsync_ConcurrentWithSaveAsyncForSameSlot_NeverReportsChecksumMismatch()
        {
            FakeSaveable saveable = new FakeSaveable { State = new FakeState { value = 1 } };
            BFSaveManager.Register(saveable);
            BFSaveManager.SaveAsync("Slot1", scratchDir).GetAwaiter().GetResult();

            Task<bool>[] tasks = new Task<bool>[6];
            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = i % 2 == 0
                    ? BFSaveManager.SaveAsync("Slot1", scratchDir)
                    : BFSaveManager.LoadAsync("Slot1", scratchDir);
            }

            bool[] results = Task.WhenAll(tasks).GetAwaiter().GetResult();

            Assert.IsTrue(Array.TrueForAll(results, result => result), "A load racing a save for the same slot should never see a torn data/checksum pair.");
        }
    }
}