using UnityEngine;

namespace BFTools.Core.ProjectSetup.Editor
{
    public interface IBFSystemPrefabContributor
    {
        GameObject SystemPrefab { get; }
    }
}