using UnityEngine;
namespace BFTools.Systems.LevelBootstrapper
{
    public class BFLevelBootstrapConfig : ScriptableObject
    {
        [SerializeField] private GameObject[] prefabsToInstantiate;
        internal GameObject[] PrefabsToInstantiate => prefabsToInstantiate;
    }
}