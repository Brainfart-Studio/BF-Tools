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
        [Range(0f, 1f)][SerializeField] private float offsetX;
        [Range(0f, 1f)][SerializeField] private float offsetY;
        [Range(0.01f, 1f)][SerializeField] private float width = 1f;
        [Range(0.01f, 1f)][SerializeField] private float height = 1f;
        [SerializeField] private int sampleResolution = 32;
        [SerializeField] private float sampleInterval = 0.1f;

        public event Action<Color> ColorSampled;
        public Color CurrentColor { get; private set; } = Color.black;

        private BFCameraResolver cameraResolver;
        private RenderTexture sampleTexture;
        private float sampleTimer;
        private bool readbackInFlight;

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

        private void RequestSample()
        {
            Camera targetCamera = cameraResolver.Resolve();
            if (targetCamera == null)
                return;

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

            int startX = Mathf.Clamp(Mathf.FloorToInt(offsetX * sampleResolution), 0, sampleResolution - 1);
            int startY = Mathf.Clamp(Mathf.FloorToInt(offsetY * sampleResolution), 0, sampleResolution - 1);
            int regionWidth = Mathf.Clamp(Mathf.CeilToInt(width * sampleResolution), 1, sampleResolution - startX);
            int regionHeight = Mathf.Clamp(Mathf.CeilToInt(height * sampleResolution), 1, sampleResolution - startY);

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
    }
}