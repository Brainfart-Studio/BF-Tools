# BF-Tools

Monorepo for BFTools, a personal Unity toolkit and game jam speed pack, built around config-driven ScriptableObjects and a lightweight event bus. No DI containers, no speculative abstraction: systems are added when there's a real second use case.

## Packages

### [Core](Packages/Core/README.md): `com.bftools.core` (1.1.0)
Foundational systems every other package builds on.
- **Event Bus**: generic static pub/sub system for struct-based events. See [EventBus.md](Packages/Core/Documentation~/EventBus.md).
- **Editor Asset Utility**: shared editor-only helpers (folder creation, config asset creation, prefab variant creation) used by this repo's `Assets/Create/BFTools/...` menu creators.
- **Logger**: tag-based logging with per-tag level overrides and pluggable sinks. See [Logger.md](Packages/Core/Documentation~/Logger.md).
- **Project Setup**: `IBFProjectSetupStep`, `IBFSystemPrefabContributor`, and `IBFSystemPrefabConsumer` interfaces for building self-contained, auto-discovered setup tooling — any implementer anywhere in the project is picked up automatically by EditorTools' `BF Tools > New Project Setup`. See [EditorTools README](Packages/EditorTools/README.md#project-setup) for the tool that consumes these.
- **Service Locator**: static registry for locating shared services by type at runtime. See [ServiceLocator.md](Packages/Core/Documentation~/ServiceLocator.md).
- **Screen Color Sampler**: averages camera-rendered color within a world-space box that follows a GameObject's transform, exposed as `CurrentColor` and a `ColorSampled` event. See [ScreenColorSampler.md](Packages/Core/Documentation~/ScreenColorSampler.md).

Depends on `com.unity.nuget.newtonsoft-json` (3.2.1).

### [Systems](Packages/Systems/README.md): `com.bftools.systems` (1.0.0)
Gameplay-facing systems built on Core.
- **Bootstrapper**: Global (app-lifetime) and Level (per-scene) system initialization. See [Bootstrapper.md](Packages/Systems/Documentation~/Bootstrapper.md).
- **Object Pooler**: config-driven object pooling with prewarming and Get/Release, registered with the Service Locator. See [ObjectPooler.md](Packages/Systems/Documentation~/ObjectPooler.md).
- **Save System**: ISaveable-based state capture/restore, saved to encrypted, checksummed, slot-based files with a per-install key and a version migration scaffold. See [SaveSystem.md](Packages/Systems/Documentation~/SaveSystem.md).
- **Scene Manager**: additive scene loading and transitions, orchestrated through a config asset, a static loader, a set of swappable transitions (fade, wipe, radial wipe, iris), and door/preload trigger components. See [SceneManager.md](Packages/Systems/Documentation~/SceneManager.md).
- **Settings Manager**: ISettingsProvider-based state capture/restore, saved as plain JSON to a single shared settings file. See [SettingsManager.md](Packages/Systems/Documentation~/SettingsManager.md).

Depends on `com.bftools.core` (1.0.0) and `com.unity.nuget.newtonsoft-json` (3.2.1).

### [Feedback](Packages/Feedback/README.md): `com.bftools.feedback` (1.3.0)
Event-driven player feedback.
- **Haptics**: controller rumble triggered by named events. See [Haptics.md](Packages/Feedback/Documentation~/Haptics.md).
- **Screen Shake**: camera shake triggered by named events. See [ScreenShake.md](Packages/Feedback/Documentation~/ScreenShake.md).
- **Hitstop**: brief global time freeze triggered by named events. See [Hitstop.md](Packages/Feedback/Documentation~/Hitstop.md).
- **Screen Flash**: full-screen color flash triggered by named events. See [ScreenFlash.md](Packages/Feedback/Documentation~/ScreenFlash.md).
- **Controller LED**: gamepad LED/light bar color, set directly or triggered by named events, plus a drop-in rainbow fallback effect and a `BFControllerLedColorFeeder` that forwards colors from Core's `BFScreenColorSampler` straight to the LED. PS4 only for now. See [ControllerLED.md](Packages/Feedback/Documentation~/ControllerLED.md).
- **Vignette**: full-screen radial mask, tinted and faded in/out over time, triggered by named events, with a wavy (non-circular) edge and support for multiple simultaneous/layered triggers. See [Vignette.md](Packages/Feedback/Documentation~/Vignette.md).

Depends on `com.bftools.core` (1.1.0) and `com.unity.inputsystem` (1.7.0).

### [Visuals](Packages/Visuals/README.md): `com.bftools.visuals` (1.0.0)
Live-editable, config-driven visual systems.
- **Palette**: live-editable color palette for rapid prototyping, driven by named entries on a config asset. See [Palette.md](Packages/Visuals/Documentation~/Palette.md).
- **Background**: layered background rendering, compositing gradient, aurora ribbon, and twinkling star layers behind the scene via a dedicated camera. See [Background.md](Packages/Visuals/Documentation~/Background.md).
- **Parallax**: layered parallax scrolling, moving a stack of sprite layers at different rates relative to the camera, with per-layer looping and one-way movement lock. See [Parallax.md](Packages/Visuals/Documentation~/Parallax.md).

Depends on `com.bftools.core` (1.0.0).

### [EditorTools](Packages/EditorTools/README.md): `com.bftools.editortools` (1.0.0)
Editor-only tooling for bootstrapping and verifying a new project.
- **Project Setup**: `BF Tools > New Project Setup` discovers and runs every `IBFProjectSetupStep` in the project via `TypeCache` — no hardcoded knowledge of what gets created or where. With Core, Systems, and Feedback installed, this currently creates the Logger, Global Bootstrapper, and Feedback module configs/prefabs, wired together and seeded with a `"Default"` entry. See [EditorTools README](Packages/EditorTools/README.md#project-setup).
- **Project Verification**: creates a `BFToolsTest` scene with bouncing test balls and buttons to fire each Feedback event, for visually confirming the setup works. See [EditorTools README](Packages/EditorTools/README.md#project-verification).

Depends on `com.bftools.core` (1.0.0), `com.bftools.systems` (1.0.0), `com.bftools.feedback` (1.0.0), `com.unity.textmeshpro` (3.0.6), and `com.unity.inputsystem` (1.7.0).

## Structure
```
Packages/
  Core/                    com.bftools.core
    EventBus/                Runtime/
    EditorAssetUtility/      Editor/
    Logger/                  Editor/, Runtime/
    ProjectSetup/            Editor/
    ServiceLocator/          Runtime/
    ScreenColorSampler/      Runtime/, Tests/
    Documentation~/
  Systems/                 com.bftools.systems
    EditorAssetUtility/      Editor/
    GlobalBootstrapper/      Editor/, Runtime/
    LevelBootstrapper/       Editor/, Runtime/, Prefabs/
    ObjectPooler/            Editor/, Runtime/, Prefabs/
    SaveSystem/              Runtime/
    SceneManager/            Editor/, Runtime/, Prefabs/
    SettingsManager/         Runtime/
    Documentation~/
  Feedback/                com.bftools.feedback
    EditorAssetUtility/      Editor/
    Haptics/                 Editor/, Runtime/, Prefabs/
    ScreenShake/              Editor/, Runtime/, Prefabs/
    Hitstop/                 Editor/, Runtime/, Prefabs/
    ScreenFlash/             Editor/, Runtime/, Prefabs/
    ControllerLED/           Editor/, Runtime/, Tests/
    Vignette/                Editor/, Runtime/, Prefabs/, Tests/
    Documentation~/
  Visuals/                 com.bftools.visuals
    EditorAssetUtility/      Editor/
    Palette/                 Editor/, Runtime/
    Background/              Editor/, Runtime/, Prefabs/
    Parallax/                Editor/, Runtime/, Prefabs/
    Documentation~/
  EditorTools/             com.bftools.editortools
    ProjectSetup/             Editor/
    ProjectVerification/      Editor/, Runtime/
```

## Installation
Each package installs independently via git URL (recommended) or as a `manifest.json` dependency. See each package's README for exact steps:
- [Core installation](Packages/Core/README.md#installation)
- [Systems installation](Packages/Systems/README.md#installation)
- [Feedback installation](Packages/Feedback/README.md#installation)
- [Visuals installation](Packages/Visuals/README.md#installation)
- [EditorTools installation](Packages/EditorTools/README.md#installation)

> **Don't copy a package folder directly into your project's `Packages/` directory.** Embedded packages are mounted in the AssetDatabase under their on-disk folder name (`Packages/Core/...`), not the package's declared name (`Packages/com.bftools.core/...`). This repo's editor tooling, including the Level Bootstrapper, Object Pooler, Scene Transition Controller, Haptics, Screen Shake, Hitstop, Screen Flash, and Vignette prefab variant creators, hardcodes asset lookups against the declared package name, so an embedded copy will fail to find its base prefab and log an error. EditorTools' `New Project Setup` runs those same creators (among others) via `TypeCache` discovery rather than hardcoding paths itself, but it inherits the same failure mode, since the underlying creators it discovers still do. Always install via git URL, a registry, or a `file:` dependency in `manifest.json` instead, which Unity mounts by package name automatically.

## Requirements
- Unity 2022.3+
- `com.unity.inputsystem` 1.7.0+ (Feedback, EditorTools)
- `com.unity.nuget.newtonsoft-json` 3.2.1+ (Core / Serialization, Systems / Save System)
- `com.unity.textmeshpro` 3.0.6+ (EditorTools only)

## Changelogs
Each package tracks its own version and history:
- [Core CHANGELOG](Packages/Core/CHANGELOG.md)
- [Systems CHANGELOG](Packages/Systems/CHANGELOG.md)
- [Feedback CHANGELOG](Packages/Feedback/CHANGELOG.md)
- [Visuals CHANGELOG](Packages/Visuals/CHANGELOG.md)
- [EditorTools CHANGELOG](Packages/EditorTools/CHANGELOG.md)