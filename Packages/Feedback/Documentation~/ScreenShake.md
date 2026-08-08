# Screen Shake

Camera shake triggered by named events, driven by config-defined amplitude/duration entries.

## Setup
1. `Assets/Create/BFTools/Feedback/Config/Screen Shake Config` creates a config at `Assets/Configs/Feedback/ScreenShake/ScreenShakeConfig.asset`.
2. Populate `Entries` with `eventName` / `amplitude` (0–1) / `duration` (seconds) rows.
3. `Assets/Create/BFTools/Feedback/Prefabs/Screen Shake` creates a prefab variant of the base `ScreenShake` prefab at `Assets/Prefabs/Feedback/ScreenShake.prefab`.
4. Assign one or more `ScreenShakeConfig` assets to the variant's `BFScreenShake` component (`configs` list) — e.g. split by category (`UI_ScreenShake`, `Combat_ScreenShake`, `Environment_ScreenShake`) instead of cramming everything into one asset.
5. Place the prefab instance in the scene (or wire it into a Bootstrapper).

Or run `BF Tools/New Project Setup` (in `com.bftools.editortools`): it creates the config and prefab, seeds the config with a single `"Default"` entry (`amplitude` 0.3, `duration` 0.2), and assigns the config to the prefab's `BFScreenShake` component. It skips step 5 — instead it contributes the prefab to the Global Bootstrap Config's `System Prefabs` array, so Global Bootstrapper instantiates it automatically if that's set up too. Adjust or add to the seeded entry before relying on it for real events.

## Usage
Fire a screen shake event from anywhere:
```csharp
EventBus<BFScreenShakeEvent>.Fire(new BFScreenShakeEvent { eventName = "Explosion" });
```
`eventName` must match an entry in one of the assigned `configs`.

## How it works
- `BFScreenShake` subscribes to `EventBus<BFScreenShakeEvent>` in `OnEnable`, unsubscribes in `OnDisable`.
- On `OnEnable`, entries from every config in `configs` are merged into a single runtime lookup, keyed by `eventName`.
- If two configs in the list define the same `eventName`, the later config in the list wins and a warning is logged.
- Target is resolved as `Camera.main.transform`, re-resolved on every `SceneManager.sceneLoaded`.
- On event, looks up `eventName` in the merged lookup; no match, no configs, or no target means no-op.
- `Trigger` stops any active shake coroutine before starting a new one (shakes don't stack).
- `ShakeRoutine` offsets `target.localPosition` from its original position each frame using random X/Y within `[-amplitude, amplitude]`, for `duration` seconds (`Time.deltaTime`-based), then restores the original position.

## Notes
- `originalPosition` is captured at the start of each shake. If the camera moves during a shake (e.g. follows a player), the restore position may be stale relative to camera's intended position at shake end.
- No Z-axis shake (2D X/Y offset only).
- The merged lookup is built once in `OnEnable`. Changing `configs` at runtime (e.g. via script) while the component is already enabled won't take effect until it's re-enabled.
- Depends on `com.bftools.core` (EventBus).