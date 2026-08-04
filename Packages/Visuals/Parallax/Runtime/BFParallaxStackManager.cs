using System.Collections.Generic;
using BFTools.Core.Logger;
using UnityEngine;

namespace BFTools.Visuals.Parallax
{
    public class BFParallaxStackManager : MonoBehaviour
    {
        private const string LogTag = "Parallax";

        [SerializeField] private List<BFParallaxStackConfig> stacks = new List<BFParallaxStackConfig>();
        [SerializeField] private Camera targetCameraOverride;

        private static BFParallaxStackManager instance;

        private readonly List<BFParallaxStack> activeStacks = new List<BFParallaxStack>();
        private BFParallaxCameraTracker cameraTracker;

        // Runs on every Play session regardless of domain reload state. Without this,
        // disabling domain reload (Edit > Project Settings > Editor > Enter Play Mode
        // Options) leaves a stale instance reference from the previous session, and the
        // next Play falsely reports a duplicate instance.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            instance = null;
        }

        private void OnEnable()
        {
            if (instance != null)
            {
                BFLogger.Error(LogTag, "BFParallaxStackManager: another instance is already active. Only one is supported at a time.", this);
                enabled = false;
                return;
            }

            instance = this;

            cameraTracker = new BFParallaxCameraTracker(targetCameraOverride, this);
            cameraTracker.Init();

            BuildStacks();

            int totalLayers = 0;
            for (int i = 0; i < activeStacks.Count; i++)
                totalLayers += activeStacks[i].LayerCount;

            BFLogger.Info(LogTag, $"BFParallaxStackManager: enabled with {activeStacks.Count} stack(s), {totalLayers} layer(s) total.", this);
        }

        private void Update()
        {
            Vector2 cameraDisplacement = cameraTracker.Sync();

            for (int i = 0; i < activeStacks.Count; i++)
                activeStacks[i].Tick(cameraDisplacement, Time.deltaTime);
        }

        private void OnDisable()
        {
            if (instance != this)
                return;

            for (int i = 0; i < activeStacks.Count; i++)
                activeStacks[i].Cleanup();
            activeStacks.Clear();

            cameraTracker.Cleanup();
            cameraTracker = null;

            instance = null;

            BFLogger.Info(LogTag, "BFParallaxStackManager: disabled and cleaned up.", this);
        }

        private void BuildStacks()
        {
            activeStacks.Clear();

            int sortingOrder = 0;
            for (int i = 0; i < stacks.Count; i++)
            {
                BFParallaxStackConfig stackConfig = stacks[i];
                if (stackConfig == null)
                {
                    BFLogger.Warning(LogTag, "BFParallaxStackManager: skipping empty stack slot.", this);
                    continue;
                }

                BFParallaxStack stack = new BFParallaxStack(stackConfig);
                stack.Init(transform, sortingOrder);
                sortingOrder += stack.LayerCount;

                activeStacks.Add(stack);
            }
        }
    }
}