using BFTools.Core.CameraUtility;
using UnityEngine;

namespace BFTools.Visuals.Parallax
{
    internal class BFParallaxCameraTracker
    {
        private const string LogTag = "Parallax";

        private readonly BFCameraResolver cameraResolver;

        private Camera trackedCamera;
        private Vector3 startPosition;

        public BFParallaxCameraTracker(Camera targetCameraOverride, Object context)
        {
            cameraResolver = new BFCameraResolver(targetCameraOverride, context, LogTag,
                "BFParallaxCameraTracker: no camera found. Assign a Camera to Target Camera Override, or tag a camera MainCamera in the scene - otherwise parallax layers will never move.");
        }

        public void Init()
        {
            ResolveCamera();
        }

        public Vector2 Sync()
        {
            // Retry each frame until resolved: the scene's main camera may not exist yet
            // if the manager's OnEnable runs before it (script execution order, async
            // scene loads), and a dropped-in prefab shouldn't depend on getting that
            // ordering right.
            if (trackedCamera == null)
            {
                ResolveCamera();
                if (trackedCamera == null)
                    return Vector2.zero;
            }

            return trackedCamera.transform.position - startPosition;
        }

        public void Cleanup()
        {
            trackedCamera = null;
        }

        private void ResolveCamera()
        {
            trackedCamera = cameraResolver.Resolve();
            if (trackedCamera != null)
                startPosition = trackedCamera.transform.position;
        }
    }
}