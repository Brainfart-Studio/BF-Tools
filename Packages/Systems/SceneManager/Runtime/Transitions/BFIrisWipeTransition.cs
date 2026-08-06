using System.Collections;
using UnityEngine;

namespace BFTools.Systems.SceneManager
{
    public class BFIrisWipeTransition : BFTransitionBehaviour
    {
        [SerializeField] private SpriteRenderer curtainRenderer;
        [SerializeField] private Transform maskTransform;
        [SerializeField] private Color curtainColor = Color.black;
        [SerializeField] private float revealedMaskScale = 20f;
        [SerializeField] private float closedMaskScale = 0f;
        [SerializeField] private float rotationSpeed;
        [SerializeField] private float wipeOutDuration = 0.5f;
        [SerializeField] private float wipeInDuration = 0.5f;

        public override IEnumerator PlayOut()
        {
            curtainRenderer.color = curtainColor;
            curtainRenderer.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
            yield return AnimateMask(revealedMaskScale, closedMaskScale, wipeOutDuration);
        }

        public override IEnumerator PlayIn()
        {
            curtainRenderer.color = curtainColor;
            curtainRenderer.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
            yield return AnimateMask(closedMaskScale, revealedMaskScale, wipeInDuration);
        }

        private IEnumerator AnimateMask(float fromScale, float toScale, float duration)
        {
            SetMaskScale(fromScale);

            if (duration <= 0f)
            {
                SetMaskScale(toScale);
                yield break;
            }

            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                SetMaskScale(Mathf.Lerp(fromScale, toScale, t / duration));
                maskTransform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
                yield return null;
            }

            SetMaskScale(toScale);
        }

        private void SetMaskScale(float scale)
        {
            maskTransform.localScale = new Vector3(scale, scale, 1f);
        }
    }
}