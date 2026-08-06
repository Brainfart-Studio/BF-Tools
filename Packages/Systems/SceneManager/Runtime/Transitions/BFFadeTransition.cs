using System.Collections;
using UnityEngine;

namespace BFTools.Systems.SceneManager
{
    public class BFFadeTransition : BFTransitionBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private float fadeOutDuration = 0.5f;
        [SerializeField] private float fadeInDuration = 0.5f;

        public override IEnumerator PlayOut()
        {
            canvasGroup.blocksRaycasts = true;
            yield return Fade(0f, 1f, fadeOutDuration);
        }

        public override IEnumerator PlayIn()
        {
            yield return Fade(1f, 0f, fadeInDuration);
            canvasGroup.blocksRaycasts = false;
        }

        private IEnumerator Fade(float from, float to, float duration)
        {
            canvasGroup.alpha = from;

            if (duration <= 0f)
            {
                canvasGroup.alpha = to;
                yield break;
            }

            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(from, to, t / duration);
                yield return null;
            }

            canvasGroup.alpha = to;
        }
    }
}