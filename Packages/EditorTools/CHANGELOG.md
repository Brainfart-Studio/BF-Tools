# Changelog

## [0.1.0] - Project Setup and Project Verification

### Added
- Scaffold Editor Tools package
- `BF Tools > New Project Setup` menu command: creates the Logger and Global Bootstrapper configs, creates prefab variants + configs for Hitstop, Screen Shake, Screen Flash, and Haptics, seeds each config with a `"Default"` entry, assigns each config to its prefab, and wires all four prefabs into the Global Bootstrapper config
- `BF Tools > New Project Verification` menu command: creates a `BFToolsTest` scene with a camera, 3 bouncing test balls, and 4 UI buttons wired to fire each Feedback event with `eventName = "Default"`, for visually confirming Hitstop, Screen Shake, Screen Flash, and Haptics are working
- `BFProjectVerificationTrigger` runtime component, firing each Feedback event on demand
- `BFProjectVerificationBouncer` runtime component, spawning 3 randomly colored/sized bouncing balls to visualize Hitstop and Screen Shake

### Fixed
- Qualified `UnityEngine.Object` in `BFNewProjectSetup` to resolve a `CS0104` ambiguity with `System.Object`
- Moved `BFProjectVerificationTrigger` out of the Editor assembly into a Runtime assembly — Unity does not allow attaching a `MonoBehaviour` defined in an Editor-only assembly to a GameObject
- Verification scene's camera now positioned at `z = -10` instead of the origin, so spawned sprites render in Game view instead of being clipped by the near plane
