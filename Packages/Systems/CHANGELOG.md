# Changelog

## [1.0.0] - Production Release

### Added
- `BFSceneTransitionInvoker`, for triggering scene transitions without a collision trigger, with support for inline scene load requests
- Inline scene load requests on door and preload triggers
- Save slot discovery and deletion on `BFSaveManager`
- Logging coverage across the scene transition routine
- Test coverage for the scene transition invoker, inline request resolution, and `BFSavePath`'s default and override branches

### Changed
- `BFSceneLoadRequestResolver` extracted and shared across `BFSceneTransitionInvoker`, door triggers, and preload triggers for inline scene load request resolution
- Scene load request configuration consolidated onto a single field
- `BFSceneLoader`'s tracked-scene guard and `AwaitCompletion` logic deduplicated
- Persistent data path default moved to Core's `BFTools.Core.FileIO`
- Save System's allowlist JSON serializer and state registration/capture/restore logic extracted to Core's `BFTools.Core.Serialization` (`BFAllowlistJsonSerializer`, `BFStateRegistry<T>`), now shared with Settings Manager
- Bumped `com.bftools.core` dependency to 1.0.0

### Fixed
- `BFFadeTransition` now initializes to a hidden state in `Awake` and correctly restores fader image alpha and canvas group alpha to zero by default
- `BFSceneTransitionController` guarded against duplicate instances and an unassigned transition, and now resets `isTransitioning` when a transition faults
- `BFSceneLoader` now rejects null or empty scene names
- Door and preload triggers guarded against unassigned scene load requests
- `BFSettingsManager.LoadAsync` now catches settings file read failures instead of throwing
- Save data corruption and tamper detection now logged at Error level
- `BFSaveManager.RegisterSlot` restricted to internal access
- Playtime now tracked accurately instead of always saving zero
- Save and load operations serialized per slot to prevent races
- Redundant migration log removed from `LoadAsync`
- `BFStateRegistry.RestoreAll` guarded against a null states dictionary
- `BFSettingsManager.SaveAsync` and `CaptureState` failures during `SaveAsync` now caught instead of throwing
- Save type allowlist dictionary made thread-safe
- Save System's shared static state guarded with locks
- Input validated on `BFSaveChecksum` and `BFSaveEncryptor`
- Migration no longer claims a migration happened when no migration steps exist
- Save slot names now reject unsafe characters
- Captured state now keyed by full type name to avoid namespace collisions

## [0.5.0] - Project Setup Step

### Added
- `BFGlobalBootstrapConfigCreator` now implements `IBFProjectSetupStep` and `IBFSystemPrefabConsumer` (from `com.bftools.core`'s new `BFTools.Core.ProjectSetup.Editor` assembly), so `BF Tools/New Project Setup` (in `com.bftools.editortools`) creates and wires the Global Bootstrap Config automatically, without EditorTools needing a hardcoded reference to Systems
- The Global Bootstrap Config setup step collects every `IBFSystemPrefabContributor` the setup orchestrator finds and assigns their prefabs to `System Prefabs`, replacing the wiring code that previously lived in EditorTools

### Changed
- Bumped `com.bftools.core` dependency to 0.11.0, required for the new `IBFProjectSetupStep`, `IBFSystemPrefabContributor`, and `IBFSystemPrefabConsumer` interfaces

## [0.4.1] - Menu Priority Ownership

### Changed
- GlobalBootstrapper, LevelBootstrapper, ObjectPooler, and SceneManager Editor menu creators now reference a new `BFSystemsMenuPriority` class instead of Core's `BFMenuPriority.Module` constants, so new Systems Create menu entries no longer require a Core version bump
- Added the `BFTools.Systems.EditorAssetUtility.Editor` asmdef, housing `BFSystemsMenuPriority`

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