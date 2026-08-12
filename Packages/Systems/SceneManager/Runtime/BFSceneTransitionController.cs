using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;
using BFTools.Core.EventBus;
using BFTools.Core.Logger;
using BFTools.Core.ServiceLocator;
using BFTools.Core.SingletonGuard;

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

        [SerializeField, FormerlySerializedAs("fadeTransition")] private BFTransitionBehaviour transition;

        private string currentSceneName;
        private bool isTransitioning;

        private ITransition Transition => transition;

        private void Awake()
        {
            if (!BFActiveInstanceGuard<BFSceneTransitionController>.TryActivate(this))
            {
                BFLogger.Warning(LogTag, "Duplicate BFSceneTransitionController detected. Destroying this instance.", this);
                Destroy(gameObject);
                return;
            }

            BFServiceLocator.Register(this);
            BFLogger.Trace(LogTag, "Registered with ServiceLocator");
        }

        private void OnDestroy()
        {
            if (!BFActiveInstanceGuard<BFSceneTransitionController>.IsActive(this))
                return;

            BFActiveInstanceGuard<BFSceneTransitionController>.Deactivate(this);
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

            if (transition == null)
            {
                BFLogger.Error(LogTag, "No transition assigned. Ignoring transition request.", this);
                return;
            }

            StartCoroutine(TransitionRoutine(request));
        }

        private IEnumerator TransitionRoutine(BFSceneLoadRequest request)
        {
            isTransitioning = true;

            try
            {
                string sceneName = request.SceneName;
                BFLogger.Debug(LogTag, $"Transition started for '{sceneName}'.", this);

                EventBus<BFSceneTransitionStartedEvent>.Fire(new BFSceneTransitionStartedEvent
                {
                    sceneName = sceneName,
                    showLoadingScreen = request.ShowLoadingScreen
                });

                BFLogger.Trace(LogTag, "Playing out transition.", this);
                yield return Transition.PlayOut();

                float loadStartTime = Time.time;

                if (BFSceneLoader.IsTracked(sceneName))
                {
                    BFLogger.Debug(LogTag, $"'{sceneName}' is preloaded. Activating.", this);
                    yield return WaitForTask(BFSceneLoader.ActivateAsync(sceneName));
                }
                else
                {
                    BFLogger.Debug(LogTag, $"'{sceneName}' is not preloaded. Loading.", this);
                    yield return WaitForTask(BFSceneLoader.LoadAsync(sceneName, request.LoadMode));
                }

                float remainingDisplayTime = request.MinimumDisplayTime - (Time.time - loadStartTime);
                if (remainingDisplayTime > 0f)
                {
                    BFLogger.Trace(LogTag, $"Holding for remaining minimum display time of {remainingDisplayTime:0.###}s.", this);
                    yield return new WaitForSeconds(remainingDisplayTime);
                }

                EventBus<BFSceneLoadedEvent>.Fire(new BFSceneLoadedEvent { sceneName = sceneName });

                BFLogger.Trace(LogTag, "Playing in transition.", this);
                yield return Transition.PlayIn();

                if (!string.IsNullOrEmpty(currentSceneName))
                {
                    BFLogger.Debug(LogTag, $"Unloading previous scene '{currentSceneName}'.", this);
                    yield return WaitForTask(BFSceneLoader.UnloadAsync(currentSceneName));
                }

                currentSceneName = sceneName;

                BFLogger.Debug(LogTag, $"Transition complete for '{sceneName}'.", this);
                EventBus<BFSceneTransitionCompleteEvent>.Fire(new BFSceneTransitionCompleteEvent { sceneName = sceneName });
            }
            finally
            {
                isTransitioning = false;
            }
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