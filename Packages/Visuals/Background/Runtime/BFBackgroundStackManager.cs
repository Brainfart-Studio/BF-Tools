using System.Collections.Generic;
using BFTools.Core.Logger;
using UnityEngine;

namespace BFTools.Visuals.Background
{
    public class BFBackgroundStackManager : MonoBehaviour
    {
        private const string LogTag = "Background";

        public const int BackgroundLayer = 30;

        [SerializeField] private List<BFBackgroundStackConfig> stacks = new List<BFBackgroundStackConfig>();
        [SerializeField] private Camera targetCameraOverride;

        private static BFBackgroundStackManager instance;

        private readonly List<BFBackgroundStack> activeStacks = new List<BFBackgroundStack>();
        private BFBackgroundStackCamera stackCamera;

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
                BFLogger.Error(LogTag, "BFBackgroundStackManager: another instance is already active. Only one is supported at a time.", this);
                enabled = false;
                return;
            }

            instance = this;

            stackCamera = new BFBackgroundStackCamera(targetCameraOverride, this);
            stackCamera.Init();

            BuildStacks();

            int totalLayers = 0;
            for (int i = 0; i < activeStacks.Count; i++)
                totalLayers += activeStacks[i].LayerCount;

            BFLogger.Info(LogTag, $"BFBackgroundStackManager: enabled with {activeStacks.Count} stack(s), {totalLayers} layer(s) total.", this);
        }

        private void Update()
        {
            stackCamera.Sync();

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

            stackCamera.Cleanup();
            stackCamera = null;

            instance = null;

            BFLogger.Info(LogTag, "BFBackgroundStackManager: disabled and cleaned up.", this);
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
                    BFLogger.Warning(LogTag, "BFBackgroundStackManager: skipping empty stack slot.", this);
                    continue;
                }

                BFBackgroundStack stack = new BFBackgroundStack(stackConfig);
                stack.Init(transform, sortingOrder);
                sortingOrder += stack.LayerCount;

                activeStacks.Add(stack);
            }
        }
    }
}