# Changelog

## [1.5.0] - Sprite Flash

### Added
- Scaffold Sprite Flash system in Feedback package: `BFSpriteFlashConfig` ScriptableObject, `BFSpriteFlash` runtime component
- `BFSpriteFlash` attaches directly to any GameObject with a `SpriteRenderer` (`[RequireComponent]`), toggling `enabled` on/off for a config-defined interval/count rather than a global overlay
- SpriteFlashConfig Editor creator
- Test suite: `BFSpriteFlashConfigTests` (EditMode), `BFSpriteFlashComponentTests` (PlayMode)
- Sprite Flash documentation (`Documentation~/SpriteFlash.md`)

### Changed
- Added a `SpriteFlash` entry to `BFFeedbackMenuPriority`

## [1.4.0] - SloMo

### Added
- Scaffold SloMo system in Feedback package: `BFSloMoConfig` ScriptableObject, `BFSloMo` runtime component, base SloMo prefab
- SloMoConfig and SloMo prefab variant Editor creators
- Test suite: `BFSloMoConfigTests` (EditMode), `BFSloMoComponentTests` (PlayMode)
- SloMo documentation (`Documentation~/SloMo.md`)

### Changed
- Added a `SloMo` entry to `BFFeedbackMenuPriority`

## [1.3.0] - Vignette

### Added
- Scaffold Vignette system in Feedback package: `BFVignetteConfig` ScriptableObject, `BFVignette` runtime component, `BFVignetteTextureBaker`, base Vignette prefab
- Angular wave ripple profile on the vignette mask edge (`frequency`, `waveCrest`/`waveTrough`, `spacingVariance`, `waveHeightVariance`, `jaggedness`); a new random seed is picked per trigger, so the same config produces a different-looking wave shape each time it fires
- Gradient color support (`useGradient` / `colorGradient`), sampled across the full mask radius
- Custom Inspector for `BFVignetteConfig` so new entries added via the list's `+` button get real default values instead of all-zero fields
- Layer pool (`vignetteImages`) on `BFVignette` so multiple vignettes can play/layer simultaneously; when every layer is busy, the longest-running one is reused instead of dropping the trigger
- VignetteConfig and Vignette prefab variant Editor creators
- Test suite: `BFVignetteConfigTests`, `BFVignetteTextureBakerTests` (EditMode), `BFVignetteComponentTests` (PlayMode)
- Vignette documentation (`Documentation~/Vignette.md`)

### Changed
- `BFVignetteEntry` changed from a struct to a class so its fields can carry real defaults
- Vignette wave-shape fields renamed from amplitude-based naming (`amplitude`, `amplitudeVariance`) to `waveCrest`/`waveTrough`/`waveHeightVariance`
- Vignette mask `radius` and `softness` constrained to 0–1

### Fixed
- Vignette gradient now samples across the full mask radius instead of just the softness band
- Vignette routine re-reads live config values every frame instead of only at trigger time
- Resolved an ambiguous `Image` type reference in `BFVignette`

## [1.2.0] - Controller LED Color Feeder

### Added
- `BFControllerLedColorFeeder` runtime component: subscribes to a `BFScreenColorSampler` reference's `ColorSampled` event and forwards each sampled color straight to a `BFControllerLedManager` reference's `SetColor`, independent of the named-event/config path
- Test suite covering forwarding and unsubscribe-on-disable behavior

### Changed
- Bumped `com.bftools.core` dependency to 1.1.0, required for the new `BFScreenColorSampler`

## [1.1.0] - Controller LED

### Added
- Scaffold Controller LED system in Feedback package
- `IBFControllerLed` interface with `BFNoOpLed` fallback
- `BFDualShockLed` implementation (PS4, `DualShock4GamepadHID` over HID; compiled only under `UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_WSA`, matching Unity's own guard on that type)
- `BFControllerLedConfig` ScriptableObject (`eventName` / `color` entries)
- `BFControllerLedManager` runtime component: direct `SetColor(Color)` / `TurnOff()` API, a list of config assets (`configs`) merged at runtime consistent with Haptics/ScreenShake/Hitstop/ScreenFlash, config-driven `BFControllerLedEvent` via EventBus, and automatic LED implementation resolution on `InputSystem.onDeviceChange`
- `BFControllerLedRainbow` fallback effect component, cycles LED color through HSV hue over time given a `BFControllerLedManager` reference, for projects that just want the LED to do something without wiring specific colors
- ControllerLedConfig Editor creator
- Test suite (Config, Manager)

### Changed
- Added a `ControllerLED` entry to `BFFeedbackMenuPriority`

## [1.0.0] - Production Release

### Changed
- Bumped `com.bftools.core` dependency to 1.0.0

### Fixed
- `BFScreenShake.Trigger` now restores the camera to its original position before starting a new shake when re-triggered mid-shake, instead of treating the current offset position as the new baseline; repeated re-triggers (combos, chained explosions) no longer leave the camera drifted from its true resting position

## [0.7.0] - Project Setup Steps

### Added
- `BFHitstopSetupStep`, `BFScreenShakeSetupStep`, `BFScreenFlashSetupStep`, and `BFHapticsSetupStep`, each implementing `IBFProjectSetupStep` and `IBFSystemPrefabContributor` (from `com.bftools.core`'s new `BFTools.Core.ProjectSetup.Editor` assembly), so `BF Tools/New Project Setup` (in `com.bftools.editortools`) creates and wires each module's prefab + config automatically, without EditorTools needing a hardcoded reference to Feedback
- Each setup step seeds its config with a single `"Default"` entry using the same values `New Project Setup` previously hardcoded (Hitstop: `timescale` 0.05, `duration` 0.15; Screen Shake: `amplitude` 0.3, `duration` 0.2; Screen Flash: `flashColor` white, `duration` 0.15, `flashCount` 1; Haptics: `intensity` 0.5, `duration` 0.2), assigns the config to the prefab's component, and reports the prefab so it can be added to the Global Bootstrap Config's `System Prefabs` array
- The existing standalone Config/Prefab `[MenuItem]` creators for all four modules are unchanged and still work independently

### Changed
- Bumped `com.bftools.core` dependency to 0.11.0, required for the new `IBFProjectSetupStep` and `IBFSystemPrefabContributor` interfaces

### Fixed
- Declared `com.unity.inputsystem` (1.7.0) as a `package.json` dependency — `BFHaptics` has referenced `Unity.InputSystem` in its asmdef since Haptics was scaffolded, but the dependency was never declared, so installing `com.bftools.feedback` standalone without Input System already present would fail to compile

## [0.6.2] - Menu Priority Ownership

### Changed
- Haptics, Hitstop, Screen Flash, and Screen Shake Editor menu creators now reference a new `BFFeedbackMenuPriority` class instead of Core's `BFMenuPriority.Module` constants, so new Feedback Create menu entries no longer require a Core version bump
- Added the `BFTools.Feedback.EditorAssetUtility.Editor` asmdef, housing `BFFeedbackMenuPriority`

## [0.6.1] - Test Coverage

### Added
- Test suites for Haptics (Component, Config), Hitstop (Component, Config), Screen Flash (Component, Config), and Screen Shake (Component, Config)
- BFTools.Feedback.Haptics.Tests, .Hitstop.Tests, .Hitstop.PlayModeTests, .ScreenFlash.Tests, .ScreenFlash.PlayModeTests, .ScreenShake.Tests, and .ScreenShake.PlayModeTests asmdefs

### Changed
- Bumped `com.bftools.core` dependency to 0.10.0, required for the new `BFTools.Core.Logger.TestUtilities` assembly the test suites reference

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