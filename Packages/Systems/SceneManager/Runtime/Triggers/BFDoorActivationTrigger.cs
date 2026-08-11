using UnityEngine;
using BFTools.Core.Logger;
using BFTools.Core.ServiceLocator;

namespace BFTools.Systems.SceneManager
{
    public class BFDoorActivationTrigger : MonoBehaviour
    {
        private const string LogTag = "SceneManager";

        [SerializeField] private BFSceneLoadRequest request = new BFSceneLoadRequest();
        [SerializeField] private BFSceneLoadRequestAsset sharedRequest;
        [SerializeField] private string playerTag = "Player";

        private bool suppressed;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (suppressed || !other.CompareTag(playerTag))
                return;

            BFSceneLoadRequest activeRequest = ResolveRequest();
            if (activeRequest == null)
            {
                BFLogger.Error(LogTag, "No scene name configured. Ignoring door trigger.", this);
                return;
            }

            suppressed = true;
            BFLogger.Debug(LogTag, $"Door entered by '{other.name}'. Transitioning to '{activeRequest.SceneName}'.", this);
            BFServiceLocator.Get<BFSceneTransitionController>().BeginTransition(activeRequest);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag(playerTag))
                suppressed = false;
        }

        private BFSceneLoadRequest ResolveRequest()
        {
            BFSceneLoadRequest activeRequest = sharedRequest != null ? sharedRequest.Request : request;
            return activeRequest.HasSceneName ? activeRequest : null;
        }
    }
}