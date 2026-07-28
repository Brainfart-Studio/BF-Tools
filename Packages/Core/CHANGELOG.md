# Changelog

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