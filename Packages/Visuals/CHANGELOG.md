# Changelog

## [0.2.0] - Background

### Added
- Background package, a layered background rendering system (Stack Manager, Stack Config, per-layer configs)
- BFBackgroundStackManager, enforcing a single active instance and owning a dedicated background camera
- BFBackgroundStackCamera, compositing the background onto the scene's output camera via a reserved rendering layer
- BFBackgroundStack and the BFBackgroundLayerConfig/IBFBackgroundLayer factory pattern for per-layer instantiation
- Three layer types (Gradient, Aurora Ribbons, Twinkling Stars)
- BFBackgroundStackManagerVariantCreator and per-config Editor menu creators, wired through BFMenuPriority
- BFLogger tracing throughout, tagged "Background" plus a per-layer tag on each concrete layer

### Changed
- Replaced an earlier monolithic single-effect background prototype with the layered stack system
- Renamed the Twinkling Stars layer folder for naming consistency with Aurora Ribbons
- Background config ScriptableObjects now use prioritized Editor menu creators instead of raw CreateAssetMenu attributes

### Fixed
- Guarded BFAuroraRibbon against an empty Ribbon Colors list, previously a divide-by-zero crash
- BFBackgroundStackCamera now re-resolves the output camera if it's destroyed after being configured
- BFBackgroundStackManager's static instance now resets on every Play session, not just after a domain reload

## [0.1.0] - Palette

### Added
- Visuals package.json
- BFPaletteConfig ScriptableObject with name/color entries
- BFPalette MonoBehaviour applying a named color entry to a SpriteRenderer
- BFPaletteEditor custom inspector with a Selected Entry dropdown
- BFPaletteConfigCreator editor tool
- Palette runtime/editor asmdefs
- Warning logged on duplicate entry names in BFPaletteConfig

### Fixed
- BFPalette now seeds its runtime selection from the serialized Selected Entry field on enable, instead of only tracking selections made through Select
- Added ExecuteAlways to BFPalette so it subscribes to config change events in Edit Mode, not just Play Mode
- BFPaletteEditor's entry dropdown now calls Select instead of Apply, so a manual Inspector choice stays live for future config edits