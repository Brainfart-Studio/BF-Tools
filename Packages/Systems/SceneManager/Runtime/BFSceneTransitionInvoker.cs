using UnityEngine;
using BFTools.Core.Logger;
using BFTools.Core.ServiceLocator;

namespace BFTools.Systems.SceneManager
{
    public class BFSceneTransitionInvoker : MonoBehaviour
    {
        private const string LogTag = "SceneManager";

        [SerializeField] private BFSceneLoadRequest request;

        public void Invoke()
        {
            if (request == null)
            {
                BFLogger.Error(LogTag, "No BFSceneLoadRequest assigned. Ignoring invoke.", this);
                return;
            }

            BFLogger.Debug(LogTag, $"Invoked. Transitioning to '{request.SceneName}'.", this);
            BFServiceLocator.Get<BFSceneTransitionController>().BeginTransition(request);
        }
    }
}