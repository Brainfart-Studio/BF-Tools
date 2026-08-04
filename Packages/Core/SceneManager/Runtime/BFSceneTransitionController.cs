using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using BFTools.Core.Logger;

namespace BFTools.Core.SceneManager
{
    public class BFSceneTransitionController : MonoBehaviour
    {
        private const string LogTag = "SceneManager";

        [SerializeField] private BFFadeTransition fadeTransition;

        private string currentSceneName;
        private bool isTransitioning;

        private ITransition Transition => fadeTransition;

        public void BeginTransition(BFSceneLoadRequest request)
        {
            if (isTransitioning)
            {
                BFLogger.Warning(LogTag, $"Transition to '{request.SceneName}' requested while already transitioning. Ignoring.", this);
                return;
            }

            StartCoroutine(TransitionRoutine(request));
        }

        private IEnumerator TransitionRoutine(BFSceneLoadRequest request)
        {
            isTransitioning = true;
            string sceneName = request.SceneName;

            yield return Transition.PlayOut();

            if (BFSceneLoader.IsTracked(sceneName))
                yield return WaitForTask(BFSceneLoader.ActivateAsync(sceneName));
            else
                yield return WaitForTask(BFSceneLoader.LoadAsync(sceneName, request.LoadMode));

            yield return Transition.PlayIn();

            if (!string.IsNullOrEmpty(currentSceneName))
                yield return WaitForTask(BFSceneLoader.UnloadAsync(currentSceneName));

            currentSceneName = sceneName;
            isTransitioning = false;
        }

        private static IEnumerator WaitForTask(Task task)
        {
            while (!task.IsCompleted)
                yield return null;

            if (task.IsFaulted)
                throw task.Exception;
        }
    }
}