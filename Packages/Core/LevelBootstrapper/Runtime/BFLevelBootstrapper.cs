using UnityEngine;
namespace BFTools.Core.Bootstrapper
{
    public class BFLevelBootstrapper : MonoBehaviour
    {
        [SerializeField] private BFLevelBootstrapConfig config;

        private void Awake()
        {
            if (config == null)
            {
                Debug.LogError("BFLevelBootstrapper: No LevelBootstrapConfig assigned.", this);
                return;
            }
            foreach (GameObject prefab in config.PrefabsToInstantiate)
            {
                if (prefab == null)
                    continue;
                Instantiate(prefab);
            }
        }
    }
}