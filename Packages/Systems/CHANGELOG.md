# Changelog

## [0.4.0] - Scene Transitions

### Added
- `BFWipeTransition`: directional colored wipe with configurable angle, duration, and a separate edge/border color and thickness, plus an optional leading/trailing edge sprite that tracks the wipe's moving edge
- `BFRadialWipeTransition`: radial wipe from 12 o'clock using `Image.fillMethod = Radial360`, with configurable color, direction, and duration
- `BFIrisWipeTransition`: iris/point wipe built on `SpriteMask` and `SpriteRenderer.maskInteraction`, with a swappable mask sprite (circle by default, any custom shape) and optional rotation while scaling
- Test suites for all three new transitions, following the existing `BFFadeTransitionTests` reflection-based field injection and end-state assertion pattern
- `UnityEngine.UI` reference added to `BFTools.Systems.SceneManager.PlayModeTests.asmdef` for the new `Image`-based transition tests

### Changed
- `BFSceneTransitionController` now holds a `BFTransitionBehaviour` reference instead of a hardcoded `BFFadeTransition`, so any transition component can be assigned to its `Transition` field; `BFFadeTransition` now derives from the new `BFTransitionBehaviour` abstract base instead of implementing `ITransition` directly
- The controller's serialized field was renamed from `fadeTransition` to `transition`, with `FormerlySerializedAs` preserving existing prefab references

## [0.3.0] - Settings Manager

### Added
- Settings Manager system: `BFSettingsManager`, `ISettingsProvider`, and asmdefs (Runtime, Tests)
- `BFSettingsManager.Register`/`Unregister` for provider registration, and `SaveAsync`/`LoadAsync` for persisting all registered providers' state to a single `settings.json`, reusing Save System's `BFSaveFileIO` and `BFSaveSerializer`
- Test suite covering provider registration, save/load round-trips, missing-file handling, and providers absent from a saved file

### Fixed
- `BFSettingsManager.SaveAsync`/`LoadAsync` no longer deadlock when awaited synchronously (e.g. via `GetAwaiter().GetResult()` in tests); internal awaits now use `ConfigureAwait(false)`, matching Save System's `BFSaveFileIO`/`BFSaveManager`

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