using BFTools.Core.Logger;
using UnityEngine;

namespace BFTools.Visuals.Background
{
    internal class BFBackgroundStackCamera
    {
        private const string LogTag = "Background";

        private readonly Camera targetCameraOverride;
        private readonly Object context;

        private Camera backgroundCamera;
        private Camera outputCamera;
        private CameraClearFlags outputCameraOriginalClearFlags;
        private int outputCameraOriginalCullingMask;
        private bool outputCameraConfigured;
        private bool hasWarnedMissingCamera;

        public BFBackgroundStackCamera(Camera targetCameraOverride, Object context)
        {
            this.targetCameraOverride = targetCameraOverride;
            this.context = context;
        }

        public void Init()
        {
            backgroundCamera = CreateBackgroundCamera();
            Sync();
        }

        public void Sync()
        {
            if (backgroundCamera != null)
            {
                backgroundCamera.orthographicSize = Screen.height / 2f;
                backgroundCamera.transform.position = new Vector3(Screen.width / 2f, Screen.height / 2f, -10f);
            }

            // Retry each frame until resolved: the scene's main camera may not exist yet
            // if the manager's OnEnable runs before it (script execution order, async
            // scene loads), and a dropped-in prefab shouldn't depend on getting that
            // ordering right.
            if (!outputCameraConfigured)
                ConfigureOutputCamera();
        }

        public void Cleanup()
        {
            RestoreOutputCamera();

            if (backgroundCamera != null)
                Object.Destroy(backgroundCamera.gameObject);
            backgroundCamera = null;
        }

        private Camera CreateBackgroundCamera()
        {
            GameObject cameraObj = new GameObject("BFBackgroundStackCamera");

            Camera cam = cameraObj.AddComponent<Camera>();
            cam.orthographic = true;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
            cam.cullingMask = 1 << BFBackgroundStackManager.BackgroundLayer;
            cam.depth = -100;

            return cam;
        }

        // Without this, the output camera's default clear flags wipe the background
        // camera's draw every frame (it renders first at depth -100, then gets erased
        // when the output camera clears the screen for its own pass), and the reserved
        // layer would otherwise be drawn twice if left in the output camera's mask.
        private void ConfigureOutputCamera()
        {
            outputCamera = targetCameraOverride != null ? targetCameraOverride : Camera.main;
            if (outputCamera == null)
            {
                if (!hasWarnedMissingCamera)
                {
                    BFLogger.Error(LogTag, "BFBackgroundStackCamera: no output camera found. Assign a Camera to Target Camera Override, or tag a camera MainCamera in the scene - otherwise this background will never be visible.", context);
                    hasWarnedMissingCamera = true;
                }

                return;
            }

            outputCameraOriginalClearFlags = outputCamera.clearFlags;
            outputCameraOriginalCullingMask = outputCamera.cullingMask;

            outputCamera.clearFlags = CameraClearFlags.Depth;
            outputCamera.cullingMask &= ~(1 << BFBackgroundStackManager.BackgroundLayer);

            outputCameraConfigured = true;

            BFLogger.Info(LogTag, $"BFBackgroundStackCamera: configured output camera '{outputCamera.name}'.", context);
        }

        private void RestoreOutputCamera()
        {
            if (outputCameraConfigured && outputCamera != null)
            {
                outputCamera.clearFlags = outputCameraOriginalClearFlags;
                outputCamera.cullingMask = outputCameraOriginalCullingMask;

                BFLogger.Info(LogTag, $"BFBackgroundStackCamera: restored output camera '{outputCamera.name}'.", context);
            }

            outputCamera = null;
            outputCameraConfigured = false;
        }
    }
}