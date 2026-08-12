# Changelog

## [1.0.0] - Production Release

### Added
- `BFTools.Core.ConfigLookup` assembly, with `BFConfigLookupBuilder` for merging config entries into a key-based lookup with duplicate-key warnings
- `BFTools.Core.Serialization` assembly, with `BFAllowlistJsonSerializer` (Newtonsoft.Json wrapper restricted to allowlisted types), `BFTypeAllowlistBinder`, `IStateCapturable`, and `BFStateRegistry<T>` (shared registration, capture, and restore logic, extracted from Save System and Settings Manager)
- `BFTools.Core.CameraUtility` assembly, with `BFCameraResolver` for resolving a target or main camera with one-time missing-camera warnings
- `BFTools.Core.LayerStack` assembly, with `BFLayerStackBase<TLayerConfig, TLayer>`, `IBFLayerConfig<TLayer>`, and `IBFStackLayer`, extracted for use by config-driven visual/layer stacks
- `BFTools.Core.SingletonGuard` assembly, with `BFActiveInstanceGuard<TOwner>` for enforcing a single active instance per owner type
- `BFPersistentDataPath` added to `BFTools.Core.FileIO`, centralizing the default persistent data path

### Fixed
- `BFFileIO.ReadAsync` now loops until the full buffer is read, instead of assuming a single `ReadAsync` call fills it, and throws `EndOfStreamException` if the stream ends early
- `package.json` now declares `com.unity.nuget.newtonsoft-json` as a dependency, matching what `BFTools.Core.Serialization` has required since it was added; a fresh install without Newtonsoft already present would fail to compile

## [0.11.0] - Project Setup Steps

### Added
- `BFTools.Core.ProjectSetup.Editor` assembly, with three new interfaces for building auto-discovered, self-contained project setup tooling: `IBFProjectSetupStep` (`Order`, `DisplayName`, `Run()`), `IBFSystemPrefabContributor` (`GameObject SystemPrefab`), and `IBFSystemPrefabConsumer` (`AssignSystemPrefabs(GameObject[])`)
- `BFLoggerConfigCreator` now also implements `IBFProjectSetupStep`, so it participates in `BF Tools/New Project Setup` (in `com.bftools.editortools`) in addition to its existing `Assets/Create/BFTools/Core/Config/Logger Config` menu item

## [0.10.1] - Menu Priority Ownership

### Removed
- `Module.GlobalBootstrapper`, `Module.LevelBootstrapper`, `Module.ObjectPooler`, `Module.SceneManager`, `Module.Haptics`, `Module.ScreenShake`, `Module.Hitstop`, `Module.ScreenFlash`, `Module.Palette`, `Module.Background`, and `Module.Parallax` from `BFMenuPriority`, now owned by each consuming package's own `*MenuPriority` class; `Group` and `Module.Logger` are unchanged

## [0.10.0] - Test Coverage

### Added
- BFTools.Core.Logger.TestUtilities assembly, with SpyLoggerSink (an IBFLoggerSink capturing log entries for assertions) and BFLoggerTestUtility (resets BFLogger's static state between tests)
- Test suites for Editor Asset Utility, Event Bus, Logger (including FileSink and UnityConsoleSink), and Service Locator
- Corresponding Test asmdefs for all four systems

## [0.9.0] - Systems Package Split

### Removed
- Global Bootstrapper, Level Bootstrapper, Object Pooler, Scene Manager, and Save System, moved to the new `com.bftools.systems` package; see that package's CHANGELOG for their continued history
- `com.unity.nuget.newtonsoft-json` dependency, no longer needed now that Save System has moved out

### Added
- `Group.Systems` constant to `BFMenuPriority`, supporting the new Systems package's Create menu entries

## [0.8.1] - Menu Priority Update

### Changed
- Added `Module.Parallax` constant to `BFMenuPriority`, required for the Visuals package's Parallax config and prefab variant creators

## [0.8.0] - Scene Manager

### Added
- ITransition interface
- BFSceneLoadRequest config (scene name, load mode, show loading screen, minimum display time)
- BFSceneLoader static loader with dictionary-tracked concurrent preloads, Activate and progress reporting
- BFFadeTransition component (CanvasGroup-driven fade in/out)
- BFSceneTransitionController orchestration, wired to EventBus (BFSceneTransitionStartedEvent, BFSceneLoadedEvent, BFSceneTransitionCompleteEvent)
- BFDoorActivationTrigger with debounced activation-on-enter
- BFPreloadZoneTrigger with debounced preload-on-enter
- BFSceneLoadRequestCreator editor tool
- BFSceneTransitionControllerVariantCreator editor tool
- Base SceneTransitionController prefab, with a full-screen fade Canvas and CanvasGroup wired to BFFadeTransition
- `Module.SceneManager` constant added to `BFMenuPriority`

### Changed
- Renamed SceneLoadRequest to BFSceneLoadRequest for naming consistency
- Moved BFFadeTransition and trigger scripts into Transitions/Triggers subfolders
- Renamed the SceneTransitionController prefab generator to BFSceneTransitionControllerVariantCreator to match the VariantCreator naming convention used by other modules

### Fixed
- BFSceneLoader.UnloadAsync now checks whether the scene is actually loaded before calling UnloadSceneAsync, preventing a null reference exception (and a permanently stuck transition) when a Single-mode load has already unloaded it
- BFSceneLoadRequest's ShowLoadingScreen and MinimumDisplayTime fields are now read by BFSceneTransitionController instead of being ignored
- SceneTransitionController prefab shipped without a fade Canvas or CanvasGroup, causing a null reference exception on the first transition

## [0.7.4] - Menu Priority Update

### Changed
- Added `Module.Background` constant to `BFMenuPriority`, required for the Visuals package's Background config and prefab variant creators

## [0.7.3] - Menu Priority Update

### Changed
- Added `Group.Visuals` and `Module.Palette` constants to `BFMenuPriority`, required for the Visuals package's Palette Config creator

## [0.7.2] - Menu Priority Update

### Changed
- Added `Module.ScreenFlash` constant to `BFMenuPriority`, required for the Feedback package's Screen Flash Config and prefab variant creators

## [0.7.1] - Menu Priority Update

### Changed
- Added `Module.Hitstop` constant to `BFMenuPriority`, required for the Feedback package's Hitstop Config and prefab variant creators

## [0.7.0] - Save System

### Added
- SaveSystem asmdef added to Core package, with a Newtonsoft.Json dependency
- ISaveable interface (StateType, CaptureState, RestoreState contract)
- BFSaveMetadata struct (version, timestamp, playtime)
- BFSaveSerializer wrapper for JSON conversion via Newtonsoft.Json
- BFSaveEncryptor for AES encryption/decryption of save bytes
- BFSaveKeyProvider deriving a per-install encryption key, persisted alongside save data
- BFSaveChecksum generation and validation
- BFSaveFileIO with async read/write and a temp-file-then-rename write pattern
- BFSaveVersionMigrator (upgrade path scaffold)
- BFSaveManager with ISaveable registration and Save/Load wired to CaptureState/RestoreState
- Save slot support (multiple named files + slot metadata)
- Default save path resolution (Application.persistentDataPath)
- BFLogger tracing added to SaveSystem
- BFSaveTypeAllowlistBinder restricting save deserialization to registered ISaveable state types

### Changed
- Renamed SaveMetadata struct to BFSaveMetadata for naming consistency

### Fixed
- Fully qualified Newtonsoft Formatting to resolve a CS0104 ambiguity
- Save encryption key now derived per install, with the IV randomized per call
- Loads now fail gracefully when save data cannot be decrypted or parsed, instead of throwing
- Guarded against null save data after decrypt to prevent an unhandled crash
- Restricted BFSaveSerializer TypeNameHandling to allowlisted types
- Save checksums now use a keyed MAC (HMAC-SHA256) instead of unkeyed SHA256

## [0.6.1] - Create Menu Reorganization

### Added
- `BFMenuPriority` static class centralizing Create menu priority values (`Group` + `Module`)

### Changed
- Reorganized `Assets/Create/BFTools` menu items into per-package submenus (`Core`, `Feedback`)
- Logger, GlobalBootstrapper, LevelBootstrapper, and ObjectPooler creators now use explicit low priority values via `BFMenuPriority`, sorting BFTools above Unity's built-in Create menu categories

## [0.6.0] - Object Pooler

### Added
- BFObjectPoolConfig ScriptableObject with key/prefab/prewarmCount pool entries
- BFObjectPooler MonoBehaviour with prewarm-on-Awake and Get/Release
- BFObjectPooler registration with BFServiceLocator on Awake/OnDestroy
- BFLogger tracing on BFObjectPooler (prewarm, get, release) and warning on pool exhaustion
- BFObjectPoolConfigCreator editor tool
- BFObjectPoolerVariantCreator editor tool
- Base ObjectPooler prefab

### Fixed
- Corrected ObjectPooler prefab folder name (`Prefab` -> `Prefabs`) to match BFObjectPoolerVariantCreator's base prefab path

## [0.5.0] - Service Locator

### Added
- BFServiceLocator static class with Register/Get/Unregister for type-keyed service registration
- BFLogger tracing on Register/Get/Unregister calls

## [0.4.1] - Logger Rollout

### Added
- BFLogger tracing added to EventBus (subscribe/unsubscribe/fire/clear), GlobalBootstrapper, LevelBootstrapper, and EditorAssetUtility

### Fixed
- BFLogger now auto-loads `BFLoggerConfig` from `Resources/BFTools/BFLoggerConfig` on the first log call if `Initialize` wasn't called explicitly, falling back to a default config (`Info` level, console sink) with a one-time warning if none is found

## [0.4.0] - Logger

### Added
- LogLevel enum (Trace/Debug/Info/Warning/Error/Critical)
- BFLoggerConfig ScriptableObject with global minimum level, per-tag level overrides, tag allowlist, and stack trace threshold
- IBFLoggerSink interface
- UnityConsoleSink implementation
- FileSink implementation, writing Warning and above to a rotating log file
- BFLogger static wrapper with level and tag filtering
- BFLoggerConfigCreator editor tool

### Fixed
- UnityConsoleSink no longer throws a FormatException when a log message contains `{}`-style characters
- Tag level overrides can now raise the effective minimum level above the global default, not just lower it

## [0.3.0] - Editor Asset Utility

### Added
- BFEditorAssetUtility editor utility: shared recursive folder creation, config asset creation, and prefab variant creation

### Changed
- Migrated BFGlobalBootstrapConfigCreator to BFEditorAssetUtility
- Migrated BFLevelBootstrapConfigCreator and BFLevelBootstrapperVariantCreator to BFEditorAssetUtility

### Fixed
- Folder creation no longer silently continues when an intermediate folder fails to create

## [0.2.0] - Event Bus

### Added
- EventBus core system

### Changed
- Bumped package version for Event Bus

## [0.1.0] - Bootstrapper

### Added
- Core package.json
- GlobalBootstrapper and BootstrapConfig
- BootstrapConfigCreator editor tool
- Bootstrapper runtime/editor asmdefs
- LevelBootstrapper matching Global config, editor creators, asmdefs

### Changed
- Renamed BootstrapConfig to BFGlobalBootstrapConfig for naming consistency
- Renamed BFGlobalBootstrapConfig to BFGlobalBootstrapperConfig for naming consistency
- Split Bootstrapper namespaces and asmdefs into Global/Level

### Fixed
- Restricted BootstrapConfig.systemPrefabs to internal access
- Corrected LevelBootstrapperVariantCreator base prefab path to include LevelBootstrapper subfolder