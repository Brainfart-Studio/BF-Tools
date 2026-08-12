namespace BFTools.Systems.SceneManager
{
    internal static class BFSceneLoadRequestResolver
    {
        public static BFSceneLoadRequest Resolve(BFSceneLoadRequest request, BFSceneLoadRequestAsset sharedRequest)
        {
            BFSceneLoadRequest activeRequest = sharedRequest != null ? sharedRequest.Request : request;
            return activeRequest.HasSceneName ? activeRequest : null;
        }
    }
}