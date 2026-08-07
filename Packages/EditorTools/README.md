# BFTools Editor Tools (`com.bftools.editortools`)

Editor-only tooling for BFTools: one-click project bootstrap and feedback-system verification.

## Version
0.1.0

## Contents

### Project Setup
`BF Tools > New Project Setup` creates the Logger and Global Bootstrapper configs, creates a prefab variant + config for each Feedback module (Hitstop, Screen Shake, Screen Flash, Haptics), seeds each config with a `"Default"` entry, assigns each config to its prefab, and wires all four prefabs into the Global Bootstrapper config. Idempotent — re-running it will not duplicate or overwrite assets that already exist.

### Project Verification
`BF Tools > New Project Verification` creates (or opens, if it already exists) a `BFToolsTest` scene containing a camera, 3 randomly colored/sized bouncing balls, and 4 UI buttons (Hitstop, Screen Shake, Screen Flash, Haptics) wired to fire each feedback event with `eventName = "Default"`. Enter Play Mode with this scene open to visually confirm all four Feedback systems are working end-to-end.

## Dependencies
- `com.bftools.core` @ 0.10.1
- `com.bftools.systems` @ 0.4.1
- `com.bftools.feedback` @ 0.6.2
- `com.unity.textmeshpro` @ 3.0.6
- `com.unity.inputsystem` @ 1.7.0

## Installation

> **Note:** This package is still under development and hasn't been merged to `main` yet, so the URLs below point at the `development` branch. Once it goes live on `main`, drop the `#development` segment (or switch it to a release tag) and update this README accordingly.

### Via Package Manager (git URL)
1. Open **Window > Package Manager**.
2. Click **+ > Add package from git URL...**
3. Enter:
   ```
   https://github.com/Brainfart-Studio/BF-Tools.git?path=Packages/EditorTools#development
   ```

Editor Tools depends on `com.bftools.core`, `com.bftools.systems`, and `com.bftools.feedback`; install each the same way (see [Core's README](../Core/README.md#installation), [Systems' README](../Systems/README.md#installation), and [Feedback's README](../Feedback/README.md#installation)) if they aren't already in the project.

### Via manifest.json
Add the entry directly to your project's `Packages/manifest.json`:

```json
"dependencies": {
  "com.bftools.editortools": "https://github.com/Brainfart-Studio/BF-Tools.git?path=Packages/EditorTools#development"
}
```

### As a dependency of another package
Reference `com.bftools.editortools` from a dependent package's `package.json`:

```json
"dependencies": {
  "com.bftools.editortools": "0.1.0"
}
```

### Note on embedding
Install via git URL (or as a registry/UPM dependency) rather than copying this folder directly into a project's `Packages/` directory. Git/UPM installs are mounted by the package's `name` (`com.bftools.editortools`), which is what the Project Setup tool's hardcoded prefab paths (e.g. `Packages/com.bftools.feedback/Hitstop/Prefabs/Hitstop.prefab`) expect. A directly embedded folder is mounted by its on-disk name (`EditorTools`) instead, which will break those paths.
