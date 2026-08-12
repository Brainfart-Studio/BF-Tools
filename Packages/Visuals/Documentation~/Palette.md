# Palette

Live-editable color palette for rapid prototyping. Pick a named color per `SpriteRenderer` and repaint every object using that name the moment the palette config changes.

## Setup
1. `Assets/Create/BFTools/Visuals/Config/Palette Config` creates a config at `Assets/Configs/Visuals/Palette/PaletteConfig.asset`.
2. Populate `Entries` with `name` / `color` rows (e.g. `player`, `ground`, `sky`).
3. Add a `BFPalette` component to any `GameObject` with a `SpriteRenderer`.
4. Assign the `PaletteConfig` asset and pick an entry from the `Selected Entry` dropdown in the inspector.

## Usage
Editing an entry's color on the config asset repaints every `BFPalette` that has that entry selected and that config assigned. No Play Mode required.

Palette can also be driven at runtime, directly.
```csharp
palette.Select("ground");
```
It can also be driven via a broadcast event, matched by name against whichever config each listening `BFPalette` has assigned.
```csharp
EventBus<BFPaletteEvent>.Fire(new BFPaletteEvent { eventName = "ground" });
```

## How it works
- `BFPaletteConfig` is a `ScriptableObject` holding a flat list of `name`/`color` entries. Its `OnValidate` fires `BFPaletteConfigChangedEvent` on `EventBus<BFPaletteConfigChangedEvent>` any time it's edited, and logs a `Warning` if two entries share a name. The first match wins at lookup time; the rest are silently unreachable.
- `BFPalette` is `[ExecuteAlways]`, so it subscribes to the event bus in `OnEnable` in both Edit Mode and Play Mode. This is what makes color edits propagate without entering Play Mode.
- `Select(eventName)` records `eventName` as the component's current selection and applies it immediately. The inspector's `Selected Entry` dropdown calls `Select`, not `Apply`, so a manual choice stays "live." Future config edits know which entry to re-apply.
- `Apply(eventName)` does the actual lookup. It scans the assigned config's `Entries` for a matching `name` and sets `SpriteRenderer.color`. No match, including an unassigned selection, is a no-op with a `Trace` log.
- `OnPaletteConfigChanged` re-`Apply`s the current selection whenever the *assigned* config fires a change event; a `BFPalette` watching a different config asset ignores it.
- Only `SpriteRenderer` is supported as a paint target. `[RequireComponent(typeof(SpriteRenderer))]` enforces this.
- All logging in this module goes through `BFLogger` under a single shared `"Palette"` tag, covering both `BFPalette` and `BFPaletteConfig`. `BFPalette` logs `Info` on enable/disable (subscribing/unsubscribing from palette events), `Warning` when applying with no config assigned, and `Trace` on every apply attempt (match or no match found). `BFPaletteConfig` logs `Info` whenever it fires a config-changed event, and `Warning` on duplicate entry names.

## Notes
- One config per `BFPalette`, not a list. For names shared across scenes, reuse the same config asset rather than duplicating entries.
- Duplicate entry names within a config aren't blocked, only warned about in the console; rename rather than leaving both in place.
- Depends on `com.bftools.core` (EventBus, Logger).