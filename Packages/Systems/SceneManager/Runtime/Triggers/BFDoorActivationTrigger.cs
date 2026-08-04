using UnityEngine;
using BFTools.Core.Logger;
using BFTools.Core.ServiceLocator;

namespace BFTools.Systems.SceneManager
{
    public class BFDoorActivationTrigger : MonoBehaviour
    {
        private const string LogTag = "SceneManager";

        [SerializeField] private BFSceneLoadRequest request;
        [SerializeField] private string playerTag = "Player";

        private bool suppressed;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (suppressed || !other.CompareTag(playerTag))
                return;

            suppressed = true;
            BFLogger.Debug(LogTag, $"Door entered by '{other.name}'. Transitioning to '{request.SceneName}'.", this);
            BFServiceLocator.Get<BFSceneTransitionController>().BeginTransition(request);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag(playerTag))
                suppressed = false;
        }
    }
}