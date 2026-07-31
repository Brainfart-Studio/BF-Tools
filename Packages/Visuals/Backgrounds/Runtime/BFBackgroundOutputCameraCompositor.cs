using System.Collections.Generic;
using UnityEngine;

namespace BFTools.Visuals.Background
{
    // Reference-counted per output camera. Without this, a second BFBackgroundManager
    // targeting the same output camera would cache that camera's already-modified state as
    // if it were the original, and whichever manager disables first would strip the
    // compositing setup out from under any other manager still relying on it.
    internal static class BFBackgroundOutputCameraCompositor
    {
        private class Entry
        {
            public int RefCount;
            public CameraClearFlags OriginalClearFlags;
            public int OriginalCullingMask;
        }

        private static readonly Dictionary<Camera, Entry> entries = new Dictionary<Camera, Entry>();

        public static void Acquire(Camera camera)
        {
            if (camera == null)
                return;

            if (!entries.TryGetValue(camera, out Entry entry))
            {
                entry = new Entry
                {
                    OriginalClearFlags = camera.clearFlags,
                    OriginalCullingMask = camera.cullingMask
                };
                entries[camera] = entry;

                camera.clearFlags = CameraClearFlags.Depth;
                camera.cullingMask &= ~(1 << BFBackgroundManager.BackgroundLayer);
            }

            entry.RefCount++;
        }

        public static void Release(Camera camera)
        {
            if (camera == null)
                return;

            if (!entries.TryGetValue(camera, out Entry entry))
                return;

            entry.RefCount--;
            if (entry.RefCount > 0)
                return;

            camera.clearFlags = entry.OriginalClearFlags;
            camera.cullingMask = entry.OriginalCullingMask;
            entries.Remove(camera);
        }
    }
}