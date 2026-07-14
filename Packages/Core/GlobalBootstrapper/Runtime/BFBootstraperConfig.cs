using UnityEngine;
namespace BFTools.Core.Bootstrapper
{
    public class BootstrapConfig : ScriptableObject
    {
        [SerializeField] private GameObject[] systemPrefabs;
        internal GameObject[] SystemPrefabs => systemPrefabs;
    }
}