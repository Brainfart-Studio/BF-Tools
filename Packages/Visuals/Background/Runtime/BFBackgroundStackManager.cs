using System.Collections.Generic;
using UnityEngine;

namespace BFTools.Visuals.Background
{
    public class BFBackgroundStackManager : MonoBehaviour
    {
        public const int BackgroundLayer = 30;

        [SerializeField] private List<BFBackgroundStackConfig> stacks = new List<BFBackgroundStackConfig>();
        [SerializeField] private Camera targetCameraOverride;

        private static BFBackgroundStackManager instance;

        private readonly List<BFBackgroundStack> activeStacks = new List<BFBackgroundStack>();

        private Camera backgroundCamera;
        private Camera outputCamera;
        private CameraClearFlags outputCameraOriginalClearFlags;
        private int outputCameraOriginalCullingMask;
        private bool outputCameraConfigured;
        private bool hasWarnedMissingCamera;

        private void OnEnable()
        {
            if (instance != null)
            {
                Debug.LogError("BFBackgroundStackManager: another instance is already active. Only one is supported at a time.", this);
                enabled = false;
                return;
            }

            instance = this;
            hasWarnedMissingCamera = false;

            backgroundCamera = CreateBackgroundCamera();
            SyncBackgroundCamera();

            ConfigureOutputCamera();

            BuildStacks();
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

            for (int i = 0; i < activeStacks.Count; i++)
                activeStacks[i].Tick(Time.deltaTime);
        }

        private void OnDisable()
        {
            if (instance != this)
                return;

            for (int i = 0; i < activeStacks.Count; i++)
                activeStacks[i].Cleanup();
            activeStacks.Clear();

            RestoreOutputCamera();

            if (backgroundCamera != null)
                Destroy(backgroundCamera.gameObject);
            backgroundCamera = null;

            instance = null;
        }

        private void BuildStacks()
        {
            activeStacks.Clear();

            int sortingOrder = 0;
            for (int i = 0; i < stacks.Count; i++)
            {
                BFBackgroundStackConfig stackConfig = stacks[i];
                if (stackConfig == null)
                {
                    Debug.LogWarning("BFBackgroundStackManager: skipping empty stack slot.", this);
                    continue;
                }

                BFBackgroundStack stack = new BFBackgroundStack(stackConfig);
                stack.Init(transform, sortingOrder);
                sortingOrder += stack.LayerCount;

                activeStacks.Add(stack);
            }
        }

        private Camera CreateBackgroundCamera()
        {
            GameObject cameraObj = new GameObject("BFBackgroundStackCamera");

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

        private void ConfigureOutputCamera()
        {
            outputCamera = targetCameraOverride != null ? targetCameraOverride : Camera.main;
            if (outputCamera == null)
            {
                if (!hasWarnedMissingCamera)
                {
                    Debug.LogError("BFBackgroundStackManager: no output camera found. Assign a Camera to Target Camera Override, or tag a camera MainCamera in the scene - otherwise this background will never be visible.", this);
                    hasWarnedMissingCamera = true;
                }

                return;
            }

            outputCameraOriginalClearFlags = outputCamera.clearFlags;
            outputCameraOriginalCullingMask = outputCamera.cullingMask;

            outputCamera.clearFlags = CameraClearFlags.Depth;
            outputCamera.cullingMask &= ~(1 << BackgroundLayer);

            outputCameraConfigured = true;
        }

        private void RestoreOutputCamera()
        {
            if (outputCameraConfigured)
            {
                outputCamera.clearFlags = outputCameraOriginalClearFlags;
                outputCamera.cullingMask = outputCameraOriginalCullingMask;
            }

            outputCamera = null;
            outputCameraConfigured = false;
        }
    }
}