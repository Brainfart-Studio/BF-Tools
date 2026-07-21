# BFTools Feedback (`com.bftools.feedback`)

Event-driven feedback systems: controller haptics and camera screen shake.

## Version
0.3.0

## Contents

### Haptics
Controller rumble triggered by named events. See [Documentation~/Haptics.md](Documentation~/Haptics.md).

### Screen Shake
Camera shake triggered by named events. See [Documentation~/ScreenShake.md](Documentation~/ScreenShake.md).

## Dependencies
- `com.bftools.core` @ 0.3.0
- `com.unity.inputsystem` @ 1.7.0

## Installation

> **Note:** This package is still under development and hasn't been merged to `main` yet, so the URLs below point at the `development` branch. Once it goes live on `main`, drop the `#development` segment (or switch it to a release tag) and update this README accordingly.

### Via Package Manager (git URL)
1. Open **Window > Package Manager**.
2. Click **+ > Add package from git URL...**
3. Enter:
   ```
   https://github.com/Brainfart-Studio/BF-Tools.git?path=Packages/Feedback#development
   ```

Feedback depends on `com.bftools.core`; install it the same way (see [Core's README](../Core/README.md#installation)) if it isn't already in the project.

### Via manifest.json
Add the entry directly to your project's `Packages/manifest.json`:

```json
"dependencies": {
  "com.bftools.feedback": "https://github.com/Brainfart-Studio/BF-Tools.git?path=Packages/Feedback#development"
}
```

### As a dependency of another package
Reference `com.bftools.feedback` from a dependent package's `package.json`:

```json
"dependencies": {
  "com.bftools.feedback": "0.3.0"
}
```

### Note on embedding
Install via git URL (or as a registry/UPM dependency) rather than copying this folder directly into a project's `Packages/` directory. Git/UPM installs are mounted by the package's `name` (`com.bftools.feedback`), which is what the editor tooling's hardcoded asset paths (e.g. the Haptics and Screen Shake prefab variant creators) expect. A directly embedded folder is mounted by its on-disk name (`Feedback`) instead, which will break those paths.