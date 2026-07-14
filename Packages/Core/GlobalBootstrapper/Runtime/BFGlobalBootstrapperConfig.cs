using UnityEngine;
namespace BFTools.Core.GlobalBootstrapper
{
    public class BFGlobalBootstrapperConfig : ScriptableObject
    {
        [SerializeField] private GameObject[] systemPrefabs;
        internal GameObject[] SystemPrefabs => systemPrefabs;
    }
}