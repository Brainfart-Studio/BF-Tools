using UnityEngine;
using BFTools.Core.Logger;
using BFTools.Core.ServiceLocator;

namespace BFTools.Systems.SceneManager
{
    public class BFSceneTransitionInvoker : MonoBehaviour
    {
        private const string LogTag = "SceneManager";

        [SerializeField] private BFSceneLoadRequest request = new BFSceneLoadRequest();
        [SerializeField] private BFSceneLoadRequestAsset sharedRequest;

        public void Invoke()
        {
            BFSceneLoadRequest activeRequest = ResolveRequest();
            if (activeRequest == null)
            {
                BFLogger.Error(LogTag, "No scene name configured. Ignoring invoke.", this);
                return;
            }

            BFLogger.Debug(LogTag, $"Invoked. Transitioning to '{activeRequest.SceneName}'.", this);
            BFServiceLocator.Get<BFSceneTransitionController>().BeginTransition(activeRequest);
        }

        private BFSceneLoadRequest ResolveRequest()
        {
            BFSceneLoadRequest activeRequest = sharedRequest != null ? sharedRequest.Request : request;
            return activeRequest.HasSceneName ? activeRequest : null;
        }
    }
}