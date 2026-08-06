using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using BFTools.Systems.SceneManager;
using Assert = NUnit.Framework.Assert;

namespace BFTools.Systems.SceneManager.PlayModeTests
{
    public class BFWipeTransitionTests
    {
        private const float ScreenWidth = 200f;
        private const float ScreenHeight = 100f;
        private const float Tolerance = 0.01f;

        private static readonly Color WipeColor = Color.blue;
        private static readonly Color EdgeColor = Color.yellow;
        private static readonly Vector2 EdgeSpriteSize = new Vector2(40f, 30f);

        private GameObject go;
        private Image mainImage;
        private Image edgeImage;
        private Image edgeSpriteImage;

        [TearDown]
        public void TearDown()
        {
            if (go != null)
                Object.Destroy(go);
        }

        private BFWipeTransition CreateTransition(float angle, float edgeThickness, float wipeOutDuration, float wipeInDuration, bool withEdgeSprite = false)
        {
            go = new GameObject("WipeTransition");
            RectTransform screenRect = go.AddComponent<RectTransform>();
            screenRect.sizeDelta = new Vector2(ScreenWidth, ScreenHeight);

            BFWipeTransition transition = go.AddComponent<BFWipeTransition>();

            mainImage = CreateChildImage("MainImage", screenRect);
            edgeImage = CreateChildImage("EdgeImage", screenRect);

            SetField(transition, "screenRect", screenRect);
            SetField(transition, "mainImage", mainImage);
            SetField(transition, "edgeImage", edgeImage);
            SetField(transition, "wipeColor", WipeColor);
            SetField(transition, "edgeColor", EdgeColor);
            SetField(transition, "edgeThickness", edgeThickness);
            SetField(transition, "angle", angle);
            SetField(transition, "wipeOutDuration", wipeOutDuration);
            SetField(transition, "wipeInDuration", wipeInDuration);

            if (withEdgeSprite)
            {
                edgeSpriteImage = CreateChildImage("EdgeSpriteImage", screenRect);
                SetField(transition, "edgeSpriteImage", edgeSpriteImage);
                SetField(transition, "edgeSpriteSize", EdgeSpriteSize);
            }

            return transition;
        }

        private static Image CreateChildImage(string name, RectTransform parent)
        {
            GameObject imageGo = new GameObject(name);
            imageGo.transform.SetParent(parent, false);
            return imageGo.AddComponent<Image>();
        }

        private static void SetField(BFWipeTransition transition, string fieldName, object value)
        {
            typeof(BFWipeTransition)
                .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(transition, value);
        }

        private static void AssertVector2Approximately(Vector2 expected, Vector2 actual)
        {
            Assert.AreEqual(expected.x, actual.x, Tolerance);
            Assert.AreEqual(expected.y, actual.y, Tolerance);
        }

        [UnityTest]
        public IEnumerator PlayOut_AfterCompletion_SetsColorsAndMovesMainImageToAxisMax()
        {
            BFWipeTransition transition = CreateTransition(angle: 0f, edgeThickness: 10f, wipeOutDuration: 0.05f, wipeInDuration: 0.05f);

            yield return transition.PlayOut();

            Assert.AreEqual(WipeColor, mainImage.color);
            Assert.AreEqual(EdgeColor, edgeImage.color);
            AssertVector2Approximately(new Vector2(0f, 50f), mainImage.rectTransform.anchoredPosition);
            AssertVector2Approximately(new Vector2(0f, 60f), edgeImage.rectTransform.anchoredPosition);
        }

        [UnityTest]
        public IEnumerator PlayIn_AfterCompletion_MovesMainImagePastAxisMaxByAxisRange()
        {
            BFWipeTransition transition = CreateTransition(angle: 0f, edgeThickness: 10f, wipeOutDuration: 0.05f, wipeInDuration: 0.05f);

            yield return transition.PlayIn();

            AssertVector2Approximately(new Vector2(0f, 150f), mainImage.rectTransform.anchoredPosition);
            AssertVector2Approximately(new Vector2(0f, 160f), edgeImage.rectTransform.anchoredPosition);
        }

        [UnityTest]
        public IEnumerator PlayOut_ZeroDuration_SnapsMainImageToAxisMaxWithoutWaiting()
        {
            BFWipeTransition transition = CreateTransition(angle: 0f, edgeThickness: 10f, wipeOutDuration: 0f, wipeInDuration: 0f);

            yield return transition.PlayOut();

            AssertVector2Approximately(new Vector2(0f, 50f), mainImage.rectTransform.anchoredPosition);
        }

        [UnityTest]
        public IEnumerator PlayIn_ZeroDuration_SnapsMainImagePastAxisMaxWithoutWaiting()
        {
            BFWipeTransition transition = CreateTransition(angle: 0f, edgeThickness: 10f, wipeOutDuration: 0f, wipeInDuration: 0f);

            yield return transition.PlayIn();

            AssertVector2Approximately(new Vector2(0f, 150f), mainImage.rectTransform.anchoredPosition);
        }

        [UnityTest]
        public IEnumerator PlayOut_AfterCompletion_MainImageRendersAboveEdgeImage()
        {
            BFWipeTransition transition = CreateTransition(angle: 0f, edgeThickness: 10f, wipeOutDuration: 0.05f, wipeInDuration: 0.05f);

            yield return transition.PlayOut();

            Assert.Greater(mainImage.rectTransform.GetSiblingIndex(), edgeImage.rectTransform.GetSiblingIndex());
        }

        [UnityTest]
        public IEnumerator PlayOut_WithEdgeSpriteAssigned_PositionsSpriteAtOuterEdgeAboveMainImage()
        {
            BFWipeTransition transition = CreateTransition(angle: 0f, edgeThickness: 10f, wipeOutDuration: 0.05f, wipeInDuration: 0.05f, withEdgeSprite: true);

            yield return transition.PlayOut();

            AssertVector2Approximately(new Vector2(0f, 60f), edgeSpriteImage.rectTransform.anchoredPosition);
            AssertVector2Approximately(EdgeSpriteSize, edgeSpriteImage.rectTransform.sizeDelta);
            Assert.Greater(edgeSpriteImage.rectTransform.GetSiblingIndex(), mainImage.rectTransform.GetSiblingIndex());
        }

        [UnityTest]
        public IEnumerator PlayIn_WithEdgeSpriteAssigned_PositionsSpriteAtOuterEdge()
        {
            BFWipeTransition transition = CreateTransition(angle: 0f, edgeThickness: 10f, wipeOutDuration: 0.05f, wipeInDuration: 0.05f, withEdgeSprite: true);

            yield return transition.PlayIn();

            AssertVector2Approximately(new Vector2(0f, 160f), edgeSpriteImage.rectTransform.anchoredPosition);
        }

        [UnityTest]
        public IEnumerator PlayOut_WithoutEdgeSpriteAssigned_DoesNotThrow()
        {
            BFWipeTransition transition = CreateTransition(angle: 0f, edgeThickness: 10f, wipeOutDuration: 0.05f, wipeInDuration: 0.05f);

            yield return transition.PlayOut();

            Assert.IsNull(edgeSpriteImage);
        }
    }
}