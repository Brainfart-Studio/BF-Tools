# Changelog

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