using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BFTools.Systems.SceneManager
{
    [Serializable]
    public class BFSceneLoadRequest
    {
        [SerializeField] private string sceneName;
        [SerializeField] private LoadSceneMode loadMode = LoadSceneMode.Additive;
        [SerializeField] private bool showLoadingScreen;
        [SerializeField] private float minimumDisplayTime;

        internal string SceneName => sceneName;
        internal LoadSceneMode LoadMode => loadMode;
        internal bool ShowLoadingScreen => showLoadingScreen;
        internal float MinimumDisplayTime => minimumDisplayTime;
        internal bool HasSceneName => !string.IsNullOrEmpty(sceneName);
    }
}