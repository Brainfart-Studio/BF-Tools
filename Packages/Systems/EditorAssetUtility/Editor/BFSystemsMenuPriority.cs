using BFTools.Core.EditorAssetUtility.Editor;

namespace BFTools.Systems.EditorAssetUtility.Editor
{
    public static class BFSystemsMenuPriority
    {
        public const int GlobalBootstrapper = BFMenuPriority.Group.Systems + 1;
        public const int LevelBootstrapper = BFMenuPriority.Group.Systems + 2;
        public const int ObjectPooler = BFMenuPriority.Group.Systems + 3;
        public const int SceneManager = BFMenuPriority.Group.Systems + 4;
    }
}