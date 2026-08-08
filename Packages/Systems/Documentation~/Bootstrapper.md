# Bootstrapper

Two independent bootstrap systems: **Global** (app-lifetime systems, initialized before scene load) and **Level** (per-scene systems, initialized on scene load).

## Global Bootstrapper

Instantiates persistent system prefabs once, before the first scene loads. Prefabs are parented to root and marked `DontDestroyOnLoad`.

### Setup
1. `Assets/Create/BFTools/Systems/Config/Global Bootstrap Config` creates config at `Assets/Resources/BFTools/GlobalBootstrapConfig.asset`.
2. Assign system prefabs to the config's `System Prefabs` array.

Or run `BF Tools/New Project Setup` (in `com.bftools.editortools`): it creates the config and fills `System Prefabs` with whatever prefabs the other installed BFTools modules created during the same run.

### How it works
- `BFGlobalBootstrapper.Initialize()` runs via `[RuntimeInitializeOnLoadMethod(BeforeSceneLoad)]`.
- Loads `BFGlobalBootstrapperConfig` from `Resources/BFTools/GlobalBootstrapConfig`.
- Logs an error and aborts if the config asset is missing, or if `SystemPrefabs` is null.
- Instantiates each non-null prefab in `SystemPrefabs`, sets parent to null, calls `DontDestroyOnLoad`.
- A prefab that fails to instantiate is logged as an error and skipped; the rest of the array still runs.
- Logs Trace on config load, Warning for each skipped null entry, Debug per successful instantiation, and Info with the final spawned count.

### Notes
- Config must live in `Resources/`. Nothing exists at boot to hold a direct reference.
- `SystemPrefabs` is `internal`, only accessible within the Systems assembly.

## Level Bootstrapper

Instantiates scene-specific prefabs via `Awake()` on a `MonoBehaviour`, using a directly assigned config (no `Resources.Load`).

### Setup
1. `Assets/Create/BFTools/Systems/Config/Level Bootstrap Config` creates config at `Assets/Configs/Systems/LevelBootstrapper/LevelBootstrapConfig.asset`.
2. `Assets/Create/BFTools/Systems/Prefabs/Level Bootstrapper` creates a prefab variant of the base `LevelBootstrapper` prefab at `Assets/Prefabs/Systems/LevelBootstrapper.prefab`.
3. Place the prefab variant in the scene, assign the config to its `Config` field.

### How it works
- `BFLevelBootstrapper.Awake()` iterates `config.PrefabsToInstantiate` and instantiates each.
- Logs an error and aborts if no config is assigned.

### Notes
- Unlike Global, config is a direct serialized reference. No `Resources/` folder needed.
- Prefab variant creator instantiates the base prefab (`Packages/com.bftools.systems/LevelBootstrapper/Prefabs/LevelBootstrapper.prefab`), saves as a new asset, and destroys the temp instance.