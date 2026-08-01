using UnityEditor;
using BFTools.Core.EditorAssetUtility.Editor;

namespace BFTools.Visuals.Background.Editor
{
    public static class BFBackgroundStackConfigCreator
    {
        private const string TargetPath = "Assets/Configs/Visuals/Background";
        private const string AssetName = "BackgroundStackConfig.asset";

        [MenuItem("Assets/Create/BFTools/Visuals/Background/Config/Background Stack Config", priority = BFMenuPriority.Group.Visuals + BFMenuPriority.Module.Background)]
        private static void Create()
        {
            BFEditorAssetUtility.CreateConfigAsset<BFBackgroundStackConfig>(TargetPath, AssetName);
        }
    }
}