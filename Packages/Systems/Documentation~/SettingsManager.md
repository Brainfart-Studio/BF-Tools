\# Settings Manager



ISettingsProvider-based state capture and restore, persisted as plain JSON to a single shared `settings.json`.



\## Usage



Implement `ISettingsProvider` on anything holding user-facing settings, and register/unregister it with `BFSettingsManager`:



```csharp

public class AudioSettings : MonoBehaviour, ISettingsProvider

{

&#x20;   \[Serializable]

&#x20;   private struct State

&#x20;   {

&#x20;       public float musicVolume;

&#x20;       public float sfxVolume;

&#x20;   }



&#x20;   public Type StateType => typeof(State);



&#x20;   public object CaptureState() => new State { musicVolume = musicVolume, sfxVolume = sfxVolume };



&#x20;   public void RestoreState(object state)

&#x20;   {

&#x20;       State s = (State)state;

&#x20;       musicVolume = s.musicVolume;

&#x20;       sfxVolume = s.sfxVolume;

&#x20;   }



&#x20;   private void OnEnable() => BFSettingsManager.Register(this);

&#x20;   private void OnDisable() => BFSettingsManager.Unregister(this);

}

```



Then save or load every registered provider's state in one call:



```csharp

await BFSettingsManager.SaveAsync();

bool loaded = await BFSettingsManager.LoadAsync();

```



`LoadAsync` returns `false` (without throwing) if `settings.json` doesn't exist yet or couldn't be parsed, so a `false` result on first boot is expected and not itself an error.



\## How it works



\- `BFSettingsManager` keeps an in-memory list of registered `ISettingsProvider`s. `SaveAsync` calls `CaptureState()` on each and stores the result on a `BFSettingsData` wrapper's `Dictionary<string, object>`, keyed by the provider's runtime type name.

\- `BFSettingsData` is serialized to JSON (`BFSaveSerializer`, Newtonsoft) and written unencrypted to a single `settings.json` via `BFSaveFileIO`'s write-to-`.tmp`-then-rename pattern, so a crash mid-write can't leave a half-written file at the real path.

\- `LoadAsync` reads `settings.json` and deserializes it back to `BFSettingsData`. On success, it calls `RestoreState()` on every registered provider whose type has a matching entry. A provider with no matching entry (e.g. it was added after the file was written) is left at its current runtime state rather than erroring.

\- Default location is `Application.persistentDataPath` (`BFSavePath.DefaultDirectory`). Both `SaveAsync` and `LoadAsync` have an overload that takes an explicit directory instead.

\- Everything logs under the `Settings` tag via `BFLogger` at `Trace` (routine steps), `Debug` (capture/restore counts), or `Warning` (recoverable load failures).



\## Settings Manager vs. Save System



Both systems share `BFSaveFileIO` and `BFSaveSerializer`, but they exist for different kinds of data:



\- \*\*Save System\*\* (`BFSaveManager`) is for gameplay progress: slotted, encrypted, checksummed, versioned. Built to resist casual tampering and to fail loudly on corruption.

\- \*\*Settings Manager\*\* (`BFSettingsManager`) is for user preferences (audio, graphics, controls): a single unslotted file, plain unencrypted JSON, no checksum. Settings aren't gameplay-authoritative, so there's nothing here worth encrypting, and keeping the file human-readable makes it easy to hand-inspect or hand-edit during development.



Deserialization is safe the same way Save System's is: `BFSettingsManager.Register` allowlists a provider's `StateType` with `BFSaveSerializer` the first time that provider type registers, so only types that have actually been registered as an `ISettingsProvider.StateType` are resolvable when deserializing `settings.json`.



\## Notes

\- No editor tooling yet, no config asset creators, no in-editor settings inspector.

\- Provider state types must be `\[Serializable]` (or otherwise supported by `BFSaveSerializer`) the same as `ISaveable.StateType` in Save System.

\- `settings.json` is plain JSON with no integrity check, so unlike Save System's slots, a hand-edited or corrupted file that still parses as valid JSON will load without complaint.

