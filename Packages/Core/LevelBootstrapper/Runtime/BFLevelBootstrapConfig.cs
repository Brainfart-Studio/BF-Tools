using UnityEngine;
namespace BFTools.Core.LevelBootstrapper
{
    public class BFLevelBootstrapConfig : ScriptableObject
    {
        [SerializeField] private GameObject[] prefabsToInstantiate;
        internal GameObject[] PrefabsToInstantiate => prefabsToInstantiate;
    }
}