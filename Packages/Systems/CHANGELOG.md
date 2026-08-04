# Changelog

## [0.1.0] - Systems Package Migration

### Added
- Global Bootstrapper, Level Bootstrapper, Object Pooler, Scene Manager, and Save System migrated from `com.bftools.core`; see Core's CHANGELOG (`0.1.0` through `0.8.1`) for their original implementation history
- `BFTools.Systems.*` namespaces and asmdefs for all five systems, replacing their prior `BFTools.Core.*` identities
- Dependency on `com.bftools.core` for Logger, Event Bus, Service Locator, and Editor Asset Utility
- Dependency on `com.unity.nuget.newtonsoft-json`, carried over from Save System

### Changed
- All five systems' `[MenuItem]` priorities switched from `BFMenuPriority.Group.Core` to the new `BFMenuPriority.Group.Systems`
- Prefab and config output paths updated from `Assets/Prefabs/Core`/`Assets/Configs/Core/...` to `Assets/Prefabs/Systems`/`Assets/Configs/Systems/...` for Level Bootstrapper, Object Pooler, and Scene Manager
- `BasePrefabPath` constants in Level Bootstrapper, Object Pooler, and Scene Manager's prefab variant creators updated from `Packages/com.bftools.core/...` to `Packages/com.bftools.systems/...`