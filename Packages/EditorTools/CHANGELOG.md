# Changelog

## [1.0.0] - Production Release

### Changed
- Bumped `com.bftools.core`, `com.bftools.systems`, and `com.bftools.feedback` dependencies to 1.0.0

## [0.1.0] - Project Setup and Project Verification

### Added
- Scaffold Editor Tools package
- `BF Tools > New Project Setup` menu command: discovers every `IBFProjectSetupStep` implementation in the project via `TypeCache`, runs each in `Order`, collects results from any `IBFSystemPrefabContributor` steps, and hands them to any `IBFSystemPrefabConsumer` steps — EditorTools holds no hardcoded reference to what any step does or where its assets live. Currently this wires up Logger Config (Core), Global Bootstrap Config (Systems), and prefab + config setup for Hitstop, Screen Shake, Screen Flash, and Haptics (Feedback), each seeding its config with a `"Default"` entry and contributing its prefab to the Global Bootstrap Config's `System Prefabs` array
- `BF Tools > New Project Verification` menu command: creates a `BFToolsTest` scene with a camera, 3 bouncing test balls, and 4 UI buttons wired to fire each Feedback event with `eventName = "Default"`, for visually confirming Hitstop, Screen Shake, Screen Flash, and Haptics are working
- `BFProjectVerificationTrigger` runtime component, firing each Feedback event on demand
- `BFProjectVerificationBouncer` runtime component, spawning 3 randomly colored/sized bouncing balls to visualize Hitstop and Screen Shake

### Fixed
- Qualified `UnityEngine.Object` in `BFNewProjectSetup` to resolve a `CS0104` ambiguity with `System.Object`
- Moved `BFProjectVerificationTrigger` out of the Editor assembly into a Runtime assembly — Unity does not allow attaching a `MonoBehaviour` defined in an Editor-only assembly to a GameObject
- Verification scene's camera now positioned at `z = -10` instead of the origin, so spawned sprites render in Game view instead of being clipped by the near plane