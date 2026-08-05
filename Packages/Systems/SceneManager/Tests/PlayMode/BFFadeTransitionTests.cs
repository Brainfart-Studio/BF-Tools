using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using BFTools.Systems.SceneManager;
using Assert = NUnit.Framework.Assert;

namespace BFTools.Systems.SceneManager.PlayModeTests
{
    public class BFFadeTransitionTests
    {
        private GameObject go;
        private CanvasGroup canvasGroup;

        [TearDown]
        public void TearDown()
        {
            if (go != null)
                Object.Destroy(go);
        }

        private BFFadeTransition CreateTransition(float fadeOutDuration, float fadeInDuration)
        {
            go = new GameObject("FadeTransition");
            canvasGroup = go.AddComponent<CanvasGroup>();
            BFFadeTransition transition = go.AddComponent<BFFadeTransition>();

            typeof(BFFadeTransition)
                .GetField("canvasGroup", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(transition, canvasGroup);
            typeof(BFFadeTransition)
                .GetField("fadeOutDuration", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(transition, fadeOutDuration);
            typeof(BFFadeTransition)
                .GetField("fadeInDuration", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(transition, fadeInDuration);

            return transition;
        }

        [UnityTest]
        public IEnumerator PlayOut_AfterCompletion_AlphaIsOneAndBlocksRaycastsTrue()
        {
            BFFadeTransition transition = CreateTransition(0.05f, 0.05f);

            yield return transition.PlayOut();

            Assert.AreEqual(1f, canvasGroup.alpha);
            Assert.IsTrue(canvasGroup.blocksRaycasts);
        }

        [UnityTest]
        public IEnumerator PlayIn_AfterCompletion_AlphaIsZeroAndBlocksRaycastsFalse()
        {
            BFFadeTransition transition = CreateTransition(0.05f, 0.05f);
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;

            yield return transition.PlayIn();

            Assert.AreEqual(0f, canvasGroup.alpha);
            Assert.IsFalse(canvasGroup.blocksRaycasts);
        }

        [UnityTest]
        public IEnumerator PlayOut_ZeroDuration_SnapsAlphaToTargetWithoutWaiting()
        {
            BFFadeTransition transition = CreateTransition(0f, 0f);

            yield return transition.PlayOut();

            Assert.AreEqual(1f, canvasGroup.alpha);
        }

        [UnityTest]
        public IEnumerator PlayIn_ZeroDuration_SnapsAlphaToTargetWithoutWaiting()
        {
            BFFadeTransition transition = CreateTransition(0f, 0f);
            canvasGroup.alpha = 1f;

            yield return transition.PlayIn();

            Assert.AreEqual(0f, canvasGroup.alpha);
        }
    }
}