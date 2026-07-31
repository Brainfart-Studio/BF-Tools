using UnityEngine;

namespace BFTools.Visuals.Background
{
    public class BFBackgroundManager : MonoBehaviour
    {
        // Reserved layer for engine-spawned background renderers. Any other camera in the
        // scene must exclude this layer from its culling mask, or its output will be drawn
        // twice (once by that camera, once by the dedicated background camera below).
        public const int BackgroundLayer = 31;

        [SerializeField] private BFBackgroundConfig config;
        [SerializeField] private Camera targetCameraOverride;

        private IBFBackgroundEffect activeEffect;
        private Camera backgroundCamera;

        private Camera outputCamera;
        private bool outputCameraConfigured;
        private bool hasWarnedMissingCamera;
        private CameraClearFlags cachedClearFlags;
        private int cachedCullingMask;

        private void OnEnable()
        {
            if (config == null)
                return;

            hasWarnedMissingCamera = false;

            backgroundCamera = CreateBackgroundCamera();
            SyncBackgroundCamera();

            ConfigureOutputCamera();

            activeEffect = config.CreateEffect();
            activeEffect.Init(transform);
        }

        private void Update()
        {
            SyncBackgroundCamera();

            // Retry each frame until resolved: the scene's main camera may not exist yet
            // if this manager's OnEnable runs before it (script execution order, async
            // scene loads), and a dropped-in prefab shouldn't depend on getting that
            // ordering right.
            if (!outputCameraConfigured)
                ConfigureOutputCamera();

            activeEffect?.Tick(Time.deltaTime);
        }

        private void OnDisable()
        {
            activeEffect?.Cleanup();
            activeEffect = null;

            if (backgroundCamera != null)
                Destroy(backgroundCamera.gameObject);
            backgroundCamera = null;

            RestoreOutputCamera();
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
                    Debug.LogError("BFBackgroundManager: no output camera found. Assign a Camera to Target Camera Override, or tag a camera MainCamera in the scene - otherwise this background effect will never be visible.", this);
                    hasWarnedMissingCamera = true;
                }

                return;
            }

            cachedClearFlags = outputCamera.clearFlags;
            cachedCullingMask = outputCamera.cullingMask;
            outputCameraConfigured = true;

            outputCamera.clearFlags = CameraClearFlags.Depth;
            outputCamera.cullingMask &= ~(1 << BackgroundLayer);
        }

        private void RestoreOutputCamera()
        {
            if (outputCameraConfigured && outputCamera != null)
            {
                outputCamera.clearFlags = cachedClearFlags;
                outputCamera.cullingMask = cachedCullingMask;
            }

            outputCamera = null;
            outputCameraConfigured = false;
        }

        private Camera CreateBackgroundCamera()
        {
            GameObject cameraObj = new GameObject("BFBackgroundCamera");
            cameraObj.transform.SetParent(transform, false);

            Camera cam = cameraObj.AddComponent<Camera>();
            cam.orthographic = true;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
            cam.cullingMask = 1 << BackgroundLayer;
            cam.depth = -100;

            return cam;
        }

        private void SyncBackgroundCamera()
        {
            if (backgroundCamera == null)
                return;

            backgroundCamera.orthographicSize = Screen.height / 2f;
            backgroundCamera.transform.position = new Vector3(Screen.width / 2f, Screen.height / 2f, -10f);
        }
    }
}