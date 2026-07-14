using UnityEngine;

namespace BFTools.Core.GlobalBootstrapper
{
    public static class BFGlobalBootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            BFGlobalBootstrapConfig config = Resources.Load<BFGlobalBootstrapConfig>("BFTools/GlobalBootstrapConfig");

            if (config == null)
            {
                Debug.LogError("BFGlobalBootstrapper: No GlobalBootstrapConfig found at Resources/BFTools/GlobalBootstrapConfig.");
                return;
            }

            foreach (GameObject prefab in config.SystemPrefabs)
            {
                if (prefab == null)
                    continue;

                GameObject instance = Object.Instantiate(prefab);
                instance.transform.SetParent(null); // ensure root-level, required for DontDestroyOnLoad
                Object.DontDestroyOnLoad(instance);
            }
        }
    }
}