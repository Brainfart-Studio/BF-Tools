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

        private IBFBackgroundEffect activeEffect;
        private Camera backgroundCamera;

        private void OnEnable()
        {
            if (config == null)
                return;

            backgroundCamera = CreateBackgroundCamera();
            SyncBackgroundCamera();

            activeEffect = config.CreateEffect();
            activeEffect.Init(transform);
        }

        private void Update()
        {
            SyncBackgroundCamera();
            activeEffect?.Tick(Time.deltaTime);
        }

        private void OnDisable()
        {
            activeEffect?.Cleanup();
            activeEffect = null;

            if (backgroundCamera != null)
                Destroy(backgroundCamera.gameObject);
            backgroundCamera = null;
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