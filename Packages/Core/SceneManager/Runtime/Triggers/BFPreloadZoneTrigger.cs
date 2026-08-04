using UnityEngine;
using BFTools.Core.Logger;

namespace BFTools.Core.SceneManager
{
    public class BFPreloadZoneTrigger : MonoBehaviour
    {
        private const string LogTag = "SceneManager";

        [SerializeField] private BFSceneLoadRequest request;
        [SerializeField] private string playerTag = "Player";

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag(playerTag))
                return;

            BFLogger.Debug(LogTag, $"Preload zone entered by '{other.name}'. Preloading '{request.SceneName}'.", this);
            BFSceneLoader.Preload(request.SceneName, request.LoadMode);
        }
    }
}