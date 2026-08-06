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
    public class BFRadialWipeTransitionTests
    {
        private GameObject go;
        private Image image;

        [TearDown]
        public void TearDown()
        {
            if (go != null)
                Object.Destroy(go);
        }

        private BFRadialWipeTransition CreateTransition(float wipeOutDuration, float wipeInDuration, bool clockwise = true)
        {
            go = new GameObject("RadialWipeTransition");
            image = go.AddComponent<Image>();
            BFRadialWipeTransition transition = go.AddComponent<BFRadialWipeTransition>();

            SetField(transition, "image", image);
            SetField(transition, "color", Color.red);
            SetField(transition, "clockwise", clockwise);
            SetField(transition, "wipeOutDuration", wipeOutDuration);
            SetField(transition, "wipeInDuration", wipeInDuration);

            return transition;
        }

        private static void SetField(BFRadialWipeTransition transition, string fieldName, object value)
        {
            typeof(BFRadialWipeTransition)
                .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(transition, value);
        }

        [UnityTest]
        public IEnumerator PlayOut_AfterCompletion_FillAmountIsOneAndBlocksRaycasts()
        {
            BFRadialWipeTransition transition = CreateTransition(0.05f, 0.05f);

            yield return transition.PlayOut();

            Assert.AreEqual(1f, image.fillAmount);
            Assert.IsTrue(image.raycastTarget);
            Assert.AreEqual(Color.red, image.color);
        }

        [UnityTest]
        public IEnumerator PlayIn_AfterCompletion_FillAmountIsZeroAndDoesNotBlockRaycasts()
        {
            BFRadialWipeTransition transition = CreateTransition(0.05f, 0.05f);
            image.fillAmount = 1f;
            image.raycastTarget = true;

            yield return transition.PlayIn();

            Assert.AreEqual(0f, image.fillAmount);
            Assert.IsFalse(image.raycastTarget);
        }

        [UnityTest]
        public IEnumerator PlayOut_ZeroDuration_SnapsFillAmountToTargetWithoutWaiting()
        {
            BFRadialWipeTransition transition = CreateTransition(0f, 0f);

            yield return transition.PlayOut();

            Assert.AreEqual(1f, image.fillAmount);
        }

        [UnityTest]
        public IEnumerator PlayIn_ZeroDuration_SnapsFillAmountToTargetWithoutWaiting()
        {
            BFRadialWipeTransition transition = CreateTransition(0f, 0f);
            image.fillAmount = 1f;

            yield return transition.PlayIn();

            Assert.AreEqual(0f, image.fillAmount);
        }

        [UnityTest]
        public IEnumerator PlayOut_AfterCompletion_ConfiguresRadial360FillFromTop()
        {
            BFRadialWipeTransition transition = CreateTransition(0.05f, 0.05f);

            yield return transition.PlayOut();

            Assert.AreEqual(Image.Type.Filled, image.type);
            Assert.AreEqual(Image.FillMethod.Radial360, image.fillMethod);
            Assert.AreEqual((int)Image.Origin360.Top, image.fillOrigin);
        }

        [UnityTest]
        public IEnumerator PlayOut_ClockwiseFalse_ConfiguresCounterclockwiseFill()
        {
            BFRadialWipeTransition transition = CreateTransition(0.05f, 0.05f, clockwise: false);

            yield return transition.PlayOut();

            Assert.IsFalse(image.fillClockwise);
        }
    }
}