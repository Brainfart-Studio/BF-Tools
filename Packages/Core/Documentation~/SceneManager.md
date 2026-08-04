# Scene Manager

Additive scene loading and transitions, orchestrated through a config asset, a static loader, a fade, and two trigger components for room-to-room streaming.

## Setup
1. `Assets/Create/BFTools/Core/Config/Scene Load Request` creates a `BFSceneLoadRequest` asset at `Assets/Configs/Core/SceneManager`. Set `Scene Name`, `Load Mode`, `Show Loading Screen`, and `Minimum Display Time`.
2. `Assets/Create/BFTools/Core/Prefabs/Scene Transition Controller` creates a prefab variant of the base `SceneTransitionController` prefab at `Assets/Prefabs/Core`. The base prefab already includes a full-screen fade Canvas wired to `BFFadeTransition`, adjust `Fade Out Duration` / `Fade In Duration` on the variant if needed.
3. Place the variant in the scene, or wire it into the Global Bootstrapper so it survives scene loads.
4. Add `BFDoorActivationTrigger` or `BFPreloadZoneTrigger` to a 2D trigger collider, assign a `BFSceneLoadRequest`, and set `Player Tag`.

## Usage
Trigger a transition from anywhere with access to the controller.
```csharp
BFServiceLocator.Get<BFSceneTransitionController>().BeginTransition(request);
```
`BFDoorActivationTrigger` calls this automatically on `OnTriggerEnter2D` when the entering collider matches `Player Tag`, then suppresses itself until the player exits, so walking back and forth across a door doesn't refire the transition.

`BFPreloadZoneTrigger` calls `BFSceneLoader.Preload` instead, loading the target scene ahead of time with `allowSceneActivation` set to false. When the player later reaches a door for that same scene, `BeginTransition` finds it already tracked and activates it rather than loading it from scratch.

## How it works
- `BFSceneLoader` is a static class holding a dictionary of scene name to `AsyncOperation`. `LoadAsync` and `Preload` both check this dictionary first and no-op on a duplicate request for the same scene name.
- `Preload` sets `allowSceneActivation` to false so the scene loads to 90% and waits. `ActivateAsync` flips it back to true and awaits completion.
- `BFSceneTransitionController.BeginTransition` starts a coroutine that fires `BFSceneTransitionStartedEvent` (carrying `sceneName` and `showLoadingScreen`), plays the fade out, loads or activates the target scene, holds for whatever's left of `MinimumDisplayTime` after the load finishes, fires `BFSceneLoadedEvent`, plays the fade in, unloads the previous scene, then fires `BFSceneTransitionCompleteEvent`.
- A second `BeginTransition` call while one is already running is ignored with a warning, so nothing races.
- `BFFadeTransition` implements `ITransition` and only touches a `CanvasGroup`'s `alpha` and `blocksRaycasts`. The visible fade image underneath it is a plain UI Image with no sprite. The screen fades through whatever color that image is set to, black by default in the shipped prefab.
- `BFSceneLoader.UnloadAsync` checks `SceneManager.GetSceneByName(sceneName).isLoaded` before calling `UnloadSceneAsync`, since a `Single` mode load already unloads everything else on its own and a second unload call on a scene that's already gone would throw.

## Notes
- `BFDoorActivationTrigger` and `BFPreloadZoneTrigger` use `OnTriggerEnter2D` / `Collider2D`. A 3D project needs 3D equivalents.
- `BFServiceLocator.Get<BFSceneTransitionController>()` throws `KeyNotFoundException` if no controller has registered yet. Make sure one is in the scene, or spawned by the Global Bootstrapper, before any trigger can fire.
- The controller never unloads whatever scene was loaded before the first `BeginTransition` call. A boot or persistent scene left loaded this way stays loaded for the life of the session, which is what you want if it's meant to persist. A scene meant to be replaced needs to go through its own transition call, the same as any other.
- Depends on `com.bftools.core` (Logger, EventBus, ServiceLocator). Editor tooling additionally depends on `BFTools.Core.EditorAssetUtility.Editor`.