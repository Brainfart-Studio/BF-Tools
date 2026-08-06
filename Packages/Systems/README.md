# BFTools Systems (`com.bftools.systems`)

Gameplay-facing systems for BFTools, built on Core's Logger, Event Bus, and Service Locator: bootstrapping, scene management, object pooling, and save/load.

## Version
0.3.0

## Contents

### Bootstrapper
Global (app-lifetime) and Level (per-scene) system initialization. See [Documentation~/Bootstrapper.md](Documentation~/Bootstrapper.md).

### Object Pooler
Config-driven object pooling with prewarming and Get/Release, registered with the Service Locator. See [Documentation~/ObjectPooler.md](Documentation~/ObjectPooler.md).

### Save System
ISaveable-based state capture/restore, saved to encrypted, checksummed, slot-based files with a per-install key and a version migration scaffold. See [Documentation~/SaveSystem.md](Documentation~/SaveSystem.md).

### Scene Manager
Additive scene loading and transitions, orchestrated through a config asset, a static loader, a fade, and door/preload trigger components. See [Documentation~/SceneManager.md](Documentation~/SceneManager.md).

### Settings Manager
ISettingsProvider-based state capture/restore, saved as plain JSON to a single shared settings file. See [Documentation~/SettingsManager.md](Documentation~/SettingsManager.md).

## Dependencies
- `com.bftools.core` @ 0.10.0
- `com.unity.nuget.newtonsoft-json` @ 3.2.1 (Save System)

## Installation

> **Note.** This package is still under development and hasn't been merged to `main` yet, so the URLs below point at the `development` branch. Once it goes live on `main`, drop the `#development` segment (or switch it to a release tag) and update this README accordingly.

### Via Package Manager (git URL)
1. Open **Window > Package Manager**.
2. Click **+ > Add package from git URL...**
3. Enter this URL.
   ```
   https://github.com/Brainfart-Studio/BF-Tools.git?path=Packages/Systems#development
   ```

Systems depends on `com.bftools.core`; install it the same way (see [Core's README](../Core/README.md#installation)) if it isn't already in the project.

### Via manifest.json
Add the entry directly to your project's `Packages/manifest.json`.

```json
"dependencies": {
  "com.bftools.systems": "https://github.com/Brainfart-Studio/BF-Tools.git?path=Packages/Systems#development"
}
```

### As a dependency of another package
Reference `com.bftools.systems` from a dependent package's `package.json`.

```json
"dependencies": {
  "com.bftools.systems": "0.3.0"
}
```

### Note on embedding
Install via git URL (or as a registry/UPM dependency) rather than copying this folder directly into a project's `Packages/` directory. Git/UPM installs are mounted by the package's `name` (`com.bftools.systems`), which is what the editor tooling's hardcoded asset paths expect. A directly embedded folder is mounted by its on-disk name (`Systems`) instead, which will break those paths for the Level Bootstrapper, Object Pooler, and Scene Transition Controller prefab variant creators.