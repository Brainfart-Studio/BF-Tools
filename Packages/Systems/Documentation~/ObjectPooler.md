# Object Pooler

Config-driven object pooling with prewarming, `Get`/`Release`, and registration with the Service Locator.

## Usage

Configure a `BFObjectPoolConfig` asset with one or more pool entries (key, prefab, prewarm count), then assign it to a `BFObjectPooler` in the scene. On `Awake`, the pooler prewarms each entry and registers itself with the Service Locator:

```csharp
var pooler = BFServiceLocator.Get<BFObjectPooler>();
GameObject instance = pooler.Get("Bullet");
// ...
pooler.Release(instance);
```

## How it works

- `BFObjectPoolConfig` holds a list of `PoolEntry` structs: `key`, `prefab`, and `prewarmCount`.
- On `Awake`, `BFObjectPooler` instantiates `prewarmCount` inactive instances per entry (parented under the pooler's transform) and registers itself with `BFServiceLocator`.
- `Get(key)` dequeues an inactive instance and activates it. If the pool for that key is exhausted, it instantiates a new instance on demand and logs a `Warning` suggesting a higher prewarm count.
- `Release(instance)` deactivates the instance and returns it to its pool.
- `OnDestroy` unregisters the pooler from `BFServiceLocator`.
- Each prewarm/get/release logs a `Trace`-level message under the `ObjectPooler` tag.

## Notes
- `Get` and `Release` key their internal dictionaries by exact string `key` / instance reference. Neither throws on bad input: an unassigned config, a config entry with no prefab, a duplicate key, an unknown or `null` key passed to `Get`, and a `null` or untracked instance passed to `Release` are all logged at `Error` level and handled without an exception, so a bad pool reference degrades to a console error instead of crashing the caller.
- No pool shrinking. Instances created to cover exhaustion stay in rotation after being released.

## Editor tooling
- **Assets/Create/BFTools/Systems/Config/Object Pool Config**. Creates a `BFObjectPoolConfig` asset under `Assets/Configs/Systems/ObjectPooler`.
- **Assets/Create/BFTools/Systems/Prefabs/Object Pooler**. Creates a prefab variant of the base `ObjectPooler` prefab under `Assets/Prefabs/Systems`.