# BFTools Core (`com.bftools.core`)

Foundational systems for BFTools: bootstrapping, event communication, and logging.

## Version
0.7.0

## Contents

### Bootstrapper
Global (app-lifetime) and Level (per-scene) system initialization. See [Documentation~/Bootstrapper.md](Documentation~/Bootstrapper.md).

### Event Bus
Generic static pub/sub system for struct-based events. See [Documentation~/EventBus.md](Documentation~/EventBus.md).

### Editor Asset Utility
Shared editor-only helpers (folder creation, config asset creation, prefab variant creation) used by this repo's `Assets/Create/BFTools/...` menu creators.

### Logger
Tag-based logging with per-tag level overrides and pluggable sinks. See [Documentation~/Logger.md](Documentation~/Logger.md).

### Service Locator
Static registry for locating shared services by type at runtime. See [Documentation~/ServiceLocator.md](Documentation~/ServiceLocator.md).

### Object Pooler
Config-driven object pooling with prewarming and Get/Release, registered with the Service Locator. See [Documentation~/ObjectPooler.md](Documentation~/ObjectPooler.md).

### Save System
ISaveable-based state capture/restore, saved to encrypted, checksummed, slot-based files with a per-install key and a version migration scaffold. See [Documentation~/SaveSystem.md](Documentation~/SaveSystem.md).

## Dependencies
None.

## Installation

> **Note:** This package is still under development and hasn't been merged to `main` yet, so the URLs below point at the `development` branch. Once it goes live on `main`, drop the `#development` segment (or switch it to a release tag) and update this README accordingly.

### Via Package Manager (git URL)
1. Open **Window > Package Manager**.
2. Click **+ > Add package from git URL...**
3. Enter:
   ```
   https://github.com/Brainfart-Studio/BF-Tools.git?path=Packages/Core#development
   ```

### Via manifest.json
Add the entry directly to your project's `Packages/manifest.json`:
```json
"dependencies": {
  "com.bftools.core": "https://github.com/Brainfart-Studio/BF-Tools.git?path=Packages/Core#development"
}
```

### As a dependency of another package
Reference `com.bftools.core` from a dependent package's `package.json` (see [Packages/Feedback/package.json](../Feedback/package.json) for an example):
```json
"dependencies": {
  "com.bftools.core": "0.7.0"
}
```

### Note on embedding
Install via git URL (or as a registry/UPM dependency) rather than copying this folder directly into a project's `Packages/` directory. Git/UPM installs are mounted by the package's `name` (`com.bftools.core`), which is what the editor tooling's hardcoded asset paths (e.g. the Level Bootstrapper prefab variant creator) expect. A directly embedded folder is mounted by its on-disk name (`Core`) instead, which will break those paths.