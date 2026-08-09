using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using NUnit.Framework;
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

            if (Directory.Exists(scratchDir))
                Directory.Delete(scratchDir, true);
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
        public void SaveAsync_SaveableCaptureStateThrows_ReturnsFalseInsteadOfThrowing()
        {
            BFSaveManager.Register(new ThrowingCaptureSaveable());

            bool saved = BFSaveManager.SaveAsync("Slot1", scratchDir).GetAwaiter().GetResult();

            Assert.IsFalse(saved);
        }
    }
}