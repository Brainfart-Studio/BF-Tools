using BFTools.Core.EditorAssetUtility.Editor;

namespace BFTools.Visuals.EditorAssetUtility.Editor
{
    public static class BFVisualsMenuPriority
    {
        public const int Palette = BFMenuPriority.Group.Visuals + 1;
        public const int Background = BFMenuPriority.Group.Visuals + 2;
        public const int Parallax = BFMenuPriority.Group.Visuals + 3;
    }
}