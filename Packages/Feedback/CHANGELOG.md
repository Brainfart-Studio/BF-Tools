# Changelog

## [0.6.0] - Screen Flash

### Added
- Scaffold Screen Flash system in Feedback package
- BFScreenFlashConfig ScriptableObject
- BFScreenFlash runtime component, accepting a list of config assets (`configs`) merged at runtime, consistent with Haptics/ScreenShake/Hitstop
- Base ScreenFlash prefab
- ScreenFlashConfig Editor creator
- ScreenFlash prefab variant creator

### Changed
- Bumped `com.bftools.core` dependency to 0.7.2, required for the `BFMenuPriority.Module.ScreenFlash` entry

### Fixed
- Base ScreenFlash prefab now has the `BFScreenFlash` component attached
- Base ScreenFlash prefab's flash target corrected from a `RawImage` to an `Image` component, matching the `flashImage` field type

## [0.5.0] - Hitstop

### Added
- Scaffold Hitstop system in Feedback package
- BFHitstopConfig ScriptableObject
- BFHitstop runtime component
- Base Hitstop prefab
- HitstopConfig Editor creator
- Hitstop prefab variant creator

### Changed
- Bumped `com.bftools.core` dependency to 0.7.1, required for the `BFMenuPriority.Module.Hitstop` entry

## [0.4.2] - Create Menu Reorganization

### Changed
- Haptics and ScreenShake creators' menu paths moved from `Assets/Create/BFTools/Config|Prefabs/...` to `Assets/Create/BFTools/Feedback/Config|Prefabs/...`
- Menu items now sort using priority values from Core's `BFMenuPriority`, placing BFTools above Unity's built-in Create menu categories
- Bumped `com.bftools.core` dependency to 0.6.1, required for `BFMenuPriority`

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