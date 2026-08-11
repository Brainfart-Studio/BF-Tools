using UnityEngine;
using BFTools.Core.Logger;

namespace BFTools.Systems.SceneManager
{
    public class BFPreloadZoneTrigger : MonoBehaviour
    {
        private const string LogTag = "SceneManager";

        [SerializeField] private BFSceneLoadRequest request = new BFSceneLoadRequest();
        [SerializeField] private BFSceneLoadRequestAsset sharedRequest;
        [SerializeField] private string playerTag = "Player";

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag(playerTag))
                return;

            BFSceneLoadRequest activeRequest = ResolveRequest();
            if (activeRequest == null)
            {
                BFLogger.Error(LogTag, "No scene name configured. Ignoring preload zone trigger.", this);
                return;
            }

            BFLogger.Debug(LogTag, $"Preload zone entered by '{other.name}'. Preloading '{activeRequest.SceneName}'.", this);
            BFSceneLoader.Preload(activeRequest.SceneName, activeRequest.LoadMode);
        }

        private BFSceneLoadRequest ResolveRequest()
        {
            BFSceneLoadRequest activeRequest = sharedRequest != null ? sharedRequest.Request : request;
            return activeRequest.HasSceneName ? activeRequest : null;
        }
    }
}