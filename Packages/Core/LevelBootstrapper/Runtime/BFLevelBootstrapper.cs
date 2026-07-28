using UnityEngine;
using BFTools.Core.Logger;

namespace BFTools.Core.LevelBootstrapper
{
    public class BFLevelBootstrapper : MonoBehaviour
    {
        private const string LogTag = "LevelBootstrapper";

        [SerializeField] private BFLevelBootstrapConfig config;

        private void Awake()
        {
            if (config == null)
            {
                BFLogger.Error(LogTag, "No LevelBootstrapConfig assigned.", this);
                return;
            }

            int spawned = 0;
            foreach (GameObject prefab in config.PrefabsToInstantiate)
            {
                if (prefab == null)
                    continue;
                Instantiate(prefab);
                spawned++;
            }

            BFLogger.Info(LogTag, $"Spawned {spawned} prefab(s).", this);
        }
    }
}