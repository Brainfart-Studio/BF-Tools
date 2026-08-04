using BFTools.Core.Logger;
using UnityEngine;

namespace BFTools.Visuals.Parallax
{
    internal class BFParallaxCameraTracker
    {
        private const string LogTag = "Parallax";

        private readonly Camera targetCameraOverride;
        private readonly Object context;

        private Camera trackedCamera;
        private Vector3 startPosition;
        private bool hasWarnedMissingCamera;

        public BFParallaxCameraTracker(Camera targetCameraOverride, Object context)
        {
            this.targetCameraOverride = targetCameraOverride;
            this.context = context;
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
            trackedCamera = targetCameraOverride != null ? targetCameraOverride : Camera.main;
            if (trackedCamera == null)
            {
                if (!hasWarnedMissingCamera)
                {
                    BFLogger.Error(LogTag, "BFParallaxCameraTracker: no camera found. Assign a Camera to Target Camera Override, or tag a camera MainCamera in the scene - otherwise parallax layers will never move.", context);
                    hasWarnedMissingCamera = true;
                }

                return;
            }

            startPosition = trackedCamera.transform.position;
        }
    }
}