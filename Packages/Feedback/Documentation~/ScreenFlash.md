# Screen Flash

Full-screen color flash triggered by named events, driven by config-defined color/duration/count entries.

## Setup
1. `Assets/Create/BFTools/Feedback/Config/Screen Flash Config` creates a config at `Assets/Configs/Feedback/ScreenFlash/ScreenFlashConfig.asset`.
2. Populate `Entries` with `eventName` / `flashColor` / `duration` (seconds) / `flashCount` rows.
3. `Assets/Create/BFTools/Feedback/Prefabs/Screen Flash` creates a prefab variant of the base `ScreenFlash` prefab at `Assets/Prefabs/Feedback/ScreenFlash.prefab`.
4. Assign one or more `ScreenFlashConfig` assets to the variant's `BFScreenFlash` component (`configs` list), e.g. split by category (`UI_ScreenFlash`, `Combat_ScreenFlash`, `Environment_ScreenFlash`) instead of cramming everything into one asset.
5. Place the prefab instance in the scene (or wire it into a Bootstrapper). The base prefab is a Screen Space - Overlay Canvas with a full-screen `Image` child (`flashImage`) that starts fully transparent.

Or run `BF Tools/New Project Setup` (in `com.bftools.editortools`): it creates the config and prefab, seeds the config with a single `"Default"` entry (`flashColor` white, `duration` 0.15, `flashCount` 1), and assigns the config to the prefab's `BFScreenFlash` component. It skips step 5; instead it contributes the prefab to the Global Bootstrap Config's `System Prefabs` array, so Global Bootstrapper instantiates it automatically if that's set up too. Adjust or add to the seeded entry before relying on it for real events.

## Usage
Fire a screen flash event from anywhere:
```csharp
EventBus<BFScreenFlashEvent>.Fire(new BFScreenFlashEvent { eventName = "Hit" });
```
`eventName` must match an entry in one of the assigned `configs`.

## How it works
- `BFScreenFlash` subscribes to `EventBus<BFScreenFlashEvent>` in `OnEnable`, unsubscribes in `OnDisable`.
- On `OnEnable`, entries from every config in `configs` are merged into a single runtime lookup, keyed by `eventName`, via the shared `BFConfigLookupBuilder.Merge` utility (`com.bftools.core`). If `flashImage` isn't assigned, a warning is logged.
- If two configs in the list define the same `eventName`, the later config in the list wins and a warning is logged.
- On event, looks up `eventName` in the merged lookup; no match, no configs, or no `flashImage` means no-op.
- Cancels any in-progress flash coroutine first (flashes don't stack), then starts a new one.
- `FlashRoutine` runs `flashCount` passes: each pass linearly fades `flashImage.color` alpha from ~1 down to 0 over `duration` seconds, using `entry.flashColor` for RGB. After the final pass, alpha is forced to `0`.
- Cancelling an active flash (a new trigger or the component disabling) resets `flashImage`'s alpha to `0` immediately rather than leaving it stuck mid-fade.

## Notes
- Uses `Time.deltaTime` (scaled), consistent with Screen Shake. A concurrent hitstop (`Time.timeScale` at `0`) will pause the flash's fade along with everything else.
- The merged lookup is built once in `OnEnable`. Changing `configs` at runtime (e.g. via script) while the component is already enabled won't take effect until it's re-enabled.
- `flashImage` must reference a `UnityEngine.UI.Image` (not `RawImage`), the base prefab's flash target is an `Image` component sized to fill the screen.
- Depends on `com.bftools.core` (EventBus, Logger, ConfigLookup).