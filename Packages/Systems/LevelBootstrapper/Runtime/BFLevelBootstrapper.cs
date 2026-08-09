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

            BFLogger.Trace(LogTag, $"Loaded LevelBootstrapConfig with {prefabs.Length} prefab entr{(prefabs.Length == 1 ? "y" : "ies")}.", this);

            int spawned = 0;
            for (int i = 0; i < prefabs.Length; i++)
            {
                GameObject prefab = prefabs[i];
                if (prefab == null)
                {
                    BFLogger.Warning(LogTag, $"Skipped null prefab entry at index {i}.", this);
                    continue;
                }

                try
                {
                    instantiateFunc(prefab);
                    spawned++;
                    BFLogger.Debug(LogTag, $"Instantiated prefab '{prefab.name}'.", this);
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