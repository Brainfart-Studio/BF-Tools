using System.Collections.Generic;
using System.Reflection;
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

        private class FakeConsumer : MonoBehaviour
        {
            [SerializeField] private List<Object> configs = new List<Object>();
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
            BFEditorAssetUtility.EnsureFolderExists(Root);

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
        public void CreateConfigAsset_NullFolderPath_ReturnsNull()
        {
            LogAssert.Expect(LogType.Error, new Regex("folderPath is null or empty"));

            FakeConfig result = BFEditorAssetUtility.CreateConfigAsset<FakeConfig>(null, "FakeConfig.asset");

            Assert.IsNull(result);
        }

        [Test]
        public void CreateConfigAsset_EmptyAssetName_ReturnsNull()
        {
            LogAssert.Expect(LogType.Error, new Regex("assetName is null or empty"));

            FakeConfig result = BFEditorAssetUtility.CreateConfigAsset<FakeConfig>(Root, string.Empty);

            Assert.IsNull(result);
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
        public void CreatePrefabVariant_NullFolderPath_ReturnsNull()
        {
            LogAssert.Expect(LogType.Error, new Regex("folderPath is null or empty"));

            GameObject result = BFEditorAssetUtility.CreatePrefabVariant($"{Root}/Base.prefab", null, "Variant.prefab");

            Assert.IsNull(result);
        }

        [Test]
        public void CreatePrefabVariant_EmptyAssetName_ReturnsNull()
        {
            LogAssert.Expect(LogType.Error, new Regex("assetName is null or empty"));

            GameObject result = BFEditorAssetUtility.CreatePrefabVariant($"{Root}/Base.prefab", Root, string.Empty);

            Assert.IsNull(result);
        }

        [Test]
        public void CreatePrefabVariant_MissingBasePrefab_ReturnsNull()
        {
            LogAssert.Expect(LogType.Error, new Regex("base prefab not found"));

            GameObject result = BFEditorAssetUtility.CreatePrefabVariant($"{Root}/DoesNotExist.prefab", Root, "Variant.prefab");

            Assert.IsNull(result);
        }

        private static List<Object> GetConfigs(FakeConsumer consumer)
        {
            return (List<Object>)typeof(FakeConsumer)
                .GetField("configs", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(consumer);
        }

        [Test]
        public void AssignConfigIfMissing_NotAlreadyPresent_AddsToList()
        {
            GameObject go = new GameObject("Consumer");
            FakeConsumer consumer = go.AddComponent<FakeConsumer>();
            FakeConfig config = ScriptableObject.CreateInstance<FakeConfig>();

            BFEditorAssetUtility.AssignConfigIfMissing(consumer, "configs", config);

            Assert.AreEqual(1, GetConfigs(consumer).Count);
            Assert.AreSame(config, GetConfigs(consumer)[0]);

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(config);
        }

        [Test]
        public void AssignConfigIfMissing_AlreadyPresent_DoesNotAddDuplicate()
        {
            GameObject go = new GameObject("Consumer");
            FakeConsumer consumer = go.AddComponent<FakeConsumer>();
            FakeConfig config = ScriptableObject.CreateInstance<FakeConfig>();

            BFEditorAssetUtility.AssignConfigIfMissing(consumer, "configs", config);
            BFEditorAssetUtility.AssignConfigIfMissing(consumer, "configs", config);

            Assert.AreEqual(1, GetConfigs(consumer).Count);

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(config);
        }

        [Test]
        public void AssignConfigIfMissing_NullTarget_LogsError()
        {
            FakeConfig config = ScriptableObject.CreateInstance<FakeConfig>();
            LogAssert.Expect(LogType.Error, new Regex("target is null"));

            Assert.DoesNotThrow(() => BFEditorAssetUtility.AssignConfigIfMissing(null, "configs", config));

            Object.DestroyImmediate(config);
        }

        [Test]
        public void AssignConfigIfMissing_NullConfig_LogsError()
        {
            GameObject go = new GameObject("Consumer");
            FakeConsumer consumer = go.AddComponent<FakeConsumer>();
            LogAssert.Expect(LogType.Error, new Regex("config is null"));

            Assert.DoesNotThrow(() => BFEditorAssetUtility.AssignConfigIfMissing(consumer, "configs", null));

            Object.DestroyImmediate(go);
        }

        [Test]
        public void AssignConfigIfMissing_UnknownFieldName_LogsError()
        {
            GameObject go = new GameObject("Consumer");
            FakeConsumer consumer = go.AddComponent<FakeConsumer>();
            FakeConfig config = ScriptableObject.CreateInstance<FakeConfig>();
            LogAssert.Expect(LogType.Error, new Regex("has no 'missingField' field"));

            Assert.DoesNotThrow(() => BFEditorAssetUtility.AssignConfigIfMissing(consumer, "missingField", config));

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(config);
        }
    }
}