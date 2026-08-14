# BFTools Feedback (`com.bftools.feedback`)

Event-driven feedback systems: controller haptics, camera screen shake, hitstop, screen flash, and controller LED color.

## Version
1.4.0

## Contents

### Haptics
Controller rumble triggered by named events. See [Documentation~/Haptics.md](Documentation~/Haptics.md).

### Screen Shake
Camera shake triggered by named events. See [Documentation~/ScreenShake.md](Documentation~/ScreenShake.md).

### Hitstop
Brief global time freeze triggered by named events. See [Documentation~/Hitstop.md](Documentation~/Hitstop.md).

### Screen Flash
Full-screen color flash triggered by named events. See [Documentation~/ScreenFlash.md](Documentation~/ScreenFlash.md).

### Controller LED
Gamepad LED/light bar color, set directly or triggered by named events. Includes `BFControllerLedRainbow`, a drop-in fallback effect that cycles the LED through a rainbow for projects that just want it to do something, and `BFControllerLedColorFeeder`, which forwards colors sampled by `com.bftools.core`'s `BFScreenColorSampler` straight to the LED. PS4 only for now. See [Documentation~/ControllerLED.md](Documentation~/ControllerLED.md).

### Vignette
Full-screen radial mask, tinted and faded in/out over time, triggered by named events. Supports a wavy (non-circular) edge and multiple simultaneous/layered triggers. See [Documentation~/Vignette.md](Documentation~/Vignette.md).

### SloMo
Sustained slow motion triggered by named events, eased in and out over a config-defined curve. See [Documentation~/SloMo.md](Documentation~/SloMo.md).

## Dependencies
- `com.bftools.core` @ 1.1.0
- `com.unity.inputsystem` @ 1.7.0

## Installation

### Via Package Manager (git URL)
1. Open **Window > Package Manager**.
2. Click **+ > Add package from git URL...**
3. Enter:
   ```
   https://github.com/Brainfart-Studio/BF-Tools.git?path=Packages/Feedback
   ```

Feedback depends on `com.bftools.core`; install it the same way (see [Core's README](../Core/README.md#installation)) if it isn't already in the project.

### Via manifest.json
Add the entry directly to your project's `Packages/manifest.json`:

```json
"dependencies": {
  "com.bftools.feedback": "https://github.com/Brainfart-Studio/BF-Tools.git?path=Packages/Feedback"
}
```

### As a dependency of another package
Reference `com.bftools.feedback` from a dependent package's `package.json`:

```json
"dependencies": {
  "com.bftools.feedback": "1.4.0"
}
```

### Note on embedding
Install via git URL (or as a registry/UPM dependency) rather than copying this folder directly into a project's `Packages/` directory. Git/UPM installs are mounted by the package's `name` (`com.bftools.feedback`), which is what the editor tooling's hardcoded asset paths (e.g. the Haptics, Screen Shake, Hitstop, Screen Flash, Controller LED, Vignette, and SloMo config/prefab creators) expect. A directly embedded folder is mounted by its on-disk name (`Feedback`) instead, which will break those paths.