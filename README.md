# BF-Tools

Monorepo for BFTools — a personal Unity toolkit and game jam speed pack, built around config-driven ScriptableObjects and a lightweight event bus. No DI containers, no speculative abstraction — systems are added when there's a real second use case.

## Packages

### [Core](Packages/Core/README.md) (`com.bftools.core`)
Bootstrapping (Global/Level) and the generic `EventBus<T>` pub/sub system. Foundation for all other packages.

### [Feedback](Packages/Feedback/README.md) (`com.bftools.feedback`)
Event-driven feedback: controller haptics and camera screen shake.

## Structure
Packages/
Core/
Bootstrapper/
EventBus/
Documentation/
Feedback/
Haptics/
ScreenShake/
Documentation/

## Installation
This is a Unity project repo with packages under `Packages/`. Clone and open in Unity, or reference individual packages via git URL with a `path` parameter — see each package's README for details.