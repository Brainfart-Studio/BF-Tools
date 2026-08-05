# BFTools Core (`com.bftools.core`)

Foundational systems for BFTools, covering logging, event communication, and shared editor tooling used by the rest of the library.

## Version
0.10.0

## Contents

### Event Bus
Generic static pub/sub system for struct-based events. See [Documentation~/EventBus.md](Documentation~/EventBus.md).

### Editor Asset Utility
Shared editor-only helpers (folder creation, config asset creation, prefab variant creation) used by this repo's `Assets/Create/BFTools/...` menu creators.

### Logger
Tag-based logging with per-tag level overrides and pluggable sinks. See [Documentation~/Logger.md](Documentation~/Logger.md).

### Service Locator
Static registry for locating shared services by type at runtime. See [Documentation~/ServiceLocator.md](Documentation~/ServiceLocator.md).

## Dependencies
None.

## Installation

> **Note.** This package is still under development and hasn't been merged to `main` yet, so the URLs below point at the `development` branch. Once it goes live on `main`, drop the `#development` segment (or switch it to a release tag) and update this README accordingly.

### Via Package Manager (git URL)
1. Open **Window > Package Manager**.
2. Click **+ > Add package from git URL...**
3. Enter this URL.
   ```
   https://github.com/Brainfart-Studio/BF-Tools.git?path=Packages/Core#development
   ```

### Via manifest.json
Add the entry directly to your project's `Packages/manifest.json`.
```json
"dependencies": {
  "com.bftools.core": "https://github.com/Brainfart-Studio/BF-Tools.git?path=Packages/Core#development"
}
```

### As a dependency of another package
Reference `com.bftools.core` from a dependent package's `package.json` (see [Packages/Systems/package.json](../Systems/package.json) for an example).
```json
"dependencies": {
  "com.bftools.core": "0.10.0"
}
```

### Note on embedding
Install via git URL (or as a registry/UPM dependency) rather than copying this folder directly into a project's `Packages/` directory. Git/UPM installs are mounted by the package's `name` (`com.bftools.core`); a directly embedded folder is mounted by its on-disk name (`Core`) instead. None of Core's current modules hardcode a `Packages/com.bftools.core/...` asset path, but downstream packages (Feedback, Visuals, Systems) that depend on Core assume it's mounted under its declared name, so embedding it directly can still break those.