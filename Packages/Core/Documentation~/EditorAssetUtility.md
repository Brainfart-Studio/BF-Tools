# Editor Asset Utility

Shared editor-only helpers for creating folders, config assets, and prefab variants, used by the `Create` menu tooling across every package.

## Usage

Ensure a folder path exists, creating any missing segments:
```csharp
BFEditorAssetUtility.EnsureFolderExists("Assets/Configs/Feedback/Haptics");
```

Create a config asset (typically from a `MenuItem`):
```csharp
[MenuItem("Assets/Create/BFTools/Feedback/Config/Haptics Config", priority = BFFeedbackMenuPriority.Haptics)]
private static void Create()
{
    BFEditorAssetUtility.CreateConfigAsset<BFHapticsConfig>("Assets/Configs/Feedback/Haptics", "HapticsConfig.asset");
}
```

Create a prefab variant from a base prefab:
```csharp
BFEditorAssetUtility.CreatePrefabVariant(
    "Packages/com.bftools.feedback/Runtime/Prefabs/Haptics.prefab",
    "Assets/Prefabs/Feedback",
    "Haptics.prefab");
```

## How it works

- `EnsureFolderExists` splits the path on `/` and walks it one segment at a time, calling `AssetDatabase.CreateFolder` only for segments that don't already exist. A null/empty `assetPath`, or an empty segment (e.g. from a doubled `//`), is rejected with an `Error` log and the call returns without creating anything past that point.
- `CreateConfigAsset<T>` and `CreatePrefabVariant` both call `EnsureFolderExists` first, then check whether an asset already exists at the target path. If one does, it's returned as-is (no overwrite) with a `Warning` log, and gets selected/pinged in the Project window.
- If the target folder still isn't valid after `EnsureFolderExists` (e.g. because `folderPath` was malformed), both creation methods log an `Error` and return `null` rather than attempting to write into a bad path.
- `CreatePrefabVariant` loads the base prefab, instantiates it, saves the instance as a prefab variant at the target path via `PrefabUtility.SaveAsPrefabAsset`, then destroys the temporary scene instance.
- On success, all three methods select and ping the created (or pre-existing) asset in the Project window, so the menu action immediately shows the user the result.
- All logging goes through `BFLogger` under the `EditorAssetUtility` tag, at `Trace` (folder/asset lookups and skips), `Debug` (folder segment created), `Info` (asset created), `Warning` (asset already exists), or `Error` (bad input, invalid folder, missing base prefab).

## Notes
- `CreateConfigAsset` and `CreatePrefabVariant` don't validate `assetName` or `folderPath` for null/empty before building the target path. A null/empty `assetName` silently becomes a trailing-slash path, and the resulting failure is reported as "folder does not exist" even when the real problem is the asset name.
- `CreateConfigAsset`/`CreatePrefabVariant` repeat the same "already exists → warn/select/ping" and "folder invalid → error" blocks rather than sharing a helper; a third asset-creation method would triple that duplication.