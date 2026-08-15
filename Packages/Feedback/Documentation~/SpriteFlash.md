# Sprite Flash

Toggles a `SpriteRenderer` on and off for a set number of cycles, triggered by named events and driven by config-defined interval/count entries.

## Setup
1. `Assets/Create/BFTools/Feedback/Config/Sprite Flash Config` creates a config at `Assets/Configs/Feedback/SpriteFlash/SpriteFlashConfig.asset`.
2. Populate `Entries` with `eventName`, `interval` (seconds each on/off state lasts), and `flashCount` (number of on/off cycles) rows.
3. Add a `BFSpriteFlash` component to any GameObject with a `SpriteRenderer` (the component requires one on the same object, matching `BFPalette`'s attach pattern rather than `BFScreenFlash`'s single global overlay).
4. Assign one or more `SpriteFlashConfig` assets to the component's `configs` list.

There's no prefab variant or project setup step for this system. Each actor that needs a flash gets its own `BFSpriteFlash` component and its own assigned config, the way `BFPalette` works.

## Usage
Fire a sprite flash event from anywhere:
```csharp
EventBus<BFSpriteFlashEvent>.Fire(new BFSpriteFlashEvent { eventName = "Hit" });
```
`eventName` must match an entry in one of the assigned `configs`.

Because the event has no target field, every `BFSpriteFlash` instance whose config contains that `eventName` will flash, not just one actor. Scope who reacts by which configs and entries are assigned per instance. A player takes damage on `"PlayerHit"` because only the player's component has that entry, not because the event is addressed to it.

## How it works
- `BFSpriteFlash` subscribes to `EventBus<BFSpriteFlashEvent>` in `OnEnable`, unsubscribes in `OnDisable`.
- On `OnEnable`, entries from every config in `configs` are merged into a single runtime lookup, keyed by `eventName`, via the shared `BFConfigLookupBuilder.Merge` utility (`com.bftools.core`).
- If two configs in the list define the same `eventName`, the later config in the list wins and a warning is logged.
- On event, looks up `eventName` in the merged lookup; no match or no configs means no-op.
- Cancels any in-progress flash coroutine first (flashes don't stack), then starts a new one.
- `FlashRoutine` runs `flashCount` cycles, each cycle disabling the renderer for `interval` seconds then re-enabling it for `interval` seconds. After the last cycle, the renderer is left enabled.
- Cancelling an active flash (a new trigger or the component disabling) re-enables the renderer immediately rather than leaving it stuck off.

## Notes
- Uses `WaitForSeconds` (scaled), consistent with Screen Shake and Screen Flash. A concurrent hitstop (`Time.timeScale` at `0`) will pause the flash cycle along with everything else.
- The merged lookup is built once in `OnEnable`. Changing `configs` at runtime while the component is already enabled won't take effect until it's re-enabled.
- This is the enable/disable toggle version. A shader-based version (material swap, tint flash, etc.) is a future option if the on/off blink isn't enough for a given project.
- Depends on `com.bftools.core` (EventBus, Logger, ConfigLookup).