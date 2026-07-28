namespace BFTools.Core.EditorAssetUtility.Editor
{
    public static class BFMenuPriority
    {
        public static class Group
        {
            public const int Core = -100;
            public const int Feedback = -80;
        }

        public static class Module
        {
            // Core
            public const int GlobalBootstrapper = 1;
            public const int LevelBootstrapper = 2;
            public const int ObjectPooler = 3;
            public const int Logger = 4;

            // Feedback
            public const int Haptics = 1;
            public const int ScreenShake = 2;
        }
    }
}