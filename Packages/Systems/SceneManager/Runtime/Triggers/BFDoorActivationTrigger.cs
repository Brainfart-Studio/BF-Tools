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

        [Header("Inline Request (used when Request is unassigned)")]
        [SerializeField] private BFInlineSceneLoadRequest inlineRequest;

        private bool suppressed;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (suppressed || !other.CompareTag(playerTag))
                return;

            BFSceneLoadRequest activeRequest = inlineRequest.Resolve(request);
            if (activeRequest == null)
            {
                BFLogger.Error(LogTag, "No BFSceneLoadRequest assigned and no inline scene name set. Ignoring door trigger.", this);
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
    }
}