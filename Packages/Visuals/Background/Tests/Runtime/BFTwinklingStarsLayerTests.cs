using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using BFTools.Core.Logger.TestUtilities;
using BFTools.Visuals.Background;

namespace BFTools.Visuals.Background.Tests
{
    public class BFTwinklingStarsLayerTests
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

        private BFTwinklingStarsLayerConfig CreateConfig()
        {
            BFTwinklingStarsLayerConfig config = ScriptableObject.CreateInstance<BFTwinklingStarsLayerConfig>();
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

        [Test]
        public void CreateLayer_ReturnsBFTwinklingStarsLayer()
        {
            BFTwinklingStarsLayerConfig config = CreateConfig();

            IBFBackgroundLayer layer = config.CreateLayer();

            Assert.IsInstanceOf<BFTwinklingStarsLayer>(layer);
        }

        [Test]
        public void Init_CreatesRootOnBackgroundLayerWithSortingOrder()
        {
            BFTwinklingStarsLayerConfig config = CreateConfig();
            BFTwinklingStarsLayer layer = new BFTwinklingStarsLayer(config);

            layer.Init(parentGo.transform, 6);

            Assert.AreEqual(1, parentGo.transform.childCount);
            Transform root = parentGo.transform.GetChild(0);
            Assert.AreEqual(BFBackgroundStackManager.BackgroundLayer, root.gameObject.layer);

            MeshRenderer meshRenderer = root.GetComponent<MeshRenderer>();
            Assert.AreEqual(6, meshRenderer.sortingOrder);
        }

        [Test]
        public void Init_SyncsStarCountAndMeshVertices()
        {
            BFTwinklingStarsLayerConfig config = CreateConfig();
            SetField(config, "starCount", 5);

            BFTwinklingStarsLayer layer = new BFTwinklingStarsLayer(config);
            layer.Init(parentGo.transform, 0);

            List<BFTwinklingStar> stars = GetField<List<BFTwinklingStar>>(layer, "stars");
            Mesh mesh = GetField<Mesh>(layer, "mesh");

            Assert.AreEqual(5, stars.Count);
            Assert.AreEqual(20, mesh.vertexCount);
        }

        [Test]
        public void Tick_StarCountIncreased_GrowsStarsAndMesh()
        {
            BFTwinklingStarsLayerConfig config = CreateConfig();
            SetField(config, "starCount", 2);
            BFTwinklingStarsLayer layer = new BFTwinklingStarsLayer(config);
            layer.Init(parentGo.transform, 0);

            SetField(config, "starCount", 4);
            layer.Tick(0f);

            List<BFTwinklingStar> stars = GetField<List<BFTwinklingStar>>(layer, "stars");
            Mesh mesh = GetField<Mesh>(layer, "mesh");
            Assert.AreEqual(4, stars.Count);
            Assert.AreEqual(16, mesh.vertexCount);
        }

        [Test]
        public void Tick_StarCountDecreased_ShrinksStarsAndMesh()
        {
            BFTwinklingStarsLayerConfig config = CreateConfig();
            SetField(config, "starCount", 4);
            BFTwinklingStarsLayer layer = new BFTwinklingStarsLayer(config);
            layer.Init(parentGo.transform, 0);

            SetField(config, "starCount", 1);
            layer.Tick(0f);

            List<BFTwinklingStar> stars = GetField<List<BFTwinklingStar>>(layer, "stars");
            Mesh mesh = GetField<Mesh>(layer, "mesh");
            Assert.AreEqual(1, stars.Count);
            Assert.AreEqual(4, mesh.vertexCount);
        }

        [Test]
        public void Cleanup_ClearsInternalState()
        {
            BFTwinklingStarsLayerConfig config = CreateConfig();
            SetField(config, "starCount", 3);
            BFTwinklingStarsLayer layer = new BFTwinklingStarsLayer(config);
            layer.Init(parentGo.transform, 0);

            layer.Cleanup();

            Assert.IsNull(GetField<Transform>(layer, "root"));
            Assert.IsNull(GetField<Mesh>(layer, "mesh"));
            Assert.IsNull(GetField<Texture2D>(layer, "rampTexture"));
            Assert.IsNull(GetField<MeshRenderer>(layer, "meshRenderer"));
            Assert.IsNull(GetField<Material>(layer, "material"));
            Assert.AreEqual(0, GetField<List<BFTwinklingStar>>(layer, "stars").Count);
        }
    }
}