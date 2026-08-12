using UnityEngine;

namespace BFTools.Systems.SceneManager
{
    public class BFSceneLoadRequestAsset : ScriptableObject
    {
        [SerializeField] private BFSceneLoadRequest request = new BFSceneLoadRequest();

        internal BFSceneLoadRequest Request => request;
    }
}