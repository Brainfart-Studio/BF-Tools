using UnityEngine;
namespace BFTools.Core.GlobalBootstrapper
{
    public class BFGlobalBootstrapConfig : ScriptableObject
    {
        [SerializeField] private GameObject[] systemPrefabs;
        internal GameObject[] SystemPrefabs => systemPrefabs;
    }
}