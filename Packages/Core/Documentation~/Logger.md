# Logger

Static, tag-based logging with per-tag level overrides, pluggable sinks, and a console + file sink included out of the box.

## Setup
1. `Assets/Create/BFTools/Config/Logger Config` creates a config at `Assets/Resources/BFTools/BFLoggerConfig.asset`.
2. Configure the config asset.
   - `Global Minimum Level` is the default level below which nothing logs (default `Info`).
   - `Tag Level Overrides` are per-tag `tag` / `minimumLevel` entries. A matching override fully replaces the global minimum for that tag, in either direction (more or less verbose).
   - `Use Tag Allowlist` and `Tag Allowlist` control tag filtering. When enabled, only logs carrying at least one allowlisted tag are emitted, on top of the level check.
   - `Stack Trace Minimum Level` is the level at or above which a stack trace is attached (default `Error`).
3. Initialize the logger once at startup (e.g. from a Bootstrapper) with the config and whichever sinks you want active.
   ```csharp
   BFLogger.Initialize(loggerConfig, new UnityConsoleSink(), new FileSink());
   ```
   Logging is a no-op until `Initialize` is called.

## Usage
```csharp
BFLogger.Info("Combat", "Player dealt 12 damage");
BFLogger.Warning(new[] { "Combat", "AI" }, "Enemy pathfinding failed", context: gameObject);
BFLogger.Error("Save", "Failed to write save file");
```
Every level (`Trace`, `Debug`, `Info`, `Warning`, `Error`, `Critical`) has a single-tag and multi-tag overload, plus an optional `Object context` for ping-to-object in the console.

## How it works
- A log call resolves the effective minimum level for its tags. If any tag has a `TagLevelOverride`, that override wins (most permissive among multiple matching tags); otherwise the global minimum applies.
- If the resolved level check passes and (when enabled) the allowlist check passes, the message is dispatched to every sink passed to `Initialize`.
- `UnityConsoleSink` color-codes by level and routes to `Debug.LogFormat` as `Log`/`Warning`/`Error` depending on level; the message is passed as a format argument (not the format string), so braces in message content can't throw.
- `FileSink` only writes `Warning` and above, to `<persistentDataPath>/Logs/bftools.log`, rotating to `bftools.log.bak` (previous backup deleted) once the active file hits 1 MB.
- `Trace` and `Debug` calls are stripped entirely from non-development, non-editor builds via `[Conditional]`. Call sites (and their arguments) don't exist in release builds, so don't rely on their side effects.

## Notes
- `Initialize` isn't additive. Calling it again replaces the config and the full sink list.
- Stack traces are captured via `Environment.StackTrace` at the sink, not the call site, so trimmed/inlined release code can shift the reported frame.