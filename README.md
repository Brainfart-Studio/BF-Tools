# BF-Tools

Monorepo for BFTools, a personal Unity toolkit and game jam speed pack, built around config-driven ScriptableObjects and a lightweight event bus. No DI containers, no speculative abstraction: systems are added when there's a real second use case.

## Packages

### [Core](Packages/Core/README.md): `com.bftools.core` (0.2.0)
Foundational systems every other package builds on.
- **Bootstrapper**: Global (app-lifetime) and Level (per-scene) system initialization. See [Bootstrapper.md](Packages/Core/Documentation~/Bootstrapper.md).
- **Event Bus**: generic static pub/sub system for struct-based events. See [EventBus.md](Packages/Core/Documentation~/EventBus.md).

No dependencies.

### [Feedback](Packages/Feedback/README.md): `com.bftools.feedback` (0.2.0)
Event-driven player feedback.
- **Haptics**: controller rumble triggered by named events. See [Haptics.md](Packages/Feedback/Documentation~/Haptics.md).
- **Screen Shake**: camera shake triggered by named events. See [ScreenShake.md](Packages/Feedback/Documentation~/ScreenShake.md).

Depends on `com.bftools.core` (0.2.0) and `com.unity.inputsystem` (1.7.0).

## Structure
```
Packages/
  Core/                    com.bftools.core
    GlobalBootstrapper/      Editor/, Runtime/
    LevelBootstrapper/       Editor/, Runtime/, Prefabs/
    EventBus/                Runtime/
    Documentation~/
  Feedback/                com.bftools.feedback
    Haptics/                 Editor/, Runtime/, Prefabs/
    ScreenShake/              Editor/, Runtime/, Prefabs/
    Documentation~/
```

## Installation
Each package installs independently via git URL (recommended) or as a `manifest.json` dependency. See each package's README for exact steps:
- [Core installation](Packages/Core/README.md#installation)
- [Feedback installation](Packages/Feedback/README.md#installation)

> **Don't copy a package folder directly into your project's `Packages/` directory.** Embedded packages are mounted in the AssetDatabase under their on-disk folder name (`Packages/Core/...`), not the package's declared name (`Packages/com.bftools.core/...`). This repo's editor tooling, including the Level Bootstrapper, Haptics, and Screen Shake prefab variant creators, hardcodes asset lookups against the declared package name, so an embedded copy will fail to find its base prefab and log an error. Always install via git URL, a registry, or a `file:` dependency in `manifest.json` instead, which Unity mounts by package name automatically.

## Requirements
- Unity 2022.3+
- `com.unity.inputsystem` 1.7.0+ (Feedback only)

## Changelogs
Each package tracks its own version and history:
- [Core CHANGELOG](Packages/Core/CHANGELOG.md)
- [Feedback CHANGELOG](Packages/Feedback/CHANGELOG.md)