using UnityEngine;
namespace BFTools.Systems.GlobalBootstrapper
{
    public class BFGlobalBootstrapperConfig : ScriptableObject
    {
        [SerializeField] private GameObject[] systemPrefabs;
        internal GameObject[] SystemPrefabs => systemPrefabs;
    }
}