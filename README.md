# BF-Tools

Monorepo for BFTools, a personal Unity toolkit and game jam speed pack, built around config-driven ScriptableObjects and a lightweight event bus. No DI containers, no speculative abstraction: systems are added when there's a real second use case.

> **Active development.** Everything on `development` is still being built and tested. Most systems should work, but nothing here has been rigorously tested enough to rely on in a commercial project yet. Use at your own risk until a package hits a tagged release on `main`.

## Packages

### [Core](Packages/Core/README.md): `com.bftools.core` (0.10.1)
Foundational systems every other package builds on.
- **Event Bus**: generic static pub/sub system for struct-based events. See [EventBus.md](Packages/Core/Documentation~/EventBus.md).
- **Editor Asset Utility**: shared editor-only helpers (folder creation, config asset creation, prefab variant creation) used by this repo's `Assets/Create/BFTools/...` menu creators.
- **Logger**: tag-based logging with per-tag level overrides and pluggable sinks. See [Logger.md](Packages/Core/Documentation~/Logger.md).
- **Service Locator**: static registry for locating shared services by type at runtime. See [ServiceLocator.md](Packages/Core/Documentation~/ServiceLocator.md).

No dependencies.

### [Systems](Packages/Systems/README.md): `com.bftools.systems` (0.4.1)
Gameplay-facing systems built on Core.
- **Bootstrapper**: Global (app-lifetime) and Level (per-scene) system initialization. See [Bootstrapper.md](Packages/Systems/Documentation~/Bootstrapper.md).
- **Object Pooler**: config-driven object pooling with prewarming and Get/Release, registered with the Service Locator. See [ObjectPooler.md](Packages/Systems/Documentation~/ObjectPooler.md).
- **Save System**: ISaveable-based state capture/restore, saved to encrypted, checksummed, slot-based files with a per-install key and a version migration scaffold. See [SaveSystem.md](Packages/Systems/Documentation~/SaveSystem.md).
- **Scene Manager**: additive scene loading and transitions, orchestrated through a config asset, a static loader, a set of swappable transitions (fade, wipe, radial wipe, iris), and door/preload trigger components. See [SceneManager.md](Packages/Systems/Documentation~/SceneManager.md).
- **Settings Manager**: ISettingsProvider-based state capture/restore, saved as plain JSON to a single shared settings file. See [SettingsManager.md](Packages/Systems/Documentation~/SettingsManager.md).

Depends on `com.bftools.core` (0.10.0) and `com.unity.nuget.newtonsoft-json` (3.2.1).

### [Feedback](Packages/Feedback/README.md): `com.bftools.feedback` (0.6.2)
Event-driven player feedback.
- **Haptics**: controller rumble triggered by named events. See [Haptics.md](Packages/Feedback/Documentation~/Haptics.md).
- **Screen Shake**: camera shake triggered by named events. See [ScreenShake.md](Packages/Feedback/Documentation~/ScreenShake.md).
- **Hitstop**: brief global time freeze triggered by named events. See [Hitstop.md](Packages/Feedback/Documentation~/Hitstop.md).
- **Screen Flash**: full-screen color flash triggered by named events. See [ScreenFlash.md](Packages/Feedback/Documentation~/ScreenFlash.md).

Depends on `com.bftools.core` (0.10.0) and `com.unity.inputsystem` (1.7.0).

### [Visuals](Packages/Visuals/README.md): `com.bftools.visuals` (0.6.2)
Live-editable, config-driven visual systems.
- **Palette**: live-editable color palette for rapid prototyping, driven by named entries on a config asset. See [Palette.md](Packages/Visuals/Documentation~/Palette.md).
- **Background**: layered background rendering, compositing gradient, aurora ribbon, and twinkling star layers behind the scene via a dedicated camera. See [Background.md](Packages/Visuals/Documentation~/Background.md).
- **Parallax**: layered parallax scrolling, moving a stack of sprite layers at different rates relative to the camera, with per-layer looping and one-way movement lock. See [Parallax.md](Packages/Visuals/Documentation~/Parallax.md).

Depends on `com.bftools.core` (0.10.0).

### [EditorTools](Packages/EditorTools/README.md): `com.bftools.editortools` (0.1.0)
Editor-only tooling for bootstrapping and verifying a new project.
- **Project Setup**: one-click creation of the Logger, Global Bootstrapper, and Feedback module configs/prefabs, wired together and seeded with a `"Default"` entry. See [EditorTools README](Packages/EditorTools/README.md#project-setup).
- **Project Verification**: creates a `BFToolsTest` scene with bouncing test balls and buttons to fire each Feedback event, for visually confirming the setup works. See [EditorTools README](Packages/EditorTools/README.md#project-verification).

Depends on `com.bftools.core` (0.10.1), `com.bftools.systems` (0.4.1), `com.bftools.feedback` (0.6.2), `com.unity.textmeshpro` (3.0.6), and `com.unity.inputsystem` (1.7.0).

## Structure
```
Packages/
  Core/                    com.bftools.core
    EventBus/                Runtime/
    EditorAssetUtility/      Editor/
    Logger/                  Editor/, Runtime/
    ServiceLocator/          Runtime/
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

> **Don't copy a package folder directly into your project's `Packages/` directory.** Embedded packages are mounted in the AssetDatabase under their on-disk folder name (`Packages/Core/...`), not the package's declared name (`Packages/com.bftools.core/...`). This repo's editor tooling, including the Level Bootstrapper, Object Pooler, Scene Transition Controller, Haptics, Screen Shake, Hitstop, Screen Flash, and Project Setup prefab variant creators, hardcodes asset lookups against the declared package name, so an embedded copy will fail to find its base prefab and log an error. Always install via git URL, a registry, or a `file:` dependency in `manifest.json` instead, which Unity mounts by package name automatically.

## Requirements
- Unity 2022.3+
- `com.unity.inputsystem` 1.7.0+ (Feedback, EditorTools)
- `com.unity.nuget.newtonsoft-json` 3.2.1+ (Systems / Save System only)
- `com.unity.textmeshpro` 3.0.6+ (EditorTools only)

## Changelogs
Each package tracks its own version and history:
- [Core CHANGELOG](Packages/Core/CHANGELOG.md)
- [Systems CHANGELOG](Packages/Systems/CHANGELOG.md)
- [Feedback CHANGELOG](Packages/Feedback/CHANGELOG.md)
- [Visuals CHANGELOG](Packages/Visuals/CHANGELOG.md)
- [EditorTools CHANGELOG](Packages/EditorTools/CHANGELOG.md)