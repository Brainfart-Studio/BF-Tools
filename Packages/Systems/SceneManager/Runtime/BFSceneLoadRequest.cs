using UnityEngine;
using UnityEngine.SceneManagement;

namespace BFTools.Systems.SceneManager
{
    public class BFSceneLoadRequest : ScriptableObject
    {
        [SerializeField] private string sceneName;
        [SerializeField] private LoadSceneMode loadMode = LoadSceneMode.Additive;
        [SerializeField] private bool showLoadingScreen;
        [SerializeField] private float minimumDisplayTime;

        internal string SceneName => sceneName;
        internal LoadSceneMode LoadMode => loadMode;
        internal bool ShowLoadingScreen => showLoadingScreen;
        internal float MinimumDisplayTime => minimumDisplayTime;

        internal static BFSceneLoadRequest Create(string sceneName, LoadSceneMode loadMode = LoadSceneMode.Additive, bool showLoadingScreen = false, float minimumDisplayTime = 0f)
        {
            BFSceneLoadRequest request = CreateInstance<BFSceneLoadRequest>();
            request.sceneName = sceneName;
            request.loadMode = loadMode;
            request.showLoadingScreen = showLoadingScreen;
            request.minimumDisplayTime = minimumDisplayTime;
            return request;
        }
    }
}