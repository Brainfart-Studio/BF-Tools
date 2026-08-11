using UnityEngine;
using BFTools.Core.Logger;
using BFTools.Core.ServiceLocator;

namespace BFTools.Systems.SceneManager
{
    public class BFSceneTransitionInvoker : MonoBehaviour
    {
        private const string LogTag = "SceneManager";

        [SerializeField] private BFSceneLoadRequest request;

        [Header("Inline Request (used when Request is unassigned)")]
        [SerializeField] private BFInlineSceneLoadRequest inlineRequest = new BFInlineSceneLoadRequest();

        public void Invoke()
        {
            BFSceneLoadRequest activeRequest = inlineRequest.Resolve(request);
            if (activeRequest == null)
            {
                BFLogger.Error(LogTag, "No BFSceneLoadRequest assigned and no inline scene name set. Ignoring invoke.", this);
                return;
            }

            BFLogger.Debug(LogTag, $"Invoked. Transitioning to '{activeRequest.SceneName}'.", this);
            BFServiceLocator.Get<BFSceneTransitionController>().BeginTransition(activeRequest);
        }
    }
}