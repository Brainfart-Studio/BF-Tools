using UnityEngine;
using BFTools.Core.Logger;

namespace BFTools.Core.GlobalBootstrapper
{
    public static class BFGlobalBootstrapper
    {
        private const string LogTag = "GlobalBootstrapper";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            BFGlobalBootstrapperConfig config = Resources.Load<BFGlobalBootstrapperConfig>("BFTools/GlobalBootstrapConfig");
            if (config == null)
            {
                BFLogger.Error(LogTag, "No GlobalBootstrapConfig found at Resources/BFTools/GlobalBootstrapConfig.");
                return;
            }

            int spawned = 0;
            foreach (GameObject prefab in config.SystemPrefabs)
            {
                if (prefab == null)
                    continue;
                GameObject instance = Object.Instantiate(prefab);
                instance.transform.SetParent(null); // ensure root-level, required for DontDestroyOnLoad
                Object.DontDestroyOnLoad(instance);
                spawned++;
            }

            BFLogger.Info(LogTag, $"Spawned {spawned} system prefab(s).");
        }
    }
}