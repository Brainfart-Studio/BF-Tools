# Event Bus

Generic static event bus for decoupled pub/sub communication using struct-based event payloads.

## Usage

Define an event as a struct:

```csharp
public struct PlayerDiedEvent
{
    public int PlayerId;
}
```

Subscribe:
```csharp
EventBus<PlayerDiedEvent>.Subscribe(OnPlayerDied);
```

Unsubscribe (always pair with Subscribe, e.g. in `OnDisable`/`OnDestroy`):
```csharp
EventBus<PlayerDiedEvent>.Unsubscribe(OnPlayerDied);
```

Fire:
```csharp
EventBus<PlayerDiedEvent>.Fire(new PlayerDiedEvent { PlayerId = 1 });
```

## How it works

- One static subscriber list per closed generic type `T`. `EventBus<PlayerDiedEvent>` and `EventBus<EnemySpawnedEvent>` are entirely separate lists.
- `T : struct` constraint enforces value-type event data, avoiding shared mutable event objects.
- `Subscribe` checks `Contains` before adding to prevent duplicate registration. A `null` handler is rejected with a `Warning` log and ignored. A handler that's already subscribed also logs a `Warning` and is ignored.
- `Fire` invokes a snapshot (`ToArray()`) of the subscriber list, taken at the start of the call, in reverse order. Because the snapshot is independent of the live list, a handler can safely `Subscribe` or `Unsubscribe` any handler, including one not yet invoked in that pass, without skipping or double-invoking other subscribers.
- `Clear()` empties all subscribers for that `T`, useful for scene transitions or teardown, but must be called explicitly (e.g. from a bootstrapper), as it's not automatic.

## Notes
- No `BF` prefix on `EventBus<T>`, flagged as a naming inconsistency to resolve.
- Static lists persist for the lifetime of the application domain; forgetting to `Unsubscribe` causes stale references.