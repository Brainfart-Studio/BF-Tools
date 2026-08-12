# Controller LED

Gamepad LED/light bar color triggered by named events or set directly, driven by config-defined color entries. Currently supports PS4 (DualShock 4) over HID; other pads fall back to a no-op.

## Setup
1. `Assets/Create/BFTools/Feedback/Config/Controller LED Config` creates a config at `Assets/Configs/Feedback/ControllerLED/ControllerLedConfig.asset`.
2. Populate `Entries` with `eventName` / `color` rows.
3. Add a `BFControllerLedManager` component to a GameObject in the scene (or wire it into a Bootstrapper).
4. Assign one or more `BFControllerLedConfig` assets to the `configs` list, e.g. split by category (`UI_LED`, `Combat_LED`) instead of one asset for everything.

## Usage
Fire a named event from anywhere:
```csharp
EventBus<BFControllerLedEvent>.Fire(new BFControllerLedEvent { eventName = "LowHealth" });
```
`eventName` must match an entry in one of the assigned `configs`.

For colors computed at runtime (an averaged ambient color, a gradient over time, anything that isn't a fixed named state), call the manager directly instead of going through a config:
```csharp
ledManager.SetColor(computedColor);
```
Both paths end at the same place, `SetColor` is the underlying API; the event/config layer is a convenience wrapper around it.

## How it works
- `BFControllerLedManager` subscribes to `EventBus<BFControllerLedEvent>` in `OnEnable`, unsubscribes in `OnDisable`.
- On `OnEnable`, entries from every config in `configs` are merged into a single runtime lookup, keyed by `eventName`, via the shared `BFConfigLookupBuilder.Merge` utility (`com.bftools.core`).
- If two configs in the list define the same `eventName`, the later config in the list wins and a warning is logged.
- On event, looks up `eventName` in the merged lookup; no match logs a trace and no-ops.
- The manager listens for `InputSystem.onDeviceChange` and resolves an `IBFControllerLed` implementation for `Gamepad.current` whenever a device connects or disconnects.
- `DualShock4GamepadHID` resolves to `BFDualShockLed`, which calls `SetLightBarColor` on the device. Anything else (including no gamepad) resolves to `BFNoOpLed`, which silently does nothing.

## Rainbow fallback
`BFControllerLedRainbow` is a drop-in component for projects that don't have specific color logic yet but still want the LED doing something rather than sitting off. Add it to any GameObject, assign a `BFControllerLedManager` reference, and it calls `SetColor` every frame with a color cycled through HSV hue:
```csharp
hue = (hue + cyclesPerSecond * Time.deltaTime) % 1f;
ledManager.SetColor(Color.HSVToRGB(hue, 1f, 1f));
```
`cyclesPerSecond` controls how fast it loops (0.2 = one full loop every 5 seconds). It talks to the manager through the same public `SetColor` API as everything else, so it can be swapped out for real color logic later without touching anything downstream.

## Notes
- PS4 support only. PS5 (`DualSenseGamepadHID`) and Xbox aren't implemented; add another `IBFControllerLed` implementation and a branch in `BFControllerLedManager.ResolveLed` to extend.
- Most Xbox controllers don't expose a public per-title LED API at all, there's likely nothing to wrap there.
- DualShock LED support is only compiled under `UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_WSA` (Unity's own guard on `DualShock4GamepadHID`). `UNITY_STANDALONE_LINUX` is not in that list, a Linux standalone build falls back to `BFNoOpLed` even with a DualShock 4 connected.
- Console builds (an actual PS4/PS5 dev kit) would need the platform SDK's own lightbar API, not the Input System's HID path. `DualShock4GamepadHID` only covers a PS4 controller connected to a PC over USB/Bluetooth.
- The merged lookup is built once in `OnEnable`. Changing `configs` at runtime while the component is already enabled won't take effect until it's re-enabled.
- `BFControllerLedRainbow` and `BFControllerLedManager` are independent components; nothing stops both being active on the same object, but the rainbow will fight anything else calling `SetColor` in the same frame since there's no arbitration between callers.
- Depends on `com.bftools.core` (EventBus, ConfigLookup, Logger) and `com.unity.inputsystem`.