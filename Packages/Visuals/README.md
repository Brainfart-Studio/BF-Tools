# BFTools Visuals (`com.bftools.visuals`)

Visual systems for BFTools. Live-editable, config-driven color palettes.

## Version
0.1.0

## Contents

### Palette
Live-editable color palette for rapid prototyping. Pick a named color per `SpriteRenderer` and repaint every object using that name the moment the palette config changes. See [Documentation~/Palette.md](Documentation~/Palette.md).

## Dependencies
- `com.bftools.core` @ 0.7.3

## Installation

> **Note.** This package is still under development and hasn't been merged to `main` yet, so the URLs below point at the `development` branch. Once it goes live on `main`, drop the `#development` segment (or switch it to a release tag) and update this README accordingly.

### Via Package Manager (git URL)
1. Open **Window > Package Manager**.
2. Click **+ > Add package from git URL...**
3. Enter this URL.
   ```
   https://github.com/Brainfart-Studio/BF-Tools.git?path=Packages/Visuals#development
   ```

Visuals depends on `com.bftools.core`; install it the same way (see [Core's README](../Core/README.md#installation)) if it isn't already in the project.

### Via manifest.json
Add the entry directly to your project's `Packages/manifest.json`.

```json
"dependencies": {
  "com.bftools.visuals": "https://github.com/Brainfart-Studio/BF-Tools.git?path=Packages/Visuals#development"
}
```

### As a dependency of another package
Reference `com.bftools.visuals` from a dependent package's `package.json`.

```json
"dependencies": {
  "com.bftools.visuals": "0.1.0"
}
```

### Note on embedding
Install via git URL (or as a registry/UPM dependency) rather than copying this folder directly into a project's `Packages/` directory. Git/UPM installs are mounted by the package's `name` (`com.bftools.visuals`); a directly embedded folder is mounted by its on-disk name (`Visuals`) instead. This repo's editor tooling generally expects the declared package name, so embedding will break asset lookups for any future tooling that needs it (a prefab variant creator, for example).