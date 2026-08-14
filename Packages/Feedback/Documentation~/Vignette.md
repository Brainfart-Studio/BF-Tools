# Vignette

Full-screen radial mask, tinted and faded in/out over time, triggered by named events. Supports a wavy (non-circular) edge and multiple simultaneous/layered triggers.

## Setup
1. `Assets/Create/BFTools/Feedback/Config/Vignette Config` creates a config at `Assets/Configs/Feedback/Vignette/VignetteConfig.asset`.
2. Add entries via the `+` button in the config's Inspector (a custom Inspector backs this so new entries start with usable defaults instead of all-zero values). Group fields per entry.
   - **Trigger.** `eventName`.
   - **Appearance.** `intensity` (0–1 peak alpha), `color`, `useGradient` / `colorGradient`, `blendMode` (only `AlphaBlend` is implemented; `Multiply` logs a warning and falls back to it).
   - **Mask Shape.** `radius` (0–1), `softness` (0–1 edge fade width), `roundness` (0–1, circular vs. rectangular silhouette).
   - **Wave Shape.** `frequency` (wave cycles around the edge), `waveCrest` / `waveTrough` (outer/inner bounds the wave oscillates between), `spacingVariance`, `waveHeightVariance`, `jaggedness` (all 0–1).
   - **Timing.** `duration` (seconds), `intensityCurve` (alpha over `duration`, evaluated 0→1).
3. `Assets/Create/BFTools/Feedback/Prefabs/Vignette` creates a prefab variant of the base `Vignette` prefab at `Assets/Prefabs/Feedback/Vignette.prefab`. The base prefab ships with 3 layered `Image` children under one `Canvas`.
4. Assign one or more `VignetteConfig` assets to the variant's `BFVignette` component (`configs` list).
5. Confirm `vignetteImages` on the component lists all the layer `Image`s you want available (defaults to the 3 from the base prefab). This is the cap on how many vignettes can be visible at once.
6. Place the prefab instance in the scene (or wire it into a Bootstrapper).

## Usage
Fire a vignette event from anywhere.
```csharp
EventBus<BFVignetteEvent>.Fire(new BFVignetteEvent { eventName = "LowHealth" });
```
`eventName` must match an entry in one of the assigned `configs`.

## How it works
- `BFVignette` subscribes to `EventBus<BFVignetteEvent>` in `OnEnable`, unsubscribes in `OnDisable`. Entries from every config in `configs` are merged into one runtime lookup keyed by `eventName` (via `BFConfigLookupBuilder.Merge`, `com.bftools.core`). A duplicate `eventName` across configs logs a warning and the later config wins.
- Each `Image` in `vignetteImages` backs one `BFVignetteLayer`, wrapped in a `LayerSlot` that tracks whether it's currently playing.
- On trigger, `BFVignette` picks the first free layer. If every layer is busy, it steals whichever layer has been playing the longest rather than dropping the trigger. A config with more `vignetteImages` slots supports more truly simultaneous/layered vignettes before any stealing happens.
- Each layer independently bakes a mask texture (`BFVignetteTextureBaker`) sized to the entry's `radius`/`softness`/`roundness` and the current screen aspect ratio, re-baking only when those inputs (or the wave profile) actually change.
- The wave profile oscillates the mask's radius between `waveCrest` and `waveTrough` around the edge, `frequency` times per revolution. `spacingVariance`/`waveHeightVariance`/`jaggedness` randomize peak spacing, peak height, and edge noise respectively. A new random seed is picked per trigger, so the same config produces a different-looking wave shape each time it fires (as long as those variance fields are above 0; at 0 they're disabled, not just "less random").
- Live config edits are re-read every frame while a vignette is playing, so tuning values in the Inspector during Play Mode updates the running vignette immediately.
- `intensity * intensityCurve.Evaluate(t / duration)` drives alpha each frame. On completion the layer's alpha is reset to 0.

## Notes
- `BFVignetteEntry` is a class, not a struct. New entries get non-zero default values, but it also means `config.Entries[i]` returns a live reference, not a copy.
- Only `AlphaBlend` is implemented for `blendMode`.
- If `BFVignette`'s GameObject is disabled and re-enabled after at least one vignette has played, the layer pool is rebuilt from scratch and any texture baked before the disable is never destroyed (`DestroyBakedAssets` only runs from `OnDestroy`). Not an issue for an object set up once and never toggled off, but it leaks a texture and sprite per baked layer on each enable/disable cycle. Fix before relying on repeated enable/disable.
- Depends on `com.bftools.core` (EventBus, ConfigLookup).