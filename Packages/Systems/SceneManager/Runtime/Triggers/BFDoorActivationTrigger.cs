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

            BFSceneLoadRequest activeRequest = BFSceneLoadRequestResolver.Resolve(request, sharedRequest);
            if (activeRequest == null)
            {
                BFLogger.Error(LogTag, "No scene name configured. Ignoring door trigger.", this);
                return;
            }

            if (!BFServiceLocator.TryGet(out BFSceneTransitionController controller))
            {
                BFLogger.Error(LogTag, "No BFSceneTransitionController is registered. Ignoring door trigger.", this);
                return;
            }

            suppressed = true;
            BFLogger.Debug(LogTag, $"Door entered by '{other.name}'. Transitioning to '{activeRequest.SceneName}'.", this);
            controller.BeginTransition(activeRequest);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag(playerTag))
                suppressed = false;
        }
    }
}