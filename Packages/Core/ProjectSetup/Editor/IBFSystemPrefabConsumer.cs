using UnityEngine;

namespace BFTools.Core.ProjectSetup.Editor
{
    public interface IBFSystemPrefabConsumer
    {
        void AssignSystemPrefabs(GameObject[] prefabs);
    }
}