# Changelog

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