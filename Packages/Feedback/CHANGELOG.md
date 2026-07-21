# Changelog

## [0.3.0] - Editor Asset Utility

### Changed
- Migrated Haptics and ScreenShake Editor creators to Core's BFEditorAssetUtility
- Bumped com.bftools.core dependency to 0.3.0

## [0.2.0] - ScreenShake

### Added
- Scaffold ScreenShake system in Feedback package
- BFScreenShakeConfig ScriptableObject
- BFScreenShake runtime component
- Base ScreenShake prefab
- ScreenShakeConfig Editor creator
- ScreenShake prefab variant creator

### Fixed
- Resolve ScreenShake target dynamically for bootstrapper compatibility

## [0.1.0] - Haptics

### Added
- Scaffold Feedback package
- BFHapticsConfig ScriptableObject
- BFHaptics runtime component
- Base Haptics prefab
- HapticsConfig Editor creator
- Haptics prefab variant creator

### Changed
- Add dependency on com.bftools.core@0.2.0