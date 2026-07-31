using UnityEngine;

namespace BFTools.Visuals.Background
{
    // Shared by every active BFBackgroundManager so concurrent effects (crossfades between
    // backgrounds, a persistent background alongside a modal one, split-screen, etc.) render
    // into a single camera instead of one each. Multiple cameras culling the same reserved
    // layer would each draw every effect's objects, doubling every effect's output.
    internal static class BFBackgroundSharedCamera
    {
        private static Camera camera;
        private static int refCount;

        public static Camera Acquire()
        {
            if (refCount == 0)
                camera = CreateCamera();

            refCount++;
            return camera;
        }

        public static void Release()
        {
            if (refCount <= 0)
                return;

            refCount--;

            if (refCount == 0 && camera != null)
            {
                Object.Destroy(camera.gameObject);
                camera = null;
            }
        }

        public static void Sync()
        {
            if (camera == null)
                return;

            camera.orthographicSize = Screen.height / 2f;
            camera.transform.position = new Vector3(Screen.width / 2f, Screen.height / 2f, -10f);
        }

        private static Camera CreateCamera()
        {
            GameObject cameraObj = new GameObject("BFBackgroundCamera");

            Camera cam = cameraObj.AddComponent<Camera>();
            cam.orthographic = true;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
            cam.cullingMask = 1 << BFBackgroundManager.BackgroundLayer;
            cam.depth = -100;

            return cam;
        }
    }
}