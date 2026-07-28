# Changelog

## [0.4.1] - Logger Integration

### Added
- BFLogger tracing added to `BFHaptics` and `BFScreenShake`: lookup building, duplicate-`eventName` warnings, no-match trace logs, and trigger calls

### Changed
- Bumped `com.bftools.core` dependency to 0.4.1, required for the Logger assembly reference

## [0.4.0] - Multi-Config Support

### Changed
- `BFHaptics` and `BFScreenShake` now accept a list of config assets (`configs`) instead of a single config reference, so entries can be split across multiple assets (e.g. by category) and merged at runtime.
- On duplicate `eventName` across configs in the same list, the later config wins and a warning is logged.
- **Breaking:** the `config` field on `BFHaptics` and `BFScreenShake` has been replaced by `configs`. Existing prefab variants need their config(s) re-assigned to the new list field.

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