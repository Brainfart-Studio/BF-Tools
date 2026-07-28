# Service Locator

Static registry for locating shared services by type at runtime, avoiding hard singleton references or manual wiring between systems.

## Usage

Register a service (typically from a bootstrapper):
```csharp
BFServiceLocator.Register<IAudioService>(new AudioService());
```

Get a service:
```csharp
var audio = BFServiceLocator.Get<IAudioService>();
```

Unregister (e.g. on teardown):
```csharp
BFServiceLocator.Unregister<IAudioService>();
```

## How it works

- One dictionary keyed by `Type`, mapping to the last-registered instance for that type.
- `Register<T>` overwrites any existing entry for `T` rather than throwing on duplicate registration.
- `Get<T>` casts the stored instance back to `T` and returns it.
- `Unregister<T>` removes the entry for `T` if present; removing an unregistered type is a no-op.
- Each call logs a `Trace`-level message under the `ServiceLocator` tag.

## Notes
- `Get<T>` throws `KeyNotFoundException` if `T` was never registered. The `Trace` log fires unconditionally before the lookup, so it doesn't distinguish a hit from a miss. There's no dedicated "not found" log line.
- No lifetime management: registering a `MonoBehaviour` or other object that gets destroyed leaves a dangling reference until something calls `Unregister`.