using UnityEngine;

namespace BFTools.Core.Bootstrapper
{
    [CreateAssetMenu(menuName = "BFTools/Config/Bootstrap Config", fileName = "BootstrapConfig")]
    public class BootstrapConfig : ScriptableObject
    {
        public GameObject[] systemPrefabs;
    }
}