# Changelog

## [0.6.1] - Test Coverage

### Added
- Edit Mode test suites for Background (Aurora Ribbon, Aurora Ribbons Layer, Background Stack, Background Stack Manager, Gradient Layer, Twinkling Star, Twinkling Stars Layer), Palette (Component, Config), and Parallax (Sprite Layer, Stack, Stack Manager)
- BFTools.Visuals.Background runtime and editor asmdefs, splitting Background out of the default assembly so it can be referenced by its own test assembly
- BFTools.Visuals.Background.Tests, BFTools.Visuals.Palette.Tests, and BFTools.Visuals.Parallax.Tests asmdefs

### Fixed
- BFBackgroundStackCamera, BFAuroraRibbonsLayer, BFGradientLayer, BFTwinklingStarsLayer, and BFParallaxSpriteLayer now call DestroyImmediate instead of Destroy when not in Play mode, preventing leaked GameObjects and assets when Cleanup runs in Edit mode

## [0.6.0] - Parallax

### Added
- Parallax package, a layered parallax scrolling system (Stack Manager, Stack Config, per-layer configs)
- BFParallaxStackManager, enforcing a single active instance and tracking camera displacement via BFParallaxCameraTracker
- BFParallaxStack and the BFParallaxLayerConfig/IBFParallaxLayer factory pattern for per-layer instantiation
- BFParallaxSpriteLayer, with independent horizontal and vertical parallax factor, auto-scroll speed, looping with tile size override, and BFParallaxAxisLock for one-way movement
- BFParallaxStackConfigCreator, BFParallaxSpriteLayerConfigCreator, and BFParallaxStackManagerVariantCreator editor menu tools
- BFTools.Visuals.Parallax runtime and editor asmdefs
- Module.Parallax constant added to BFMenuPriority (Core)
- BFLogger tracing throughout, tagged "Parallax" plus a "Sprite" tag on the Sprite layer

### Changed
- Renamed BFParallaxConfig.cs to BFParallaxLayerConfig.cs for naming consistency with its class
- Moved the Sprite layer's config and layer files into a Layers/Sprite subfolder, matching Background's per-layer-type folder convention

## [0.5.0] - Aurora Ribbons Overhaul

### Added
- Overall Opacity control for the Aurora Ribbons layer, fading every ribbon at once on top of each color's own alpha
- Ribbon Spacing, controlling the vertical distance between ribbon baselines independent of thickness, replacing the fixed 0.25-0.75 screen band
- Angle control, rotating the whole ribbon band
- Rotation animation (Rotation Speed, Rotation Oscillation Speed, Rotation Oscillation Amplitude)
- Thickness variance range (Min/Max Thickness Variance), replacing the fixed 0.8-1.3 random multiplier
- Amplitude variance range (Min/Max Amplitude Variance), replacing the fixed 0.7-1.2 random multiplier
- Wave Frequency control plus a variance range (Min/Max Frequency Variance), replacing a fixed random multiplier and hardcoded frequency constants
- Secondary Wave Strength and Secondary Wave Speed Scale, exposing the ribbon's second, smaller wave layer
- Wave Speed variance range (Min/Max Speed Variance), replacing the fixed 0.7-1.3 random multiplier
- Tooltips on every BFAuroraRibbonsLayerConfig field

### Changed
- BFAuroraRibbonsLayerConfig groups its fields into labeled inspector sections (Color, Layout, Thickness, Wave Shape, Wave Animation, Rotation Animation, Glow) via Header attributes
- Thickness now ranges up to 100 instead of 10
- Ribbons now span the screen diagonal instead of the screen width, keeping their ends hidden off-screen at any rotation angle instead of only at 0°

### Fixed
- Ribbon count, spacing, color, thickness, and every variance range now refresh live in Play mode when the config changes, instead of only taking effect on the next Init

## [0.4.0] - Twinkling Stars Overhaul

### Added
- Color Gradient control for the Twinkling Stars layer, tinting each star by sampling a full `Gradient` ramp at a random point per star (previously hardcoded white)
- Size distribution controls (Min Size, Max Size, Average Size, Size Outliers), replacing the fixed 0.6-1.8 random range
- Brightness distribution controls (Min Brightness, Max Brightness, Average Brightness, Brightness Outliers), replacing the fixed 0.3-0.9 random range
- Twinkle speed and depth ranges (Min/Max Twinkle Speed, Min/Max Twinkle Depth), varying each star's twinkle rate and dip instead of one shared formula
- Tooltips on every BFTwinklingStarsLayerConfig field

### Changed
- BFTwinklingStarsLayerConfig groups its fields into labeled inspector sections (Color, Layout, Size, Brightness, Twinkle) via Header attributes
- Twinkling Stars now renders as a single dynamic mesh with a baked color-ramp texture instead of one SpriteRenderer GameObject per star
- Star size, brightness, and twinkle speed/depth are now derived from Perlin noise sampled at each star's screen position, giving spatially coherent variation instead of independent per-star randomness

### Fixed
- Star size, brightness, color, count, and twinkle timing now refresh live in Play mode when the config changes, instead of only taking effect on the next Init
- Twinkle phase now scales by delta time again, fixing twinkle speed that varied with frame rate instead of staying constant across frame rates

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