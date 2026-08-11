# BFTools Editor Tools (`com.bftools.editortools`)

Editor-only tooling for BFTools: one-click project bootstrap and feedback-system verification.

## Version
1.0.0

## Contents

### Project Setup
`BF Tools > New Project Setup` discovers every `IBFProjectSetupStep` (from `com.bftools.core`'s `BFTools.Core.ProjectSetup.Editor` assembly) present anywhere in the project via `TypeCache`, runs each one, and wires any `IBFSystemPrefabContributor` results to any `IBFSystemPrefabConsumer` steps. EditorTools itself holds no hardcoded knowledge of what gets created or where — currently, with Core, Systems, and Feedback installed, this creates the Logger and Global Bootstrapper configs, creates a prefab variant + config for each Feedback module (Hitstop, Screen Shake, Screen Flash, Haptics), seeds each config with a `"Default"` entry, assigns each config to its prefab, and wires all four prefabs into the Global Bootstrapper config. Idempotent — re-running it will not duplicate or overwrite assets that already exist. Any package can add its own setup step and it'll be picked up the same way, with no changes needed here.

### Project Verification
`BF Tools > New Project Verification` creates (or opens, if it already exists) a `BFToolsTest` scene containing a camera, 3 randomly colored/sized bouncing balls, and 4 UI buttons (Hitstop, Screen Shake, Screen Flash, Haptics) wired to fire each feedback event with `eventName = "Default"`. Enter Play Mode with this scene open to visually confirm all four Feedback systems are working end-to-end.

## Dependencies
- `com.bftools.core` @ 1.0.0
- `com.bftools.systems` @ 1.0.0
- `com.bftools.feedback` @ 1.0.0
- `com.unity.textmeshpro` @ 3.0.6
- `com.unity.inputsystem` @ 1.7.0

## Installation

### Via Package Manager (git URL)
1. Open **Window > Package Manager**.
2. Click **+ > Add package from git URL...**
3. Enter:
   ```
   https://github.com/Brainfart-Studio/BF-Tools.git?path=Packages/EditorTools
   ```

Editor Tools depends on `com.bftools.core`, `com.bftools.systems`, and `com.bftools.feedback`; install each the same way (see [Core's README](../Core/README.md#installation), [Systems' README](../Systems/README.md#installation), and [Feedback's README](../Feedback/README.md#installation)) if they aren't already in the project.

### Via manifest.json
Add the entry directly to your project's `Packages/manifest.json`:

```json
"dependencies": {
  "com.bftools.editortools": "https://github.com/Brainfart-Studio/BF-Tools.git?path=Packages/EditorTools"
}
```

### As a dependency of another package
Reference `com.bftools.editortools` from a dependent package's `package.json`:

```json
"dependencies": {
  "com.bftools.editortools": "1.0.0"
}
```

### Note on embedding
Install via git URL (or as a registry/UPM dependency) rather than copying this folder directly into a project's `Packages/` directory, same as any other BFTools package. EditorTools itself has no hardcoded asset paths of its own, but the setup steps it discovers and runs (e.g. Feedback's Hitstop setup step, which expects its base prefab at `Packages/com.bftools.feedback/Hitstop/Prefabs/Hitstop.prefab`) do — so a directly embedded Feedback (or Core, or Systems) folder mounted under its on-disk name instead of its package name will break those steps when `BF Tools > New Project Setup` runs.