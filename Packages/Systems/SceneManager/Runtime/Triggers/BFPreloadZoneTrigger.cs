using UnityEngine;
using UnityEngine.SceneManagement;
using BFTools.Core.Logger;

namespace BFTools.Systems.SceneManager
{
    public class BFPreloadZoneTrigger : MonoBehaviour
    {
        private const string LogTag = "SceneManager";

        [SerializeField] private BFSceneLoadRequest request;
        [SerializeField] private string playerTag = "Player";

        [Header("Inline Request (used when Request is unassigned)")]
        [SerializeField] private string inlineSceneName;
        [SerializeField] private LoadSceneMode inlineLoadMode = LoadSceneMode.Additive;
        [SerializeField] private bool inlineShowLoadingScreen;
        [SerializeField] private float inlineMinimumDisplayTime;

        private BFSceneLoadRequest runtimeRequest;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag(playerTag))
                return;

            BFSceneLoadRequest activeRequest = ResolveRequest();
            if (activeRequest == null)
            {
                BFLogger.Error(LogTag, "No BFSceneLoadRequest assigned and no inline scene name set. Ignoring preload zone trigger.", this);
                return;
            }

            BFLogger.Debug(LogTag, $"Preload zone entered by '{other.name}'. Preloading '{activeRequest.SceneName}'.", this);
            BFSceneLoader.Preload(activeRequest.SceneName, activeRequest.LoadMode);
        }

        private BFSceneLoadRequest ResolveRequest()
        {
            if (request != null)
                return request;

            if (string.IsNullOrEmpty(inlineSceneName))
                return null;

            if (runtimeRequest == null)
                runtimeRequest = BFSceneLoadRequest.Create(inlineSceneName, inlineLoadMode, inlineShowLoadingScreen, inlineMinimumDisplayTime);

            return runtimeRequest;
        }
    }
}