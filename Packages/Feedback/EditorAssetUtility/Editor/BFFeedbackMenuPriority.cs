using BFTools.Core.EditorAssetUtility.Editor;

namespace BFTools.Feedback.EditorAssetUtility.Editor
{
    public static class BFFeedbackMenuPriority
    {
        public const int Haptics = BFMenuPriority.Group.Feedback + 1;
        public const int ScreenShake = BFMenuPriority.Group.Feedback + 2;
        public const int Hitstop = BFMenuPriority.Group.Feedback + 3;
        public const int ScreenFlash = BFMenuPriority.Group.Feedback + 4;
    }
}