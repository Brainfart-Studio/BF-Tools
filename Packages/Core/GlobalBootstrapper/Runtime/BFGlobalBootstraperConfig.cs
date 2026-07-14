using UnityEngine;
namespace BFTools.Core.Bootstrapper
{
    public class BFGlobalBootstrapConfig : ScriptableObject
    {
        [SerializeField] private GameObject[] systemPrefabs;
        internal GameObject[] SystemPrefabs => systemPrefabs;
    }
}