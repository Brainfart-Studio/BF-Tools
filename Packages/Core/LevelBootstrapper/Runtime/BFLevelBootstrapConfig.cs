using UnityEngine;
namespace BFTools.Core.Bootstrapper
{
    public class BFLevelBootstrapConfig : ScriptableObject
    {
        [SerializeField] private GameObject[] prefabsToInstantiate;
        internal GameObject[] PrefabsToInstantiate => prefabsToInstantiate;
    }
}