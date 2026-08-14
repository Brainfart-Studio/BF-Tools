# SloMo

Sustained slow motion triggered by named events, eased in and out over a config-defined curve.

## Setup
1. `Assets/Create/BFTools/Feedback/Config/SloMo Config` creates a config at `Assets/Configs/Feedback/SloMo/SloMoConfig.asset`.
2. Populate `Entries` with `eventName` / `duration` (seconds) / `curve` (AnimationCurve) rows. The curve is evaluated directly against `Time.timeScale`, keyed in seconds from 0 to `duration`, so author it to start and end near 1 with a dip in the middle for a smooth ease in and out.
3. `Assets/Create/BFTools/Feedback/Prefabs/SloMo` creates a prefab variant of the base `SloMo` prefab at `Assets/Prefabs/Feedback/SloMo.prefab`.
4. Assign one or more `SloMoConfig` assets to the variant's `BFSloMo` component (`configs` list), split by category if needed, same as Hitstop.
5. Place the prefab instance in the scene (or wire it into a Bootstrapper).

Or run `BF Tools/New Project Setup` (in `com.bftools.editortools`). It creates the config and prefab and assigns the config to the prefab's `BFSloMo` component, then contributes the prefab to the Global Bootstrap Config's `System Prefabs` array so Global Bootstrapper instantiates it automatically if that's set up too. It skips step 5.

## Usage
Fire a slo-mo event from anywhere.
```csharp
EventBus<BFSloMoEvent>.Fire(new BFSloMoEvent { eventName = "Slow" });
```
`eventName` must match an entry in one of the assigned `configs`.

## How it works
- `BFSloMo` subscribes to `EventBus<BFSloMoEvent>` in `OnEnable`, unsubscribes in `OnDisable`.
- On `OnEnable`, entries from every config in `configs` are merged into a single runtime lookup, keyed by `eventName`, via the shared `BFConfigLookupBuilder.Merge` utility (`com.bftools.core`).
- If two configs in the list define the same `eventName`, the later config in the list wins and a warning is logged.
- On event, looks up `eventName` in the merged lookup, no match or no configs means no-op.
- `Trigger` starts a coroutine that samples the entry's `curve` every frame against elapsed unscaled time and writes the result straight to `Time.timeScale`, cancelling any in-progress run first (slo-mo effects don't stack).
- Elapsed time is tracked with `Time.unscaledDeltaTime` so the ramp itself isn't affected by the timescale change it's producing.
- Once elapsed time passes `duration`, `Time.timeScale` is set back to `1f` and the coroutine ends.
- Cancelling an active slo-mo (a new trigger or the component disabling) restores `Time.timeScale` to `1f` immediately rather than leaving the game stuck partway through the curve.

## Notes
- `Time.timeScale` is a global value. If another system has already changed it (a pause menu, for example) when a slo-mo fires, the restore will reset it to `1f` rather than whatever it was before.
- The merged lookup is built once in `OnEnable`. Changing `configs` at runtime while the component is already enabled won't take effect until it's re-enabled.
- Depends on `com.bftools.core` (EventBus, Logger, ConfigLookup).