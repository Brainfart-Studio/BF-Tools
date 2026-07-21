# Haptics

Controller rumble/vibration triggered by named events, driven by config-defined intensity/duration entries.

## Setup
1. `Assets/Create/BFTools/Config/Haptics Config` creates a config at `Assets/Configs/Feedback/Haptics/HapticsConfig.asset`.
2. Populate `Entries` with `eventName` / `intensity` (0–1) / `duration` (seconds) rows.
3. `Assets/Create/BFTools/Prefabs/Haptics` creates a prefab variant of the base `Haptics` prefab at `Assets/Prefabs/Feedback/Haptics.prefab`.
4. Assign one or more `HapticsConfig` assets to the variant's `BFHaptics` component (`configs` list) — e.g. split by category (`UI_Haptics`, `Combat_Haptics`, `Environment_Haptics`) instead of cramming everything into one asset.
5. Place the prefab instance in the scene (or wire it into a Bootstrapper).

## Usage
Fire a haptics event from anywhere:
```csharp
EventBus<BFHapticsEvent>.Fire(new BFHapticsEvent { eventName = "Hit" });
```
`eventName` must match an entry in one of the assigned `configs`.

## How it works
- `BFHaptics` subscribes to `EventBus<BFHapticsEvent>` in `OnEnable`, unsubscribes in `OnDisable`.
- On `OnEnable`, entries from every config in `configs` are merged into a single runtime lookup, keyed by `eventName`.
- If two configs in the list define the same `eventName`, the later config in the list wins and a warning is logged.
- On event, looks up `eventName` in the merged lookup; no match or no configs means no-op.
- Triggers rumble via `Gamepad.current.SetMotorSpeeds(intensity, intensity)`, both motors set to the same value (single-intensity, not per-motor).
- Stops motors after `duration` seconds via coroutine (`WaitForSeconds`, not scaled/unscaled-time aware).

## Notes
- Requires `Gamepad.current`. No fallback for keyboard/mouse-only input or multiple simultaneous gamepads.
- Dual-motor (independent low/high frequency) support is deferred. Currently both motors always match.
- The merged lookup is built once in `OnEnable`. Changing `configs` at runtime (e.g. via script) while the component is already enabled won't take effect until it's re-enabled.
- Depends on `com.bftools.core` (EventBus) and `unity.inputsystem`.