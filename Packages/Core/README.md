# BFTools Core (`com.bftools.core`)

Foundational systems for BFTools, covering logging, event communication, and shared editor tooling used by the rest of the library.

## Version
1.0.0

## Contents

### Event Bus
Generic static pub/sub system for struct-based events. See [Documentation~/EventBus.md](Documentation~/EventBus.md).

### Editor Asset Utility
Shared editor-only helpers (folder creation, config asset creation, prefab variant creation) used by this repo's `Assets/Create/BFTools/...` menu creators.

### Logger
Tag-based logging with per-tag level overrides and pluggable sinks. See [Documentation~/Logger.md](Documentation~/Logger.md).

### Project Setup
`IBFProjectSetupStep`, `IBFSystemPrefabContributor`, and `IBFSystemPrefabConsumer` interfaces for building self-contained, auto-discovered setup tooling. Any class implementing `IBFProjectSetupStep` is picked up automatically by `com.bftools.editortools`'s `BF Tools/New Project Setup` via `TypeCache`, no reference back to the calling package required.

### Service Locator
Static registry for locating shared services by type at runtime. See [Documentation~/ServiceLocator.md](Documentation~/ServiceLocator.md).

### Config Lookup
`BFConfigLookupBuilder`, merging a set of config entries into a key-based lookup with duplicate-key warnings.

### Serialization
`BFAllowlistJsonSerializer`, `BFTypeAllowlistBinder`, `IStateCapturable`, and `BFStateRegistry<T>`, shared allowlisted JSON (de)serialization and state registration/capture/restore logic used by Save System and Settings Manager.

### Camera Utility
`BFCameraResolver`, resolving a target or main camera with one-time missing-camera warnings.

### Layer Stack
`BFLayerStackBase<TLayerConfig, TLayer>`, `IBFLayerConfig<TLayer>`, and `IBFStackLayer`, shared base logic for config-driven visual/layer stacks.

### Singleton Guard
`BFActiveInstanceGuard<TOwner>`, enforcing a single active instance per owner type.

### File IO
`BFFileIO`, `BFAtomicFile`, and `BFPersistentDataPath`, shared async file read/write, atomic writes, and default persistent data path resolution.

## Dependencies
None.

## Installation

### Via Package Manager (git URL)
1. Open **Window > Package Manager**.
2. Click **+ > Add package from git URL...**
3. Enter this URL.
   ```
   https://github.com/Brainfart-Studio/BF-Tools.git?path=Packages/Core
   ```

### Via manifest.json
Add the entry directly to your project's `Packages/manifest.json`.
```json
"dependencies": {
  "com.bftools.core": "https://github.com/Brainfart-Studio/BF-Tools.git?path=Packages/Core"
}
```

### As a dependency of another package
Reference `com.bftools.core` from a dependent package's `package.json` (see [Packages/Systems/package.json](../Systems/package.json) for an example).
```json
"dependencies": {
  "com.bftools.core": "1.0.0"
}
```

### Note on embedding
Install via git URL (or as a registry/UPM dependency) rather than copying this folder directly into a project's `Packages/` directory. Git/UPM installs are mounted by the package's `name` (`com.bftools.core`); a directly embedded folder is mounted by its on-disk name (`Core`) instead. None of Core's current modules hardcode a `Packages/com.bftools.core/...` asset path, but downstream packages (Feedback, Visuals, Systems) that depend on Core assume it's mounted under its declared name, so embedding it directly can still break those.