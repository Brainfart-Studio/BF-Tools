using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using BFTools.Systems.SceneManager;
using Assert = NUnit.Framework.Assert;

namespace BFTools.Systems.SceneManager.PlayModeTests
{
    public class BFIrisWipeTransitionTests
    {
        private const float Tolerance = 0.01f;

        private GameObject go;
        private SpriteRenderer curtainRenderer;
        private Transform maskTransform;

        [TearDown]
        public void TearDown()
        {
            if (go != null)
                Object.Destroy(go);
        }

        private BFIrisWipeTransition CreateTransition(float revealedMaskScale, float closedMaskScale, float wipeOutDuration, float wipeInDuration, float rotationSpeed = 0f)
        {
            go = new GameObject("IrisWipeTransition");
            curtainRenderer = go.AddComponent<SpriteRenderer>();

            GameObject maskGo = new GameObject("Mask");
            maskGo.transform.SetParent(go.transform);
            maskTransform = maskGo.transform;

            BFIrisWipeTransition transition = go.AddComponent<BFIrisWipeTransition>();

            SetField(transition, "curtainRenderer", curtainRenderer);
            SetField(transition, "maskTransform", maskTransform);
            SetField(transition, "curtainColor", Color.red);
            SetField(transition, "revealedMaskScale", revealedMaskScale);
            SetField(transition, "closedMaskScale", closedMaskScale);
            SetField(transition, "rotationSpeed", rotationSpeed);
            SetField(transition, "wipeOutDuration", wipeOutDuration);
            SetField(transition, "wipeInDuration", wipeInDuration);

            return transition;
        }

        private static void SetField(BFIrisWipeTransition transition, string fieldName, object value)
        {
            typeof(BFIrisWipeTransition)
                .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(transition, value);
        }

        private static void AssertVector3Approximately(Vector3 expected, Vector3 actual)
        {
            Assert.AreEqual(expected.x, actual.x, Tolerance);
            Assert.AreEqual(expected.y, actual.y, Tolerance);
            Assert.AreEqual(expected.z, actual.z, Tolerance);
        }

        [UnityTest]
        public IEnumerator PlayOut_AfterCompletion_SetsColorAndScalesMaskToClosedScale()
        {
            BFIrisWipeTransition transition = CreateTransition(revealedMaskScale: 20f, closedMaskScale: 0f, wipeOutDuration: 0.05f, wipeInDuration: 0.05f);

            yield return transition.PlayOut();

            Assert.AreEqual(Color.red, curtainRenderer.color);
            AssertVector3Approximately(new Vector3(0f, 0f, 1f), maskTransform.localScale);
        }

        [UnityTest]
        public IEnumerator PlayIn_AfterCompletion_ScalesMaskToRevealedScale()
        {
            BFIrisWipeTransition transition = CreateTransition(revealedMaskScale: 20f, closedMaskScale: 0f, wipeOutDuration: 0.05f, wipeInDuration: 0.05f);
            maskTransform.localScale = Vector3.zero;

            yield return transition.PlayIn();

            AssertVector3Approximately(new Vector3(20f, 20f, 1f), maskTransform.localScale);
        }

        [UnityTest]
        public IEnumerator PlayOut_ZeroDuration_SnapsMaskToClosedScaleWithoutWaiting()
        {
            BFIrisWipeTransition transition = CreateTransition(revealedMaskScale: 20f, closedMaskScale: 0f, wipeOutDuration: 0f, wipeInDuration: 0f);

            yield return transition.PlayOut();

            AssertVector3Approximately(new Vector3(0f, 0f, 1f), maskTransform.localScale);
        }

        [UnityTest]
        public IEnumerator PlayIn_ZeroDuration_SnapsMaskToRevealedScaleWithoutWaiting()
        {
            BFIrisWipeTransition transition = CreateTransition(revealedMaskScale: 20f, closedMaskScale: 0f, wipeOutDuration: 0f, wipeInDuration: 0f);
            maskTransform.localScale = Vector3.zero;

            yield return transition.PlayIn();

            AssertVector3Approximately(new Vector3(20f, 20f, 1f), maskTransform.localScale);
        }

        [UnityTest]
        public IEnumerator PlayOut_WithRotationSpeed_RotatesMaskDuringAnimation()
        {
            BFIrisWipeTransition transition = CreateTransition(revealedMaskScale: 20f, closedMaskScale: 0f, wipeOutDuration: 0.5f, wipeInDuration: 0.5f, rotationSpeed: 90f);

            yield return transition.PlayOut();

            Assert.Greater(Mathf.Abs(maskTransform.localEulerAngles.z), 0f);
        }
    }
}