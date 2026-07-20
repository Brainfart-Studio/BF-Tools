# Haptics

Controller rumble/vibration triggered by named events, driven by config-defined intensity/duration entries.

## Setup
1. `Assets/Create/BFTools/Config/Haptics Config` creates config at `Assets/Configs/Feedback/Haptics/HapticsConfig.asset`.
2. Populate `Entries` with `eventName` / `intensity` (0–1) / `duration` (seconds) rows.
3. `Assets/Create/BFTools/Prefabs/Haptics` creates a prefab variant of the base `Haptics` prefab at `Assets/Prefabs/Feedback/Haptics.prefab`.
4. Assign the `HapticsConfig` to the variant's `BFHaptics` component.
5. Place the prefab instance in the scene (or wire it into a Bootstrapper).

## Usage
Fire a haptics event from anywhere:
```csharp
EventBus<BFHapticsEvent>.Fire(new BFHapticsEvent { eventName = "Hit" });
```
`eventName` must match an entry in the assigned `HapticsConfig`.

## How it works
- `BFHaptics` subscribes to `EventBus<BFHapticsEvent>` in `OnEnable`, unsubscribes in `OnDisable`.
- On event, looks up `eventName` via `config.TryGetEntry`; no match or no config means no-op.
- Triggers rumble via `Gamepad.current.SetMotorSpeeds(intensity, intensity)`, both motors set to the same value (single-intensity, not per-motor).
- Stops motors after `duration` seconds via coroutine (`WaitForSeconds`, not scaled/unscaled-time aware).

## Notes
- Requires `Gamepad.current`. No fallback for keyboard/mouse-only input or multiple simultaneous gamepads.
- Dual-motor (independent low/high frequency) support is deferred. Currently both motors always match.
- Depends on `com.bftools.core` (EventBus) and `unity.inputsystem`.