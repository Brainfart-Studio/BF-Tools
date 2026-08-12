# Parallax

A layered parallax scrolling system. It moves a stack of sprite layers at different rates relative to the camera, using a single stack manager you can drop into any scene.

## Setup
1. `Assets/Create/BFTools/Visuals/Parallax/Prefabs/Parallax Stack Manager` creates a prefab variant at `Assets/Prefabs/Visuals/Parallax/ParallaxStackManager.prefab`.
2. `Assets/Create/BFTools/Visuals/Parallax/Config/Parallax Stack Config` creates a stack config. One stack config is one ordered list of layers.
3. Create one or more layer configs under `Assets/Create/BFTools/Visuals/Parallax/Config/Layer/`. Sprite Layer is the only option today. Assign them to a stack config's `Layers` list.
4. Place the manager prefab in the scene. Assign one or more stack configs to its `Stacks` list. Multiple stacks tick in list order.
5. Optionally assign `Target Camera Override` if the scene's `Camera.main` isn't the camera that should drive the parallax movement.

## Usage
No runtime API to call. The manager builds and ticks everything from its assigned configs on `OnEnable`. Add layers by creating more layer config assets and adding them to a stack's `Layers` list; add more independently-configured parallax groups by adding more stack configs to the manager's `Stacks` list.

Reordering a stack's `Layers` list, or the manager's `Stacks` list, changes draw order and sorting order. Both are plain serialized lists, so drag the handle on the left of each entry to reorder them in the Inspector.

Only one `BFParallaxStackManager` may be enabled at a time; enabling a second one logs an error and disables itself.

## How it works
- `BFParallaxStackManager` owns a `BFParallaxCameraTracker` and a list of runtime `BFParallaxStack` instances, one per assigned `BFParallaxStackConfig`. It builds everything in `OnEnable` and tears it down in `OnDisable`.
- `BFParallaxCameraTracker` resolves the tracked camera (`Target Camera Override` or `Camera.main`) and records its position the moment it's resolved. Every frame it returns that camera's total displacement since then, which is what drives every layer's movement. If the camera isn't available yet (script execution order, async scene loads), it retries each frame until one resolves.
- Camera resolution goes through Core's shared `BFCameraResolver` (also used by Background). If neither `Target Camera Override` nor `Camera.main` can be found, it logs one error and keeps retrying every frame rather than spamming the console until a camera resolves.
- `BFParallaxStackManager`'s single-active-instance enforcement is backed by Core's `BFActiveInstanceGuard`, and `BFParallaxStack` builds on Core's shared `BFLayerStackBase` for per-layer instantiation and sorting order assignment.
- `BFParallaxStack` instantiates one `IBFParallaxLayer` per non-null entry in its config's `Layers` list, via each `BFParallaxLayerConfig.CreateLayer()`. Layers within a stack, and stacks within the manager, are assigned increasing `sortingOrder` values so they draw in list order.
- One concrete layer ships today.
  - **Sprite** renders a single sprite (or a tiled grid of it, when looping) and moves it based on the camera's tracked displacement. Horizontal and Vertical each have their own toggle, and are otherwise independent of each other:
    - **Parallax Factor** scales how much of the camera's movement the layer picks up. 0 keeps the layer locked to the screen, 1 matches the camera exactly, values below 1 sit farther back, values above 1 sit closer than the camera plane, and negative values invert the direction.
    - **Auto-Scroll Speed** adds a constant drift on top of the parallax factor, independent of camera movement, for layers that should move on their own (clouds, for example).
    - **Looping** tiles the sprite across a 3x3 grid (or 3x1 / 1x3, depending on which axes loop) and wraps each tile's position every tile width or height, so the layer scrolls indefinitely instead of running out of sprite. Tile size is read from the sprite's bounds unless overridden.
    - **Movement Lock** (`BFParallaxAxisLock`) clamps a layer's tracked offset to never move back past the furthest point it's reached on that axis, once it starts moving one way. Use it for a camera that shouldn't scroll backward once the player has moved forward.
  - `Initial Offset` positions a layer before any parallax or scroll is applied, and `Sorting Order Offset` fine-tunes its draw order within the stack without changing its slot in the `Layers` list.
- All logging in this module goes through `BFLogger` under the `"Parallax"` tag; the Sprite layer additionally tags its own logs with `"Sprite"`.

## Notes
- Parallax layers live in the manager's own transform hierarchy in world space, unlike Background's screen-space compositing camera. They render through the scene's normal camera and sorting layers, not a reserved rendering layer.
- Looping assumes the sprite's tile size is smaller than the visible area on that axis; a tile much larger than the screen will still scroll correctly but won't visibly repeat.
- Movement Lock tracks the offset from the moment the layer initializes; disabling and re-enabling the manager resets it.
- Depends on `com.bftools.core` (Logger, CameraUtility, LayerStack, SingletonGuard).