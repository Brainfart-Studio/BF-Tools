using System;
using UnityEngine;
using UnityEngine.Rendering;
using BFTools.Core.CameraUtility;
using BFTools.Core.Logger;

namespace BFTools.Core.ScreenColorSampler
{
    public class BFScreenColorSampler : MonoBehaviour
    {
        private const string LogTag = "ScreenColorSampler";

        [SerializeField] private Camera targetCameraOverride;
        [SerializeField] private float offsetX = 1f;
        [SerializeField] private float offsetY;
        [SerializeField] private float width = 1f;
        [SerializeField] private float height = 1f;
        [SerializeField] private int sampleResolution = 32;
        [SerializeField] private float sampleInterval = 0.1f;
        [SerializeField] private Color gizmoColor = Color.yellow;

        public event Action<Color> ColorSampled;
        public Color CurrentColor { get; private set; } = Color.black;

        private readonly Vector3[] worldCorners = new Vector3[4];

        private BFCameraResolver cameraResolver;
        private RenderTexture sampleTexture;
        private float sampleTimer;
        private bool readbackInFlight;
        private Rect pendingViewportRect;

        private void OnEnable()
        {
            cameraResolver = new BFCameraResolver(targetCameraOverride, this, LogTag, "No camera available to sample.");
            sampleTexture = new RenderTexture(sampleResolution, sampleResolution, 0, RenderTextureFormat.ARGB32)
            {
                useMipMap = false,
                autoGenerateMips = false,
                filterMode = FilterMode.Bilinear
            };
        }

        private void OnDisable()
        {
            if (sampleTexture != null)
            {
                sampleTexture.Release();
                sampleTexture = null;
            }
        }

        private void Update()
        {
            sampleTimer += Time.unscaledDeltaTime;
            if (sampleTimer < sampleInterval || readbackInFlight)
                return;

            sampleTimer = 0f;
            RequestSample();
        }

        private void GetWorldCorners(Vector3[] corners)
        {
            Vector3 center = transform.position + new Vector3(offsetX, offsetY, 0f);
            float halfWidth = width * 0.5f;
            float halfHeight = height * 0.5f;

            corners[0] = center + new Vector3(-halfWidth, -halfHeight, 0f);
            corners[1] = center + new Vector3(halfWidth, -halfHeight, 0f);
            corners[2] = center + new Vector3(halfWidth, halfHeight, 0f);
            corners[3] = center + new Vector3(-halfWidth, halfHeight, 0f);
        }

        private bool TryGetViewportRect(Camera targetCamera, out Rect viewportRect)
        {
            GetWorldCorners(worldCorners);

            float minX = float.MaxValue, minY = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue;
            bool anyValid = false;

            for (int i = 0; i < worldCorners.Length; i++)
            {
                Vector3 viewportPoint = targetCamera.WorldToViewportPoint(worldCorners[i]);
                if (viewportPoint.z <= 0f)
                    continue;

                anyValid = true;
                minX = Mathf.Min(minX, viewportPoint.x);
                minY = Mathf.Min(minY, viewportPoint.y);
                maxX = Mathf.Max(maxX, viewportPoint.x);
                maxY = Mathf.Max(maxY, viewportPoint.y);
            }

            if (!anyValid)
            {
                viewportRect = default;
                return false;
            }

            minX = Mathf.Clamp01(minX);
            minY = Mathf.Clamp01(minY);
            maxX = Mathf.Clamp01(maxX);
            maxY = Mathf.Clamp01(maxY);

            viewportRect = Rect.MinMaxRect(minX, minY, maxX, maxY);
            return viewportRect.width > 0f && viewportRect.height > 0f;
        }

        private void RequestSample()
        {
            Camera targetCamera = cameraResolver.Resolve();
            if (targetCamera == null)
                return;

            if (!TryGetViewportRect(targetCamera, out Rect viewportRect))
                return;

            pendingViewportRect = viewportRect;

            RenderTexture previousTarget = targetCamera.targetTexture;
            targetCamera.targetTexture = sampleTexture;
            targetCamera.Render();
            targetCamera.targetTexture = previousTarget;

            readbackInFlight = true;
            AsyncGPUReadback.Request(sampleTexture, 0, OnReadbackComplete);
        }

        private void OnReadbackComplete(AsyncGPUReadbackRequest request)
        {
            readbackInFlight = false;

            if (request.hasError)
            {
                BFLogger.Trace(LogTag, "GPU readback failed, skipping sample.", this);
                return;
            }

            var pixels = request.GetData<Color32>();
            if (pixels.Length == 0)
                return;

            int startX = Mathf.Clamp(Mathf.FloorToInt(pendingViewportRect.x * sampleResolution), 0, sampleResolution - 1);
            int startY = Mathf.Clamp(Mathf.FloorToInt(pendingViewportRect.y * sampleResolution), 0, sampleResolution - 1);
            int regionWidth = Mathf.Clamp(Mathf.CeilToInt(pendingViewportRect.width * sampleResolution), 1, sampleResolution - startX);
            int regionHeight = Mathf.Clamp(Mathf.CeilToInt(pendingViewportRect.height * sampleResolution), 1, sampleResolution - startY);

            long r = 0, g = 0, b = 0;
            int count = 0;
            for (int y = startY; y < startY + regionHeight; y++)
            {
                int rowStart = y * sampleResolution;
                for (int x = startX; x < startX + regionWidth; x++)
                {
                    Color32 pixel = pixels[rowStart + x];
                    r += pixel.r;
                    g += pixel.g;
                    b += pixel.b;
                    count++;
                }
            }

            if (count == 0)
                return;

            CurrentColor = new Color(r / 255f / count, g / 255f / count, b / 255f / count);
            ColorSampled?.Invoke(CurrentColor);
        }

        private void OnDrawGizmosSelected()
        {
            GetWorldCorners(worldCorners);

            Gizmos.color = gizmoColor;
            Gizmos.DrawLine(worldCorners[0], worldCorners[1]);
            Gizmos.DrawLine(worldCorners[1], worldCorners[2]);
            Gizmos.DrawLine(worldCorners[2], worldCorners[3]);
            Gizmos.DrawLine(worldCorners[3], worldCorners[0]);
        }
    }
}