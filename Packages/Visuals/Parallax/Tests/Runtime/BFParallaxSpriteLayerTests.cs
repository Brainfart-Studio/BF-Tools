using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using BFTools.Core.Logger;
using BFTools.Core.Logger.TestUtilities;
using BFTools.Visuals.Parallax;

namespace BFTools.Visuals.Parallax.Tests
{
    public class BFParallaxSpriteLayerTests
    {
        private GameObject parentGo;
        private List<Object> createdAssets;

        [SetUp]
        public void SetUp()
        {
            parentGo = new GameObject("Parent");
            createdAssets = new List<Object>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(parentGo);

            foreach (Object asset in createdAssets)
                if (asset != null)
                    Object.DestroyImmediate(asset);

            BFLoggerTestUtility.ResetState();
        }

        private Sprite CreateSprite()
        {
            Texture2D texture = new Texture2D(1, 1);
            Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f));
            createdAssets.Add(sprite);
            createdAssets.Add(texture);
            return sprite;
        }

        private BFParallaxSpriteLayerConfig CreateConfig()
        {
            BFParallaxSpriteLayerConfig config = ScriptableObject.CreateInstance<BFParallaxSpriteLayerConfig>();
            createdAssets.Add(config);
            return config;
        }

        private static void SetField(object target, string fieldName, object value)
        {
            target.GetType()
                .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(target, value);
        }

        private static T GetField<T>(object target, string fieldName)
        {
            return (T)target.GetType()
                .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(target);
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
        public void CreateLayer_ReturnsBFParallaxSpriteLayer()
        {
            BFParallaxSpriteLayerConfig config = CreateConfig();

            IBFParallaxLayer layer = config.CreateLayer();

            Assert.IsInstanceOf<BFParallaxSpriteLayer>(layer);
        }

        [Test]
        public void Init_NonLooping_CreatesSingleTileWithSpriteAndSortingOrder()
        {
            BFParallaxSpriteLayerConfig config = CreateConfig();
            Sprite sprite = CreateSprite();
            SetField(config, "sprite", sprite);
            SetField(config, "sortingOrderOffset", 2);

            BFParallaxSpriteLayer layer = new BFParallaxSpriteLayer(config);
            layer.Init(parentGo.transform, 5);

            Assert.AreEqual(1, parentGo.transform.childCount);
            Transform root = parentGo.transform.GetChild(0);
            Assert.AreEqual(1, root.childCount);
            SpriteRenderer tile = root.GetChild(0).GetComponent<SpriteRenderer>();
            Assert.AreEqual(sprite, tile.sprite);
            Assert.AreEqual(7, tile.sortingOrder);
        }

        [Test]
        public void Init_BothAxesLooping_CreatesNineTiles()
        {
            BFParallaxSpriteLayerConfig config = CreateConfig();
            SetField(config, "horizontalLooping", true);
            SetField(config, "verticalLooping", true);

            BFParallaxSpriteLayer layer = new BFParallaxSpriteLayer(config);
            layer.Init(parentGo.transform, 0);

            Transform root = parentGo.transform.GetChild(0);
            Assert.AreEqual(9, root.childCount);
        }

        [Test]
        public void Init_NoSpriteAssigned_LogsWarning()
        {
            SpyLoggerSink spy = InitializeLogging();
            BFParallaxSpriteLayerConfig config = CreateConfig();

            BFParallaxSpriteLayer layer = new BFParallaxSpriteLayer(config);
            layer.Init(parentGo.transform, 0);

            Assert.IsTrue(spy.Entries.Exists(e => e.Level == LogLevel.Warning && e.Message.Contains("no sprite assigned")));
        }

        [Test]
        public void Init_SetsInitialOffsetOnRoot()
        {
            BFParallaxSpriteLayerConfig config = CreateConfig();
            SetField(config, "initialOffset", new Vector2(2f, 3f));

            BFParallaxSpriteLayer layer = new BFParallaxSpriteLayer(config);
            layer.Init(parentGo.transform, 0);

            Transform root = parentGo.transform.GetChild(0);
            Assert.AreEqual(new Vector3(2f, 3f, 0f), root.localPosition);
        }

        [Test]
        public void Init_HorizontalLooping_PositionsTilesSpacedByTileWidth()
        {
            BFParallaxSpriteLayerConfig config = CreateConfig();
            SetField(config, "horizontalLooping", true);
            SetField(config, "horizontalTileWidthOverride", 2f);

            BFParallaxSpriteLayer layer = new BFParallaxSpriteLayer(config);
            layer.Init(parentGo.transform, 0);

            Transform root = parentGo.transform.GetChild(0);
            Assert.AreEqual(-2f, root.GetChild(0).localPosition.x, 0.0001f);
            Assert.AreEqual(0f, root.GetChild(1).localPosition.x, 0.0001f);
            Assert.AreEqual(2f, root.GetChild(2).localPosition.x, 0.0001f);
        }

        [Test]
        public void Tick_HorizontalEnabled_AppliesParallaxFactorAndAutoScroll()
        {
            BFParallaxSpriteLayerConfig config = CreateConfig();
            SetField(config, "horizontalEnabled", true);
            SetField(config, "horizontalParallaxFactor", 0.5f);
            SetField(config, "horizontalAutoScrollSpeed", 1f);

            BFParallaxSpriteLayer layer = new BFParallaxSpriteLayer(config);
            layer.Init(parentGo.transform, 0);
            layer.Tick(new Vector2(10f, 0f), 2f);

            Transform root = parentGo.transform.GetChild(0);
            Assert.AreEqual(7f, root.localPosition.x, 0.0001f);
        }

        [Test]
        public void Tick_NeitherAxisEnabled_RootStaysAtInitialOffset()
        {
            BFParallaxSpriteLayerConfig config = CreateConfig();
            SetField(config, "initialOffset", new Vector2(1f, 1f));

            BFParallaxSpriteLayer layer = new BFParallaxSpriteLayer(config);
            layer.Init(parentGo.transform, 0);
            layer.Tick(new Vector2(50f, 50f), 1f);

            Transform root = parentGo.transform.GetChild(0);
            Assert.AreEqual(new Vector3(1f, 1f, 0f), root.localPosition);
        }

        [Test]
        public void Tick_HorizontalLockPositiveOnly_ClampsToMaxSeenOffset()
        {
            BFParallaxSpriteLayerConfig config = CreateConfig();
            SetField(config, "horizontalEnabled", true);
            SetField(config, "horizontalParallaxFactor", 1f);
            SetField(config, "horizontalLock", BFParallaxAxisLock.PositiveOnly);

            BFParallaxSpriteLayer layer = new BFParallaxSpriteLayer(config);
            layer.Init(parentGo.transform, 0);
            layer.Tick(new Vector2(5f, 0f), 0f);
            layer.Tick(new Vector2(-10f, 0f), 0f);

            Transform root = parentGo.transform.GetChild(0);
            Assert.AreEqual(5f, root.localPosition.x, 0.0001f);
        }

        [Test]
        public void Tick_HorizontalLockNegativeOnly_ClampsToMinSeenOffset()
        {
            BFParallaxSpriteLayerConfig config = CreateConfig();
            SetField(config, "horizontalEnabled", true);
            SetField(config, "horizontalParallaxFactor", 1f);
            SetField(config, "horizontalLock", BFParallaxAxisLock.NegativeOnly);

            BFParallaxSpriteLayer layer = new BFParallaxSpriteLayer(config);
            layer.Init(parentGo.transform, 0);
            layer.Tick(new Vector2(-5f, 0f), 0f);
            layer.Tick(new Vector2(10f, 0f), 0f);

            Transform root = parentGo.transform.GetChild(0);
            Assert.AreEqual(-5f, root.localPosition.x, 0.0001f);
        }

        [Test]
        public void Tick_HorizontalLoopingWithOffset_WrapsTilePositions()
        {
            BFParallaxSpriteLayerConfig config = CreateConfig();
            SetField(config, "horizontalLooping", true);
            SetField(config, "horizontalTileWidthOverride", 2f);
            SetField(config, "horizontalEnabled", true);
            SetField(config, "horizontalParallaxFactor", 1f);

            BFParallaxSpriteLayer layer = new BFParallaxSpriteLayer(config);
            layer.Init(parentGo.transform, 0);
            layer.Tick(new Vector2(3f, 0f), 0f);

            Transform root = parentGo.transform.GetChild(0);
            Assert.AreEqual(-3f, root.GetChild(0).localPosition.x, 0.0001f);
            Assert.AreEqual(-1f, root.GetChild(1).localPosition.x, 0.0001f);
            Assert.AreEqual(1f, root.GetChild(2).localPosition.x, 0.0001f);
        }

        [Test]
        public void Cleanup_ClearsInternalState()
        {
            BFParallaxSpriteLayerConfig config = CreateConfig();
            BFParallaxSpriteLayer layer = new BFParallaxSpriteLayer(config);
            layer.Init(parentGo.transform, 0);

            layer.Cleanup();

            Assert.IsNull(GetField<Transform>(layer, "root"));
            Assert.AreEqual(0, GetField<List<SpriteRenderer>>(layer, "tiles").Count);
        }
    }
}