using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using BFTools.Core.EditorAssetUtility.Editor;

namespace BFTools.Core.EditorAssetUtility.Tests
{
    public class BFEditorAssetUtilityTests
    {
        private const string Root = "Assets/_BFEditorAssetUtilityTests";

        private class FakeConfig : ScriptableObject
        {
        }

        [SetUp]
        public void SetUp()
        {
            CleanupRoot();
        }

        [TearDown]
        public void TearDown()
        {
            CleanupRoot();
        }

        private static void CleanupRoot()
        {
            if (AssetDatabase.IsValidFolder(Root))
            {
                AssetDatabase.DeleteAsset(Root);
                AssetDatabase.Refresh();
            }
        }

        private static void CreateBasePrefab(string path)
        {
            GameObject go = new GameObject("Base");
            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void EnsureFolderExists_CreatesNestedFolders()
        {
            string path = $"{Root}/A/B/C";

            BFEditorAssetUtility.EnsureFolderExists(path);

            Assert.IsTrue(AssetDatabase.IsValidFolder(path));
        }

        [Test]
        public void EnsureFolderExists_AlreadyExists_DoesNotThrow()
        {
            string path = $"{Root}/A";
            BFEditorAssetUtility.EnsureFolderExists(path);

            Assert.DoesNotThrow(() => BFEditorAssetUtility.EnsureFolderExists(path));
            Assert.IsTrue(AssetDatabase.IsValidFolder(path));
        }

        [Test]
        public void EnsureFolderExists_EmptyPathSegment_LogsErrorAndStopsBeforeIt()
        {
            string path = $"{Root}//Bad";
            LogAssert.Expect(LogType.Error, new Regex("empty path segment"));

            BFEditorAssetUtility.EnsureFolderExists(path);

            Assert.IsFalse(AssetDatabase.IsValidFolder(path));
        }

        [Test]
        public void CreateConfigAsset_CreatesNewAssetAtPath()
        {
            FakeConfig asset = BFEditorAssetUtility.CreateConfigAsset<FakeConfig>(Root, "FakeConfig.asset");

            Assert.IsNotNull(asset);
            Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<FakeConfig>($"{Root}/FakeConfig.asset"));
        }

        [Test]
        public void CreateConfigAsset_WhenAlreadyExists_ReturnsExistingInstance()
        {
            FakeConfig first = BFEditorAssetUtility.CreateConfigAsset<FakeConfig>(Root, "FakeConfig.asset");
            LogAssert.Expect(LogType.Warning, new Regex("already exists"));

            FakeConfig second = BFEditorAssetUtility.CreateConfigAsset<FakeConfig>(Root, "FakeConfig.asset");

            Assert.AreSame(first, second);
        }

        [Test]
        public void CreateConfigAsset_MalformedFolderPath_ReturnsNull()
        {
            string badFolder = $"{Root}//BadFolder";
            LogAssert.Expect(LogType.Error, new Regex("empty path segment"));
            LogAssert.Expect(LogType.Error, new Regex("does not exist"));

            FakeConfig result = BFEditorAssetUtility.CreateConfigAsset<FakeConfig>(badFolder, "FakeConfig.asset");

            Assert.IsNull(result);
        }

        [Test]
        public void CreatePrefabVariant_CreatesVariantFromBasePrefab()
        {
            string basePrefabPath = $"{Root}/Base.prefab";
            CreateBasePrefab(basePrefabPath);

            GameObject variant = BFEditorAssetUtility.CreatePrefabVariant(basePrefabPath, Root, "Variant.prefab");

            Assert.IsNotNull(variant);
            Assert.AreEqual(PrefabAssetType.Variant, PrefabUtility.GetPrefabAssetType(variant));
        }

        [Test]
        public void CreatePrefabVariant_WhenAlreadyExists_ReturnsExistingInstance()
        {
            string basePrefabPath = $"{Root}/Base.prefab";
            CreateBasePrefab(basePrefabPath);
            GameObject first = BFEditorAssetUtility.CreatePrefabVariant(basePrefabPath, Root, "Variant.prefab");
            LogAssert.Expect(LogType.Warning, new Regex("already exists"));

            GameObject second = BFEditorAssetUtility.CreatePrefabVariant(basePrefabPath, Root, "Variant.prefab");

            Assert.AreSame(first, second);
        }

        [Test]
        public void CreatePrefabVariant_MissingBasePrefab_ReturnsNull()
        {
            LogAssert.Expect(LogType.Error, new Regex("base prefab not found"));

            GameObject result = BFEditorAssetUtility.CreatePrefabVariant($"{Root}/DoesNotExist.prefab", Root, "Variant.prefab");

            Assert.IsNull(result);
        }
    }
}