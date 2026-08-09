using UnityEngine;
using BFTools.Core.Logger;

namespace BFTools.Systems.LevelBootstrapper
{
    public class BFLevelBootstrapper : MonoBehaviour
    {
        private const string LogTag = "LevelBootstrapper";

        [SerializeField] private BFLevelBootstrapConfig config;

        private static System.Func<GameObject, GameObject> instantiateFunc = prefab => Object.Instantiate(prefab);

        private void Awake()
        {
            if (config == null)
            {
                BFLogger.Error(LogTag, "No LevelBootstrapConfig assigned.", this);
                return;
            }

            GameObject[] prefabs = config.PrefabsToInstantiate;
            if (prefabs == null)
            {
                BFLogger.Error(LogTag, "LevelBootstrapConfig has a null PrefabsToInstantiate array.", this);
                return;
            }

            int spawned = 0;
            foreach (GameObject prefab in prefabs)
            {
                if (prefab == null)
                    continue;

                try
                {
                    instantiateFunc(prefab);
                    spawned++;
                }
                catch (System.Exception ex)
                {
                    BFLogger.Error(LogTag, $"Failed to instantiate prefab '{prefab.name}': {ex.Message}", this);
                }
            }

            BFLogger.Info(LogTag, $"Spawned {spawned} prefab(s).", this);
        }
    }
}