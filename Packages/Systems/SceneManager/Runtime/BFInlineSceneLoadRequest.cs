using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BFTools.Systems.SceneManager
{
    [Serializable]
    internal class BFInlineSceneLoadRequest
    {
        [SerializeField] private string sceneName;
        [SerializeField] private LoadSceneMode loadMode = LoadSceneMode.Additive;
        [SerializeField] private bool showLoadingScreen;
        [SerializeField] private float minimumDisplayTime;

        private BFSceneLoadRequest runtimeRequest;

        public BFSceneLoadRequest Resolve(BFSceneLoadRequest explicitRequest)
        {
            if (explicitRequest != null)
                return explicitRequest;

            if (string.IsNullOrEmpty(sceneName))
                return null;

            if (runtimeRequest == null)
                runtimeRequest = BFSceneLoadRequest.Create(sceneName, loadMode, showLoadingScreen, minimumDisplayTime);

            return runtimeRequest;
        }
    }
}