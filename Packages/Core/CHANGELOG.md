# Changelog

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