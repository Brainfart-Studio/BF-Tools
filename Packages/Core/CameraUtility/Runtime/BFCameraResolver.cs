using BFTools.Core.Logger;
using UnityEngine;

namespace BFTools.Core.CameraUtility
{
    public class BFCameraResolver
    {
        private readonly Camera targetCameraOverride;
        private readonly Object context;
        private readonly string logTag;
        private readonly string missingCameraMessage;

        private bool hasWarnedMissingCamera;

        public BFCameraResolver(Camera targetCameraOverride, Object context, string logTag, string missingCameraMessage)
        {
            this.targetCameraOverride = targetCameraOverride;
            this.context = context;
            this.logTag = logTag;
            this.missingCameraMessage = missingCameraMessage;
        }

        public Camera Resolve()
        {
            Camera resolvedCamera = targetCameraOverride != null ? targetCameraOverride : Camera.main;

            if (resolvedCamera == null)
            {
                if (!hasWarnedMissingCamera)
                {
                    BFLogger.Error(logTag, missingCameraMessage, context);
                    hasWarnedMissingCamera = true;
                }

                return null;
            }

            hasWarnedMissingCamera = false;
            return resolvedCamera;
        }
    }
}