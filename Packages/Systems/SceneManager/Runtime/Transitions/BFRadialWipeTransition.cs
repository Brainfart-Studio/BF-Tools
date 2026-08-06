using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace BFTools.Systems.SceneManager
{
    public class BFRadialWipeTransition : BFTransitionBehaviour
    {
        [SerializeField] private Image image;
        [SerializeField] private Color color = Color.black;
        [SerializeField] private bool clockwise = true;
        [SerializeField] private float wipeOutDuration = 0.5f;
        [SerializeField] private float wipeInDuration = 0.5f;

        public override IEnumerator PlayOut()
        {
            image.raycastTarget = true;
            ConfigureFill();
            yield return AnimateFill(0f, 1f, wipeOutDuration);
        }

        public override IEnumerator PlayIn()
        {
            ConfigureFill();
            yield return AnimateFill(1f, 0f, wipeInDuration);
            image.raycastTarget = false;
        }

        private void ConfigureFill()
        {
            image.color = color;
            image.type = Image.Type.Filled;
            image.fillMethod = Image.FillMethod.Radial360;
            image.fillOrigin = (int)Image.Origin360.Top;
            image.fillClockwise = clockwise;
        }

        private IEnumerator AnimateFill(float from, float to, float duration)
        {
            image.fillAmount = from;

            if (duration <= 0f)
            {
                image.fillAmount = to;
                yield break;
            }

            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                image.fillAmount = Mathf.Lerp(from, to, t / duration);
                yield return null;
            }

            image.fillAmount = to;
        }
    }
}