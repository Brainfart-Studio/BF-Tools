# Changelog

## [0.2.0] - Test Coverage

### Added
- Test suites for Global Bootstrapper (Runtime, PlayMode), Level Bootstrapper, Object Pooler, Save System (Checksum, Encryptor, File IO, Key Provider, Manager, Serializer, Type Allowlist Binder, Version Migrator), and Scene Manager (Door Activation Trigger, Fade Transition, Preload Zone Trigger, Scene Loader, Scene Transition Controller)
- Corresponding Test/PlayMode asmdefs for all five systems
- BFSaveKeyProvider.FillRandomBytes, generating key and IV bytes from a single shared RandomNumberGenerator instance with a timeout fallback

### Fixed
- BFSaveKeyProvider and BFSaveEncryptor no longer risk hanging indefinitely on machines with low system entropy; key and IV generation now time out after 2 seconds and fall back to a non-blocking pseudo-random seed instead of blocking on RandomNumberGenerator.Create()/GenerateIV() per call
- BFSaveFileIO and BFSaveManager's async file writes/reads no longer deadlock when awaited synchronously (e.g. via GetAwaiter().GetResult() in tests); internal awaits now use ConfigureAwait(false)

### Changed
- Bumped `com.bftools.core` dependency to 0.10.0, required for the new `BFTools.Core.Logger.TestUtilities` assembly the test suites reference

## [0.1.0] - Systems Package Migration

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