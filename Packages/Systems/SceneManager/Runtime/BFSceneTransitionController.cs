using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using BFTools.Core.EventBus;
using BFTools.Core.Logger;
using BFTools.Core.ServiceLocator;

namespace BFTools.Systems.SceneManager
{
    public struct BFSceneTransitionStartedEvent
    {
        public string sceneName;
        public bool showLoadingScreen;
    }

    public struct BFSceneLoadedEvent
    {
        public string sceneName;
    }

    public struct BFSceneTransitionCompleteEvent
    {
        public string sceneName;
    }

    public class BFSceneTransitionController : MonoBehaviour
    {
        private const string LogTag = "SceneManager";

        [SerializeField] private BFFadeTransition fadeTransition;

        private string currentSceneName;
        private bool isTransitioning;

        private ITransition Transition => fadeTransition;

        private void Awake()
        {
            BFServiceLocator.Register(this);
            BFLogger.Trace(LogTag, "Registered with ServiceLocator");
        }

        private void OnDestroy()
        {
            BFServiceLocator.Unregister<BFSceneTransitionController>();
            BFLogger.Trace(LogTag, "Unregistered from ServiceLocator");
        }

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

            EventBus<BFSceneTransitionStartedEvent>.Fire(new BFSceneTransitionStartedEvent
            {
                sceneName = sceneName,
                showLoadingScreen = request.ShowLoadingScreen
            });

            yield return Transition.PlayOut();

            float loadStartTime = Time.time;

            if (BFSceneLoader.IsTracked(sceneName))
                yield return WaitForTask(BFSceneLoader.ActivateAsync(sceneName));
            else
                yield return WaitForTask(BFSceneLoader.LoadAsync(sceneName, request.LoadMode));

            float remainingDisplayTime = request.MinimumDisplayTime - (Time.time - loadStartTime);
            if (remainingDisplayTime > 0f)
                yield return new WaitForSeconds(remainingDisplayTime);

            EventBus<BFSceneLoadedEvent>.Fire(new BFSceneLoadedEvent { sceneName = sceneName });

            yield return Transition.PlayIn();

            if (!string.IsNullOrEmpty(currentSceneName))
                yield return WaitForTask(BFSceneLoader.UnloadAsync(currentSceneName));

            currentSceneName = sceneName;
            isTransitioning = false;

            EventBus<BFSceneTransitionCompleteEvent>.Fire(new BFSceneTransitionCompleteEvent { sceneName = sceneName });
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