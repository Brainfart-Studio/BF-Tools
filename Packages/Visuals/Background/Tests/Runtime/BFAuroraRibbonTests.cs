using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using BFTools.Core.Logger.TestUtilities;
using BFTools.Visuals.Background;

namespace BFTools.Visuals.Background.Tests
{
    public class BFAuroraRibbonTests
    {
        private BFAuroraRibbonsLayerConfig config;

        [TearDown]
        public void TearDown()
        {
            if (config != null)
                Object.DestroyImmediate(config);

            BFLoggerTestUtility.ResetState();
        }

        private BFAuroraRibbonsLayerConfig CreateConfig()
        {
            config = ScriptableObject.CreateInstance<BFAuroraRibbonsLayerConfig>();
            return config;
        }

        private static void SetField(object target, string fieldName, object value)
        {
            target.GetType()
                .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(target, value);
        }

        [Test]
        public void Refresh_SelectsColorByIndexModuloRibbonColorsCount()
        {
            BFAuroraRibbonsLayerConfig cfg = CreateConfig();
            SetField(cfg, "ribbonColors", new List<Color> { Color.red, Color.green, Color.blue });

            BFAuroraRibbon ribbon = new BFAuroraRibbon(4, cfg);

            Assert.AreEqual(Color.green, ribbon.Color);
        }

        [Test]
        public void Refresh_EmptyRibbonColors_DefaultsToWhite()
        {
            BFAuroraRibbonsLayerConfig cfg = CreateConfig();
            SetField(cfg, "ribbonColors", new List<Color>());

            BFAuroraRibbon ribbon = new BFAuroraRibbon(0, cfg);

            Assert.AreEqual(Color.white, ribbon.Color);
        }

        [Test]
        public void Refresh_PinnedThicknessVariance_ComputesCurrentThickness()
        {
            BFAuroraRibbonsLayerConfig cfg = CreateConfig();
            SetField(cfg, "thickness", 10f);
            SetField(cfg, "minThicknessVariance", 2f);
            SetField(cfg, "maxThicknessVariance", 2f);

            BFAuroraRibbon ribbon = new BFAuroraRibbon(0, cfg);

            Assert.AreEqual(20f, ribbon.CurrentThickness, 0.0001f);
        }

        [Test]
        public void SampleY_SingleCenteredRibbonWithZeroAmplitude_ReturnsBaselineY()
        {
            BFAuroraRibbonsLayerConfig cfg = CreateConfig();
            SetField(cfg, "ribbonCount", 1);
            SetField(cfg, "amplitude", 0f);

            BFAuroraRibbon ribbon = new BFAuroraRibbon(0, cfg);

            float y = ribbon.SampleY(0.3f, 5f, 100f);

            Assert.AreEqual(50f, y, 0.0001f);
        }

        [Test]
        public void SampleY_MultiRibbonSpacing_OffsetsBaselineByIndex()
        {
            BFAuroraRibbonsLayerConfig cfg = CreateConfig();
            SetField(cfg, "ribbonCount", 3);
            SetField(cfg, "ribbonSpacing", 0.1f);
            SetField(cfg, "amplitude", 0f);

            BFAuroraRibbon first = new BFAuroraRibbon(0, cfg);
            BFAuroraRibbon last = new BFAuroraRibbon(2, cfg);

            Assert.AreEqual(40f, first.SampleY(0.5f, 1f, 100f), 0.0001f);
            Assert.AreEqual(60f, last.SampleY(0.5f, 1f, 100f), 0.0001f);
        }
    }
}