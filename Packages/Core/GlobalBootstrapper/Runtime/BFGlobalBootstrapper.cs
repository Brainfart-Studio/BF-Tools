using UnityEngine;

namespace BFTools.Core.Bootstrapper
{
    public static class BFGlobalBootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            BootstrapConfig config = Resources.Load<BootstrapConfig>("BFTools/BootstrapConfig");

            if (config == null)
            {
                Debug.LogError("BFGlobalBootstrapper: No BootstrapConfig found at Resources/BFTools/BootstrapConfig.");
                return;
            }

            foreach (GameObject prefab in config.systemPrefabs)
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