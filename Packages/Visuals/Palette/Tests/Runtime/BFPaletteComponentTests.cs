using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using BFTools.Core.EventBus;
using BFTools.Core.Logger;
using BFTools.Core.Logger.TestUtilities;
using BFTools.Visuals.Palette;

namespace BFTools.Visuals.Palette.Tests
{
    public class BFPaletteComponentTests
    {
        private GameObject go;
        private List<Object> createdAssets;

        [SetUp]
        public void SetUp()
        {
            createdAssets = new List<Object>();
        }

        [TearDown]
        public void TearDown()
        {
            EventBus<BFPaletteEvent>.Clear();
            EventBus<BFPaletteConfigChangedEvent>.Clear();

            if (go != null)
                Object.DestroyImmediate(go);

            foreach (Object asset in createdAssets)
                Object.DestroyImmediate(asset);

            BFLoggerTestUtility.ResetState();
        }

        private static BFPaletteConfig CreateConfig(params (string name, Color color)[] entries)
        {
            BFPaletteConfig config = ScriptableObject.CreateInstance<BFPaletteConfig>();
            List<BFPaletteEntry> list = new List<BFPaletteEntry>();
            foreach (var e in entries)
                list.Add(new BFPaletteEntry { name = e.name, color = e.color });

            typeof(BFPaletteConfig)
                .GetField("entries", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(config, list);

            return config;
        }

        private BFPalette CreatePalette(BFPaletteConfig config, string selectedEntryName = null)
        {
            go = new GameObject("Palette");
            go.SetActive(false);

            BFPalette palette = go.AddComponent<BFPalette>();

            typeof(BFPalette)
                .GetField("config", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(palette, config);

            if (selectedEntryName != null)
            {
                typeof(BFPalette)
                    .GetField("selectedEntryName", BindingFlags.NonPublic | BindingFlags.Instance)
                    .SetValue(palette, selectedEntryName);
            }

            go.SetActive(true);
            return palette;
        }

        private SpriteRenderer GetRenderer()
        {
            return go.GetComponent<SpriteRenderer>();
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
        public void Select_MatchingEntryName_AppliesColorToRenderer()
        {
            BFPaletteConfig config = CreateConfig(("Idle", Color.white), ("Hit", Color.red));
            createdAssets.Add(config);
            BFPalette palette = CreatePalette(config);

            palette.Select("Hit");

            Assert.AreEqual(Color.red, GetRenderer().color);
        }

        [Test]
        public void Select_UnknownEntryName_DoesNotChangeColorAndLogsTrace()
        {
            SpyLoggerSink spy = InitializeLogging();
            BFPaletteConfig config = CreateConfig(("Idle", Color.white), ("Hit", Color.red));
            createdAssets.Add(config);
            BFPalette palette = CreatePalette(config);
            Color before = GetRenderer().color;

            palette.Select("Missing");

            Assert.AreEqual(before, GetRenderer().color);
            Assert.IsTrue(spy.Entries.Exists(e => e.Message.Contains("No palette entry found for name 'Missing'")));
        }

        [Test]
        public void Apply_NoConfigAssigned_LogsWarning()
        {
            SpyLoggerSink spy = InitializeLogging();
            BFPalette palette = CreatePalette(null);

            palette.Apply("Idle");

            Assert.IsTrue(spy.Entries.Exists(e => e.Level == LogLevel.Warning && e.Message.Contains("No config assigned")));
        }

        [Test]
        public void OnPaletteEvent_FiredEventName_AppliesMatchingColor()
        {
            BFPaletteConfig config = CreateConfig(("Idle", Color.white), ("Hit", Color.red));
            createdAssets.Add(config);
            CreatePalette(config);

            EventBus<BFPaletteEvent>.Fire(new BFPaletteEvent { eventName = "Hit" });

            Assert.AreEqual(Color.red, GetRenderer().color);
        }

        [Test]
        public void OnPaletteConfigChanged_MatchingConfigAndSeededSelection_AppliesSeededEntry()
        {
            BFPaletteConfig config = CreateConfig(("Idle", Color.white), ("Hit", Color.red));
            createdAssets.Add(config);
            CreatePalette(config, "Hit");

            EventBus<BFPaletteConfigChangedEvent>.Fire(new BFPaletteConfigChangedEvent { config = config });

            Assert.AreEqual(Color.red, GetRenderer().color);
        }

        [Test]
        public void OnPaletteConfigChanged_NonMatchingConfig_DoesNotReapply()
        {
            BFPaletteConfig config = CreateConfig(("Idle", Color.white), ("Hit", Color.red));
            BFPaletteConfig otherConfig = CreateConfig(("Hit", Color.red));
            createdAssets.Add(config);
            createdAssets.Add(otherConfig);
            CreatePalette(config, "Hit");
            Color before = GetRenderer().color;

            EventBus<BFPaletteConfigChangedEvent>.Fire(new BFPaletteConfigChangedEvent { config = otherConfig });

            Assert.AreEqual(before, GetRenderer().color);
        }

        [Test]
        public void OnDisable_UnsubscribesFromEventBus_SubsequentEventIsIgnored()
        {
            BFPaletteConfig config = CreateConfig(("Idle", Color.white), ("Hit", Color.red));
            createdAssets.Add(config);
            CreatePalette(config);
            Color before = GetRenderer().color;

            go.SetActive(false);

            EventBus<BFPaletteEvent>.Fire(new BFPaletteEvent { eventName = "Hit" });

            Assert.AreEqual(before, GetRenderer().color);
        }
    }
}