# BFTools Visuals (`com.bftools.visuals`)

Visual systems for BFTools. Live-editable, config-driven color palettes, plus layered background and parallax scrolling systems.

## Version
1.0.0

## Contents

### Palette
Live-editable color palette for rapid prototyping. Pick a named color per `SpriteRenderer` and repaint every object using that name the moment the palette config changes. See [Documentation~/Palette.md](Documentation~/Palette.md).

### Background
Layered background rendering. Composite gradient, aurora ribbon, and twinkling star layers behind the scene using a dedicated camera. See [Documentation~/Background.md](Documentation~/Background.md).

### Parallax
Layered parallax scrolling. Move a stack of sprite layers at different rates relative to the camera, with per-layer looping and one-way movement lock. See [Documentation~/Parallax.md](Documentation~/Parallax.md).

## Dependencies
- `com.bftools.core` @ 1.0.0

## Installation

### Via Package Manager (git URL)
1. Open **Window > Package Manager**.
2. Click **+ > Add package from git URL...**
3. Enter this URL.
   ```
   https://github.com/Brainfart-Studio/BF-Tools.git?path=Packages/Visuals
   ```

Visuals depends on `com.bftools.core`; install it the same way (see [Core's README](../Core/README.md#installation)) if it isn't already in the project.

### Via manifest.json
Add the entry directly to your project's `Packages/manifest.json`.

```json
"dependencies": {
  "com.bftools.visuals": "https://github.com/Brainfart-Studio/BF-Tools.git?path=Packages/Visuals"
}
```

### As a dependency of another package
Reference `com.bftools.visuals` from a dependent package's `package.json`.

```json
"dependencies": {
  "com.bftools.visuals": "1.0.0"
}
```

### Note on embedding
Install via git URL (or as a registry/UPM dependency) rather than copying this folder directly into a project's `Packages/` directory. Git/UPM installs are mounted by the package's `name` (`com.bftools.visuals`); a directly embedded folder is mounted by its on-disk name (`Visuals`) instead. This repo's editor tooling generally expects the declared package name, so embedding will break asset lookups for any future tooling that needs it (a prefab variant creator, for example).