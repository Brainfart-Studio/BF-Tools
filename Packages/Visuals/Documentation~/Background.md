# Background

A layered background rendering system. It draws a stack of composited visual layers (gradient, aurora ribbons, twinkling stars) behind the scene using a dedicated camera, independent of whatever camera setup the scene already has.

## Setup
1. `Assets/Create/BFTools/Visuals/Background/Prefabs/Background Stack Manager` creates a prefab variant at `Assets/Prefabs/Visuals/Background/BackgroundStackManager.prefab`.
2. `Assets/Create/BFTools/Visuals/Background/Config/Background Stack Config` creates a stack config. One stack config is one ordered list of layers.
3. Create one or more layer configs under `Assets/Create/BFTools/Visuals/Background/Config/Layer/`. Options are Gradient, Aurora Ribbons, or Twinkling Stars. Assign them to a stack config's `Layers` list.
4. Place the manager prefab in the scene. Assign one or more stack configs to its `Stacks` list. Multiple stacks render in list order, back to front.
5. Optionally assign `Target Camera Override` if the scene's `Camera.main` isn't the camera the background should composite onto.

## Usage
No runtime API to call. The manager builds and ticks everything from its assigned configs on `OnEnable`. Add layers by creating more layer config assets and adding them to a stack's `Layers` list; add more backgrounds running independently by adding more stack configs to the manager's `Stacks` list.

Only one `BFBackgroundStackManager` may be enabled at a time; enabling a second one logs an error and disables itself.

## How it works
- `BFBackgroundStackManager` owns a `BFBackgroundStackCamera` and a list of runtime `BFBackgroundStack` instances, one per assigned `BFBackgroundStackConfig`. It builds everything in `OnEnable` and tears it down in `OnDisable`.
- `BFBackgroundStackCamera` creates a dedicated orthographic camera that exclusively renders a reserved Unity layer (`BFBackgroundStackManager.BackgroundLayer`, index 30) at a very low depth, then reconfigures the scene's output camera (`Target Camera Override` or `Camera.main`) to clear only depth and to exclude that reserved layer from its own culling mask. That's what lets the background render underneath the scene's normal camera output without either camera erasing the other's draw. If the output camera is destroyed later (e.g. a scene swap while the manager persists), it's detected and re-resolved automatically.
- `BFBackgroundStack` instantiates one `IBFBackgroundLayer` per non-null entry in its config's `Layers` list, via each `BFBackgroundLayerConfig.CreateLayer()`. Layers within a stack, and stacks within the manager, are assigned increasing `sortingOrder` values so they draw in list order.
- Three concrete layers ship today.
  - **Gradient** renders a full-screen subdivided mesh sampling a multi-key `Gradient` ramp along a configurable axis, with Midpoint and Spread controlling where the blend sits and how gradual it is. The axis can be rotated (Angle) and animated (drift, rotation, rotation oscillation), and the color transition line can be displaced into an animated wave (wave amplitude/frequency, wave oscillation, wave amplitude randomness).
  - **Aurora Ribbons** renders a configurable number of animated, additively-glowing `LineRenderer` ribbons.
  - **Twinkling Stars** renders a configurable number of stars as a single dynamic mesh, tinted by sampling a `Gradient` ramp at a random point per star. Each star's size, brightness, and twinkle speed/depth are drawn from Min/Max/Average/Outliers ranges via Perlin noise sampled at its screen position, so nearby stars vary together instead of independently. All of the above refresh live in Play mode as the config changes.
- All logging in this module goes through `BFLogger` under the `"Background"` tag; the three concrete layers additionally tag their own logs with their layer name (`"Gradient"`, `"AuroraRibbons"`, `"TwinklingStars"`) so a single layer can be isolated during debugging without silencing the rest of the system.

## Notes
- The reserved rendering layer (index 30) is exclusively owned by this system. Don't assign scene objects to it, or they'll be drawn by the background camera unexpectedly.
- Aurora Ribbons needs at least one entry in `Ribbon Colors`; an emptied list falls back to white and logs an error rather than throwing.
- Depends on `com.bftools.core` (Logger).