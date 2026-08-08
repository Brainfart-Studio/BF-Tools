# Hitstop

Brief global time freeze triggered by named events, driven by config-defined timescale/duration entries.

## Setup
1. `Assets/Create/BFTools/Feedback/Config/Hitstop Config` creates a config at `Assets/Configs/Feedback/Hitstop/HitstopConfig.asset`.
2. Populate `Entries` with `eventName` / `timescale` (0–1) / `duration` (seconds) rows.
3. `Assets/Create/BFTools/Feedback/Prefabs/Hitstop` creates a prefab variant of the base `Hitstop` prefab at `Assets/Prefabs/Feedback/Hitstop.prefab`.
4. Assign one or more `HitstopConfig` assets to the variant's `BFHitstop` component (`configs` list) — e.g. split by category (`UI_Hitstop`, `Combat_Hitstop`, `Environment_Hitstop`) instead of cramming everything into one asset.
5. Place the prefab instance in the scene (or wire it into a Bootstrapper).

Or run `BF Tools/New Project Setup` (in `com.bftools.editortools`): it creates the config and prefab, seeds the config with a single `"Default"` entry (`timescale` 0.05, `duration` 0.15), and assigns the config to the prefab's `BFHitstop` component. It skips step 5 — instead it contributes the prefab to the Global Bootstrap Config's `System Prefabs` array, so Global Bootstrapper instantiates it automatically if that's set up too. Adjust or add to the seeded entry before relying on it for real events.

## Usage
Fire a hitstop event from anywhere:
```csharp
EventBus<BFHitstopEvent>.Fire(new BFHitstopEvent { eventName = "Hit" });
```
`eventName` must match an entry in one of the assigned `configs`.

## How it works
- `BFHitstop` subscribes to `EventBus<BFHitstopEvent>` in `OnEnable`, unsubscribes in `OnDisable`.
- On `OnEnable`, entries from every config in `configs` are merged into a single runtime lookup, keyed by `eventName`.
- If two configs in the list define the same `eventName`, the later config in the list wins and a warning is logged.
- On event, looks up `eventName` in the merged lookup; no match or no configs means no-op.
- `Trigger` sets `Time.timeScale` directly to the entry's `timescale`, stopping any in-progress restore coroutine first (hitstops don't stack).
- After `duration` seconds (`WaitForSecondsRealtime`, unaffected by the timescale change itself), `Time.timeScale` is restored to `1f`.

## Notes
- `Time.timeScale` is a global value. If another system has already changed it (e.g. a pause menu) when a hitstop fires, the restore will reset it to `1f` rather than whatever it was before the hitstop.
- The merged lookup is built once in `OnEnable`. Changing `configs` at runtime (e.g. via script) while the component is already enabled won't take effect until it's re-enabled.
- Depends on `com.bftools.core` (EventBus, Logger).