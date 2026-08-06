# Scene Manager

Additive scene loading and transitions, orchestrated through a config asset, a static loader, a set of transition components, and two trigger components for room-to-room streaming.

## Setup
1. `Assets/Create/BFTools/Systems/Config/Scene Load Request` creates a `BFSceneLoadRequest` asset at `Assets/Configs/Systems/SceneManager`. Set `Scene Name`, `Load Mode`, `Show Loading Screen`, and `Minimum Display Time`.
2. `Assets/Create/BFTools/Systems/Prefabs/Scene Transition Controller` creates a prefab variant of the base `SceneTransitionController` prefab at `Assets/Prefabs/Systems`. The base prefab ships with a full-screen fade Canvas wired to `BFFadeTransition`; adjust `Fade Out Duration` / `Fade In Duration` on the variant if needed, or swap in a different transition entirely (see [Transitions](#transitions)).
3. Place the variant in the scene, or wire it into the Global Bootstrapper so it survives scene loads.
4. Add `BFDoorActivationTrigger` or `BFPreloadZoneTrigger` to a 2D trigger collider, assign a `BFSceneLoadRequest`, and set `Player Tag`.

## Usage
Trigger a transition from anywhere with access to the controller.
```csharp
BFServiceLocator.Get<BFSceneTransitionController>().BeginTransition(request);
```
`BFDoorActivationTrigger` calls this automatically on `OnTriggerEnter2D` when the entering collider matches `Player Tag`, then suppresses itself until the player exits, so walking back and forth across a door doesn't refire the transition.

`BFPreloadZoneTrigger` calls `BFSceneLoader.Preload` instead, loading the target scene ahead of time with `allowSceneActivation` set to false. When the player later reaches a door for that same scene, `BeginTransition` finds it already tracked and activates it rather than loading it from scratch.

## Transitions
`BFSceneTransitionController` holds a single `[SerializeField] BFTransitionBehaviour transition`, an abstract `MonoBehaviour` implementing `ITransition`'s `PlayOut`/`PlayIn` coroutines. Any of the components below can be assigned to that field; swapping styles means wiring a different component into the prefab variant, not writing code.

### BFFadeTransition
Fades a `CanvasGroup`'s `alpha` from 0 to 1 on `PlayOut` and back on `PlayIn`, toggling `blocksRaycasts` to match. The visible fade image underneath it is a plain UI `Image` with no sprite; the screen fades through whatever color that image is set to, black by default in the shipped prefab.

Fields: `Fade Out Duration`, `Fade In Duration`.

### BFWipeTransition
A directional colored wipe with a separate edge/border color and thickness. It renders as two full-screen `Image` rects (`mainImage`, `edgeImage`) rotated to `Angle` and translated across the screen; the edge image is sized `Edge Thickness` larger on both sides and kept behind `mainImage` in draw order, so only a thin colored sliver shows at whichever edge (leading or trailing) is currently crossing the screen. `PlayOut` sweeps the wipe from off-screen to fully covering; `PlayIn` continues in the same direction, revealing the new scene as the trailing edge crosses. Duration maps to the wipe actually crossing the screen, with no dead time off-screen, because the sweep distance is computed from the screen's own corners projected onto the wipe direction, the same technique `BFGradientLayer` (Visuals package) uses for its angled gradient.

Optional: `Edge Sprite Image`, an `Image` that tracks the same leading/trailing edge the border does, oriented to face the direction of travel and sized via `Edge Sprite Size`. Left unassigned, it's skipped entirely. Use it for a decorative element riding the front of the wipe, for example a lightning bolt.

Fields: `Wipe Color`, `Edge Color`, `Edge Thickness`, `Angle`, `Wipe Out Duration`, `Wipe In Duration`, `Edge Sprite Image` (optional), `Edge Sprite Size`.

Wiring: a `RectTransform` sized to the screen (`Screen Rect`), with two (or three, if using the edge sprite) child `Image` GameObjects parented directly under it. Anchors, pivot, and rotation on the child images are all set in code at play time, so they don't need manual setup beyond creating them and wiring the references.

### BFRadialWipeTransition
A radial wipe starting at 12 o'clock, built on Unity's `Image.fillMethod = Radial360` rather than any custom geometry. `PlayOut` fills clockwise (or counterclockwise, via `Clockwise`) from empty to fully covering; `PlayIn` unwinds the same fill back to empty, which visually returns to 12 o'clock. Raycasts are blocked while covered and released once revealed.

Fields: `Color`, `Clockwise`, `Wipe Out Duration`, `Wipe In Duration`.

Wiring: one full-screen `Image` assigned to `Image`. No special anchor or hierarchy requirements.

### BFIrisWipeTransition
An iris/point wipe (Looney Tunes-style) built on Unity's 2D `SpriteMask`, not a Canvas element. A full-screen `SpriteRenderer` curtain is set to `maskInteraction = VisibleOutsideMask`, so it's hidden everywhere a `SpriteMask` sprite overlaps it. Scaling that mask sprite is the entire effect: `PlayOut` shrinks it from `Revealed Mask Scale` down to `Closed Mask Scale`, closing the iris and covering the screen; `PlayIn` grows it back out, revealing the new scene. An optional `Rotation Speed` spins the mask while it scales.

Because the mask sprite is swappable in the Inspector, this same component covers both a plain circular iris and a custom shape acting as a clipping mask, for example a Banjo-Kazooie-style logo wipe. Only the assigned sprite changes.

Fields: `Curtain Color`, `Revealed Mask Scale`, `Closed Mask Scale`, `Rotation Speed`, `Wipe Out Duration`, `Wipe In Duration`.

Wiring: a GameObject with a `SpriteRenderer` (`Curtain Renderer`) and a child GameObject with a `SpriteMask` component (`sprite` set to a circle by default, or any custom shape) whose `Transform` is assigned to `Mask Transform`. This transition needs its own render setup outside the Canvas the other three share. `Revealed Mask Scale` has to be tuned by eye against the project's camera and mask sprite; there's no generic way to compute "fully covers the screen" across arbitrary orthographic sizes and mask art.

## How it works
- `BFSceneLoader` is a static class holding a dictionary of scene name to `AsyncOperation`. `LoadAsync` and `Preload` both check this dictionary first and no-op on a duplicate request for the same scene name.
- `Preload` sets `allowSceneActivation` to false so the scene loads to 90% and waits. `ActivateAsync` flips it back to true and awaits completion.
- `BFSceneTransitionController.BeginTransition` starts a coroutine that fires `BFSceneTransitionStartedEvent` (carrying `sceneName` and `showLoadingScreen`), plays the assigned transition's `PlayOut`, loads or activates the target scene, holds for whatever's left of `MinimumDisplayTime` after the load finishes, fires `BFSceneLoadedEvent`, plays `PlayIn`, unloads the previous scene, then fires `BFSceneTransitionCompleteEvent`.
- A second `BeginTransition` call while one is already running is ignored with a warning, so nothing races.
- `BFSceneLoader.UnloadAsync` checks `SceneManager.GetSceneByName(sceneName).isLoaded` before calling `UnloadSceneAsync`, since a `Single` mode load already unloads everything else on its own and a second unload call on a scene that's already gone would throw.

## Notes
- `BFDoorActivationTrigger` and `BFPreloadZoneTrigger` use `OnTriggerEnter2D` / `Collider2D`. A 3D project needs 3D equivalents.
- `BFServiceLocator.Get<BFSceneTransitionController>()` throws `KeyNotFoundException` if no controller has registered yet. Make sure one is in the scene, or spawned by the Global Bootstrapper, before any trigger can fire.
- The controller never unloads whatever scene was loaded before the first `BeginTransition` call. A boot or persistent scene left loaded this way stays loaded for the life of the session, which is what you want if it's meant to persist. A scene meant to be replaced needs to go through its own transition call, the same as any other.
- `BFIrisWipeTransition`'s curtain is a `SpriteRenderer`, not UI. It won't block clicks on Canvas buttons visible during the transition the way the other three transitions do; add a separate UI blocker if that matters for a given scene.
- Depends on `com.bftools.core` (Logger, EventBus, ServiceLocator). Editor tooling additionally depends on `BFTools.Core.EditorAssetUtility.Editor`.