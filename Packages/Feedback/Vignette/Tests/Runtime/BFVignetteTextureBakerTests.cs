using NUnit.Framework;
using UnityEngine;
using BFTools.Feedback.Vignette;

namespace BFTools.Feedback.Vignette.Tests
{
    public class BFVignetteTextureBakerTests
    {
        private static readonly float[] FlatProfileAngles = { 0f };
        private static readonly float[] FlatProfileValues = { 0f };

        [Test]
        public void Bake_ReturnsTextureAtRequestedResolution()
        {
            Texture2D texture = BFVignetteTextureBaker.Bake(0.5f, 0.2f, 1f, 1f, FlatProfileAngles, FlatProfileValues, false, null, 64);

            Assert.AreEqual(64, texture.width);
            Assert.AreEqual(64, texture.height);

            Object.DestroyImmediate(texture);
        }

        [Test]
        public void Bake_CenterPixel_IsFullyTransparent()
        {
            Texture2D texture = BFVignetteTextureBaker.Bake(0.5f, 0.2f, 1f, 1f, FlatProfileAngles, FlatProfileValues, false, null, 64);

            Color32 center = texture.GetPixel(32, 32);

            Assert.AreEqual(0, (int)center.a);

            Object.DestroyImmediate(texture);
        }

        [Test]
        public void Bake_CornerPixel_IsFullyOpaque()
        {
            Texture2D texture = BFVignetteTextureBaker.Bake(0.5f, 0.2f, 1f, 1f, FlatProfileAngles, FlatProfileValues, false, null, 64);

            Color32 corner = texture.GetPixel(0, 0);

            Assert.AreEqual(255, (int)corner.a);

            Object.DestroyImmediate(texture);
        }

        [Test]
        public void Bake_WithGradient_CornerUsesGradientEndColor()
        {
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.blue, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });

            Texture2D texture = BFVignetteTextureBaker.Bake(0.5f, 0.2f, 1f, 1f, FlatProfileAngles, FlatProfileValues, true, gradient, 64);

            Color32 corner = texture.GetPixel(0, 0);

            Assert.AreEqual((int)(Color.blue.b * 255f), (int)corner.b);
            Assert.AreEqual(255, (int)corner.a);

            Object.DestroyImmediate(texture);
        }
    }
}