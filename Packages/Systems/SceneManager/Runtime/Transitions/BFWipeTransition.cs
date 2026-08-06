using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace BFTools.Systems.SceneManager
{
    public class BFWipeTransition : BFTransitionBehaviour
    {
        [SerializeField] private RectTransform screenRect;
        [SerializeField] private Image mainImage;
        [SerializeField] private Image edgeImage;
        [SerializeField] private Color wipeColor = Color.black;
        [SerializeField] private Color edgeColor = Color.white;
        [SerializeField] private float edgeThickness = 20f;
        [SerializeField] private float angle;
        [SerializeField] private float wipeOutDuration = 0.5f;
        [SerializeField] private float wipeInDuration = 0.5f;

        private Vector2 direction;
        private float axisMin;
        private float axisMax;
        private float axisRange;

        public override IEnumerator PlayOut()
        {
            ConfigureGeometry();
            yield return AnimateWipe(axisMin, axisMax, wipeOutDuration);
        }

        public override IEnumerator PlayIn()
        {
            ConfigureGeometry();
            yield return AnimateWipe(axisMax, axisMax + axisRange, wipeInDuration);
        }

        private void ConfigureGeometry()
        {
            float width = screenRect.rect.width;
            float height = screenRect.rect.height;

            float angleRad = angle * Mathf.Deg2Rad;
            direction = new Vector2(Mathf.Sin(angleRad), Mathf.Cos(angleRad));

            Vector2[] corners =
            {
                new Vector2(-width * 0.5f, -height * 0.5f),
                new Vector2(width * 0.5f, -height * 0.5f),
                new Vector2(-width * 0.5f, height * 0.5f),
                new Vector2(width * 0.5f, height * 0.5f)
            };

            axisMin = float.MaxValue;
            axisMax = float.MinValue;
            foreach (Vector2 corner in corners)
            {
                float projection = corner.x * direction.x + corner.y * direction.y;
                axisMin = Mathf.Min(axisMin, projection);
                axisMax = Mathf.Max(axisMax, projection);
            }

            axisRange = axisMax - axisMin;
            float perpendicularSize = Mathf.Sqrt(width * width + height * height) * 2f;

            ConfigureBand(mainImage, wipeColor, axisRange, perpendicularSize);
            ConfigureBand(edgeImage, edgeColor, axisRange + edgeThickness * 2f, perpendicularSize);

            mainImage.rectTransform.SetAsLastSibling();
        }

        private void ConfigureBand(Image image, Color color, float bandWidth, float perpendicularSize)
        {
            image.color = color;

            RectTransform band = image.rectTransform;
            band.anchorMin = new Vector2(0.5f, 0.5f);
            band.anchorMax = new Vector2(0.5f, 0.5f);
            band.pivot = new Vector2(0.5f, 1f);
            band.sizeDelta = new Vector2(perpendicularSize, bandWidth);
            band.up = new Vector3(direction.x, direction.y, 0f);
        }

        private IEnumerator AnimateWipe(float fromEdge, float toEdge, float duration)
        {
            SetLeadingEdge(fromEdge);

            if (duration <= 0f)
            {
                SetLeadingEdge(toEdge);
                yield break;
            }

            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                SetLeadingEdge(Mathf.Lerp(fromEdge, toEdge, t / duration));
                yield return null;
            }

            SetLeadingEdge(toEdge);
        }

        private void SetLeadingEdge(float position)
        {
            mainImage.rectTransform.anchoredPosition = direction * position;
            edgeImage.rectTransform.anchoredPosition = direction * (position + edgeThickness);
        }
    }
}