# Changelog

## [0.3.0] - Gradient Layer Expansion

### Added
- Multi-key color support for the Gradient layer via a full `Gradient`, replacing the fixed top/bottom `Color` pair
- Layout controls for the Gradient layer: Midpoint and Spread, tuning where the color blend sits along the axis and how gradual it is
- Angle control, rotating the Gradient layer's axis
- Drift animation (Shift Speed, Shift Amplitude), moving the gradient back and forth along its axis over time
- Rotation animation (Rotation Speed, Rotation Oscillation Speed, Rotation Oscillation Amplitude)
- Wave shape and animation controls (Wave Amplitude, Wave Frequency, Wave Frequency Oscillation Speed/Amplitude, Wave Amplitude Randomness, Wave Amplitude Randomness Speed), displacing the gradient's transition line into an animated wave
- Tooltips on every BFGradientLayerConfig field

### Changed
- BFGradientLayerConfig groups its fields into labeled inspector sections (Color, Layout, Drift Animation, Rotation Animation, Wave Shape, Wave Animation) via Header attributes
- Replaced BFGradientLayerConfig's TopColor/BottomColor properties with ColorGradient (breaking change for existing Gradient layer configs and any code referencing the old properties)

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