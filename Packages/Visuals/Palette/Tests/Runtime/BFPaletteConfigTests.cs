using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using BFTools.Core.EventBus;
using BFTools.Core.Logger;
using BFTools.Core.Logger.TestUtilities;
using BFTools.Visuals.Palette;

namespace BFTools.Visuals.Palette.Tests
{
    public class BFPaletteConfigTests
    {
        private List<Object> createdAssets;

        [SetUp]
        public void SetUp()
        {
            createdAssets = new List<Object>();
        }

        [TearDown]
        public void TearDown()
        {
            EventBus<BFPaletteConfigChangedEvent>.Clear();

            foreach (Object asset in createdAssets)
                Object.DestroyImmediate(asset);

            BFLoggerTestUtility.ResetState();
        }

        private BFPaletteConfig CreateConfig(params (string name, Color color)[] entries)
        {
            BFPaletteConfig config = ScriptableObject.CreateInstance<BFPaletteConfig>();
            createdAssets.Add(config);

            SerializedObject serializedConfig = new SerializedObject(config);
            SerializedProperty entriesProperty = serializedConfig.FindProperty("entries");

            for (int i = 0; i < entries.Length; i++)
            {
                entriesProperty.InsertArrayElementAtIndex(i);
                SerializedProperty element = entriesProperty.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("name").stringValue = entries[i].name;
                element.FindPropertyRelative("color").colorValue = entries[i].color;
            }

            serializedConfig.ApplyModifiedPropertiesWithoutUndo();
            return config;
        }

        private static void InvokeOnValidate(BFPaletteConfig config)
        {
            typeof(BFPaletteConfig)
                .GetMethod("OnValidate", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(config, null);
        }

        private static SpyLoggerSink InitializeLogging()
        {
            BFLoggerConfig loggerConfig = ScriptableObject.CreateInstance<BFLoggerConfig>();
            typeof(BFLoggerConfig)
                .GetField("globalMinimumLevel", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(loggerConfig, LogLevel.Trace);

            SpyLoggerSink spy = new SpyLoggerSink();
            BFLogger.Initialize(loggerConfig, spy);
            return spy;
        }

        [Test]
        public void Entries_ReflectsConfiguredList()
        {
            BFPaletteConfig config = CreateConfig(("Idle", Color.white), ("Hit", Color.red));

            Assert.AreEqual(2, config.Entries.Count);
            Assert.AreEqual("Idle", config.Entries[0].name);
            Assert.AreEqual("Hit", config.Entries[1].name);
        }

        [Test]
        public void OnValidate_DuplicateEntryNames_LogsWarning()
        {
            SpyLoggerSink spy = InitializeLogging();
            BFPaletteConfig config = CreateConfig(("Idle", Color.white), ("Idle", Color.red));

            InvokeOnValidate(config);

            Assert.IsTrue(spy.Entries.Exists(e => e.Level == LogLevel.Warning && e.Message.Contains("Duplicate entry name 'Idle'")));
        }

        [Test]
        public void OnValidate_NoDuplicateEntryNames_DoesNotLogWarning()
        {
            SpyLoggerSink spy = InitializeLogging();
            BFPaletteConfig config = CreateConfig(("Idle", Color.white), ("Hit", Color.red));

            InvokeOnValidate(config);

            Assert.IsFalse(spy.Entries.Exists(e => e.Level == LogLevel.Warning));
        }

        [Test]
        public void OnValidate_FiresConfigChangedEventForItself()
        {
            BFPaletteConfig config = CreateConfig(("Idle", Color.white));

            BFPaletteConfig receivedConfig = null;
            EventBus<BFPaletteConfigChangedEvent>.Subscribe(evt => receivedConfig = evt.config);

            InvokeOnValidate(config);

            Assert.AreEqual(config, receivedConfig);
        }
    }
}