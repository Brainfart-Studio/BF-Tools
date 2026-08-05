using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using BFTools.Core.Logger;
using BFTools.Core.Logger.TestUtilities;
using BFTools.Visuals.Background;

namespace BFTools.Visuals.Background.Tests
{
    public class BFGradientLayerTests
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

        private BFGradientLayerConfig CreateConfig()
        {
            BFGradientLayerConfig config = ScriptableObject.CreateInstance<BFGradientLayerConfig>();
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

        private static Gradient CreateBlackToWhiteGradient()
        {
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.black, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
            return gradient;
        }

        [Test]
        public void CreateLayer_ReturnsBFGradientLayer()
        {
            BFGradientLayerConfig config = CreateConfig();

            IBFBackgroundLayer layer = config.CreateLayer();

            Assert.IsInstanceOf<BFGradientLayer>(layer);
        }

        [Test]
        public void Init_CreatesRootOnBackgroundLayerWithSortingOrder()
        {
            BFGradientLayerConfig config = CreateConfig();
            BFGradientLayer layer = new BFGradientLayer(config);

            layer.Init(parentGo.transform, 4);

            Assert.AreEqual(1, parentGo.transform.childCount);
            Transform root = parentGo.transform.GetChild(0);
            Assert.AreEqual(BFBackgroundStackManager.BackgroundLayer, root.gameObject.layer);
            Assert.AreEqual(new Vector3(0f, 0f, 10f), root.position);

            MeshRenderer meshRenderer = root.GetComponent<MeshRenderer>();
            Assert.AreEqual(4, meshRenderer.sortingOrder);
        }

        [Test]
        public void Init_DefaultMidpointAndSpread_SamplesGradientDirectlyIntoRampTexture()
        {
            BFGradientLayerConfig config = CreateConfig();
            SetField(config, "colorGradient", CreateBlackToWhiteGradient());

            BFGradientLayer layer = new BFGradientLayer(config);
            layer.Init(parentGo.transform, 0);

            Texture2D rampTexture = GetField<Texture2D>(layer, "rampTexture");
            Assert.Less(rampTexture.GetPixel(0, 0).grayscale, 0.05f);
            Assert.Greater(rampTexture.GetPixel(0, rampTexture.height - 1).grayscale, 0.95f);
        }

        [Test]
        public void Tick_AdvancesAnimationAccumulators()
        {
            BFGradientLayerConfig config = CreateConfig();
            SetField(config, "rotationSpeed", 90f);
            SetField(config, "shiftSpeed", 2f);
            SetField(config, "rotationOscillationSpeed", 1f);
            SetField(config, "waveFrequencyOscillationSpeed", 1f);
            SetField(config, "waveAmplitudeRandomnessSpeed", 1f);

            BFGradientLayer layer = new BFGradientLayer(config);
            layer.Init(parentGo.transform, 0);
            layer.Tick(1f);

            Assert.AreEqual(90f, GetField<float>(layer, "rotationElapsedTime"), 0.001f);
            Assert.AreEqual(2f, GetField<float>(layer, "elapsedTime"), 0.001f);
            Assert.AreEqual(0.1f, GetField<float>(layer, "rotationOscillationElapsedTime"), 0.001f);
            Assert.AreEqual(0.1f, GetField<float>(layer, "waveFrequencyOscillationElapsedTime"), 0.001f);
            Assert.AreEqual(0.1f, GetField<float>(layer, "waveAmplitudeDriftElapsedTime"), 0.001f);
        }

        [Test]
        public void Cleanup_ClearsInternalState()
        {
            BFGradientLayerConfig config = CreateConfig();
            BFGradientLayer layer = new BFGradientLayer(config);
            layer.Init(parentGo.transform, 0);

            layer.Cleanup();

            Assert.IsNull(GetField<Transform>(layer, "root"));
            Assert.IsNull(GetField<Mesh>(layer, "mesh"));
            Assert.IsNull(GetField<Texture2D>(layer, "rampTexture"));
            Assert.IsNull(GetField<MeshRenderer>(layer, "meshRenderer"));
            Assert.IsNull(GetField<Material>(layer, "material"));
        }
    }
}