using UnityEngine;
using BFTools.Core.Logger;

namespace BFTools.Systems.GlobalBootstrapper
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

            GameObject[] prefabs = config.SystemPrefabs;
            if (prefabs == null)
            {
                BFLogger.Error(LogTag, "GlobalBootstrapConfig has a null SystemPrefabs array.");
                return;
            }

            BFLogger.Trace(LogTag, $"Loaded GlobalBootstrapConfig with {prefabs.Length} prefab entr{(prefabs.Length == 1 ? "y" : "ies")}.");

            int spawned = 0;
            for (int i = 0; i < prefabs.Length; i++)
            {
                GameObject prefab = prefabs[i];
                if (prefab == null)
                {
                    BFLogger.Warning(LogTag, $"Skipped null prefab entry at index {i}.");
                    continue;
                }

                try
                {
                    GameObject instance = Object.Instantiate(prefab);
                    instance.transform.SetParent(null); // ensure root-level, required for DontDestroyOnLoad
                    Object.DontDestroyOnLoad(instance);
                    spawned++;
                    BFLogger.Debug(LogTag, $"Instantiated system prefab '{prefab.name}'.");
                }
                catch (System.Exception ex)
                {
                    BFLogger.Error(LogTag, $"Failed to instantiate system prefab '{prefab.name}': {ex.Message}");
                }
            }

            BFLogger.Info(LogTag, $"Spawned {spawned} system prefab(s).");
        }
    }
}