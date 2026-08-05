using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using BFTools.Core.Logger.TestUtilities;
using BFTools.Visuals.Background;

namespace BFTools.Visuals.Background.Tests
{
    public class BFTwinklingStarTests
    {
        private BFTwinklingStarsLayerConfig config;

        [TearDown]
        public void TearDown()
        {
            if (config != null)
                Object.DestroyImmediate(config);

            BFLoggerTestUtility.ResetState();
        }

        private BFTwinklingStarsLayerConfig CreateConfig()
        {
            config = ScriptableObject.CreateInstance<BFTwinklingStarsLayerConfig>();
            return config;
        }

        private static void SetField(object target, string fieldName, object value)
        {
            target.GetType()
                .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(target, value);
        }

        [Test]
        public void Size_AlwaysWithinConfiguredRange()
        {
            BFTwinklingStarsLayerConfig cfg = CreateConfig();
            SetField(cfg, "minSize", 1f);
            SetField(cfg, "maxSize", 3f);

            for (int i = 0; i < 20; i++)
            {
                BFTwinklingStar star = new BFTwinklingStar(cfg);
                Assert.GreaterOrEqual(star.Size, 1f - 0.0001f);
                Assert.LessOrEqual(star.Size, 3f + 0.0001f);
            }
        }

        [Test]
        public void Refresh_PicksUpUpdatedConfigRange()
        {
            BFTwinklingStarsLayerConfig cfg = CreateConfig();
            SetField(cfg, "minSize", 1f);
            SetField(cfg, "maxSize", 1f);

            BFTwinklingStar star = new BFTwinklingStar(cfg);
            Assert.AreEqual(1f, star.Size, 0.0001f);

            SetField(cfg, "minSize", 5f);
            SetField(cfg, "maxSize", 5f);
            star.Refresh();

            Assert.AreEqual(5f, star.Size, 0.0001f);
        }

        [Test]
        public void Tick_TwinkleDisabled_AlphaEqualsBaseAlpha()
        {
            BFTwinklingStarsLayerConfig cfg = CreateConfig();
            SetField(cfg, "minAlpha", 0.5f);
            SetField(cfg, "maxAlpha", 0.5f);
            SetField(cfg, "twinkle", false);

            BFTwinklingStar star = new BFTwinklingStar(cfg);
            star.Tick(1f);

            Assert.AreEqual(0.5f, star.Alpha, 0.0001f);
        }

        [Test]
        public void Tick_TwinkleEnabled_AppliesDepthAtKnownPhase()
        {
            BFTwinklingStarsLayerConfig cfg = CreateConfig();
            SetField(cfg, "minAlpha", 0.6f);
            SetField(cfg, "maxAlpha", 0.6f);
            SetField(cfg, "minTwinkleDepth", 0.4f);
            SetField(cfg, "maxTwinkleDepth", 0.4f);
            SetField(cfg, "twinkle", true);

            BFTwinklingStar star = new BFTwinklingStar(cfg);
            SetField(star, "phase", 0f);

            star.Tick(0f);

            Assert.AreEqual(0.6f * (1f - 0.4f), star.Alpha, 0.0001f);
        }
    }
}