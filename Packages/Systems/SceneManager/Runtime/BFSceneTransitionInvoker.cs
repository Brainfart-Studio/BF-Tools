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
            BFSceneLoadRequest activeRequest = BFSceneLoadRequestResolver.Resolve(request, sharedRequest);
            if (activeRequest == null)
            {
                BFLogger.Error(LogTag, "No scene name configured. Ignoring invoke.", this);
                return;
            }

            if (!BFServiceLocator.TryGet(out BFSceneTransitionController controller))
            {
                BFLogger.Error(LogTag, "No BFSceneTransitionController is registered. Ignoring invoke.", this);
                return;
            }

            BFLogger.Debug(LogTag, $"Invoked. Transitioning to '{activeRequest.SceneName}'.", this);
            controller.BeginTransition(activeRequest);
        }
    }
}