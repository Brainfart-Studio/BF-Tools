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
        private BFBackgroundStackCamera stackCamera;

        private void OnEnable()
        {
            if (instance != null)
            {
                Debug.LogError("BFBackgroundStackManager: another instance is already active. Only one is supported at a time.", this);
                enabled = false;
                return;
            }

            instance = this;

            stackCamera = new BFBackgroundStackCamera(targetCameraOverride, this);
            stackCamera.Init();

            BuildStacks();
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
    }
}