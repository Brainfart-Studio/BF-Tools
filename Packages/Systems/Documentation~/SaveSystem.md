# Save System

ISaveable-based state capture and restore, persisted to encrypted, checksummed, slot-based save files with a per-install key and a version migration scaffold.

## Usage

Implement `ISaveable` on anything that needs to persist state, and register/unregister it with `BFSaveManager`:

```csharp
public class PlayerProgress : MonoBehaviour, ISaveable
{
    [Serializable]
    private struct State
    {
        public int level;
        public int gold;
    }

    public Type StateType => typeof(State);

    public object CaptureState() => new State { level = level, gold = gold };

    public void RestoreState(object state)
    {
        State s = (State)state;
        level = s.level;
        gold = s.gold;
    }

    private void OnEnable() => BFSaveManager.Register(this);
    private void OnDisable() => BFSaveManager.Unregister(this);
}
```

Then save or load a named slot from anywhere:

```csharp
bool saved = await BFSaveManager.SaveAsync("slot1");
bool loaded = await BFSaveManager.LoadAsync("slot1");
```

Both calls return `false` instead of throwing when something goes wrong. `LoadAsync` returning `false` on first boot (no save exists yet) is expected and not itself an error. `SaveAsync` returning `false` means the write failed (disk full, permission denied, path missing) and the slot's on-disk files may be missing or incomplete; the failure is logged as a `Warning` either way.

Slot names become part of a file name on disk, so they're validated: `null`, empty, or whitespace-only names, path separators (`/`, `\`), `..`, and any character invalid in a file name all throw an `ArgumentException` from `GetFileNameForSlot`, `SaveAsync`, or `LoadAsync`.

## How it works

- `BFSaveManager` holds registered `ISaveable`s in a `BFStateRegistry<ISaveable>`, an internal helper shared with `BFSettingsManager` that owns the register/unregister list, the per-type allowlisting, and the capture/restore loop. `SaveAsync` calls `CaptureState()` on each registered saveable and stores the result in a `Dictionary<string, object>` on `BFSaveData`, keyed by the saveable's **fully qualified** type name (not just the short class name), so two different `PlayerProgress` classes in different namespaces can't collide on the same key.
- The `BFSaveData` (states plus `BFSaveMetadata`, which holds version, UTC timestamp, and playtime) is serialized to JSON, encrypted (AES), checksummed (HMAC-SHA256), and written as two files per slot: `save_<slotName>.dat` (encrypted state) and `save_<slotName>.dat.chk` (checksum). Both go through `BFTools.Core.FileIO.BFFileIO`'s write-to-`.tmp`-then-rename pattern, so a crash mid-write can't leave a half-written file at the real path. `BFFileIO` is a generic Core utility (not save-specific), also used by `BFSettingsManager` for its own settings file.
- `LoadAsync` reads both files, validates the checksum first, then decrypts and deserializes. On success it runs the data through `BFSaveVersionMigrator` if the stored version doesn't match current, then calls `RestoreState()` on every registered `ISaveable` whose type has a matching entry. A saveable with no matching entry (e.g. it was added after the save was written) is left at its current runtime state rather than erroring. If a saveable's `RestoreState` itself throws, that one saveable is skipped (logged as a `Warning`) and the rest of the load continues rather than aborting.
- Slots are also tracked in memory. `BFSaveManager.RegisterSlot`/`TryGetSlot` cache each slot's `BFSaveMetadata` after every successful save or load, so you can list/inspect known slots without re-reading from disk. The saveable registry, the slot list, and the cached encryption key are all safe to read and mutate from multiple threads; each is guarded by its own lock internally.
- Default location for everything (`save.key`, save files, checksums) is `Application.persistentDataPath`. Both `SaveAsync` and `LoadAsync` have an overload that takes an explicit directory instead.
- Everything logs under the `Save` tag via `BFLogger` at `Trace` (routine steps), `Debug` (capture/restore counts), `Info` (key generation), or `Warning` (recoverable load/save failures, and any version mismatch the migrator can't yet handle).
- Only `ISaveable` and `BFSaveManager` are meant to be called from outside the package. The serializer, encryptor, checksum, key provider, and migrator are internal implementation details of the pipeline, not a public API surface.

## Security

The save system encrypts and integrity-checks saves, but what that protects against and what it doesn't matters, since everything needed to decrypt a save lives on the same device as the save itself.

- **Encryption.** The encryptor uses AES with a 256-bit key. A fresh random IV is generated per encrypt call (never reused across saves) and prepended to the ciphertext; decrypting reads it back off the front of the byte array before decrypting the rest. Decrypting data shorter than one IV block, or `null` plaintext/ciphertext, fails immediately with a clear exception rather than a confusing one from deep inside the crypto stack.
- **Key storage.** The AES key is generated once, on first use, via a cryptographic RNG, and persisted to `save.key` in the same directory as the save files (`Application.persistentDataPath` by default), using the same write-to-temp-then-atomic-replace pattern as the save files themselves. Every subsequent encrypt/decrypt on that install reuses the cached key.
- **What this does and doesn't buy you.** The key sits right next to the data it protects, so this is not designed to hide save contents from the device's owner. Anyone with local file-system access can read `save.key` and decrypt every save on that install. It still provides the following.
  - Casual tamper-resistance. A save file isn't plain, editable JSON, so hand-editing stats/currency requires deliberately locating and decrypting the key first, not just opening the file in a text editor.
  - Combined with the checksum below, a save that's been edited or corrupted fails to load cleanly instead of silently corrupting game state.
  - It is **not** anti-cheat. A player motivated enough to find `save.key`, decrypt, edit, and re-encrypt (or grab a tool someone else already wrote) can. Anything that needs to be authoritative against a motivated local attacker (competitive stats, purchasable currency, etc.) needs server-side validation, not client-side save encryption.
- **Integrity.** The checksum is an HMAC-SHA256 over the encrypted bytes, keyed by a MAC key derived from the save key (itself an HMAC of a fixed context string using the encryption key). Using a *keyed* MAC instead of a plain hash matters. With a plain SHA-256, anyone editing the ciphertext could just recompute and overwrite the checksum to match. With a keyed MAC, they'd need the same on-disk key to produce a checksum that validates. As with encryption, this defends against corruption, bit-rot, and naive tampering, not against someone who already has `save.key`.
- **Deserialization safety.** Save data is dictionary-of-`object` keyed by type name, so the serializer needs `TypeNameHandling.Auto` to round-trip it. That setting is normally an insecure-deserialization risk on its own, since it lets a crafted file name arbitrary types to instantiate. An allowlist binder closes that off. Only types that have actually been registered as an `ISaveable.StateType` are resolvable during deserialization (registered automatically the first time a saveable of that type registers). Any other type name in the file throws and fails the load rather than instantiating something unexpected.
- **Failure handling.** Every decrypt/deserialize step in `LoadAsync` is wrapped in a `try`/`catch`; a wrong key, corrupted bytes, a disallowed type, or malformed JSON all end the same way, a `Warning` log and a `false` return, never an unhandled exception. Both files missing is treated as "no save yet" (no warning); one file present without the other is logged as an incomplete/interrupted write.

## Notes
- `BFSaveMetadata.playtimeSeconds` is currently always written as `0f`. `SaveAsync` doesn't track elapsed playtime yet; treat the field as a placeholder until that's wired up.
- Version migration is scaffolding only. `CurrentVersion` is `1`, and running `Migrate` against an older version passes the data through unchanged and logs a `Warning` that no migration steps exist for it yet, rather than claiming a migration happened. Add real migration steps as the save format changes.
- No editor tooling yet, no config asset creators, no in-editor save inspector.
- `save.key` lives in the same directory as save data by default. If you need it excluded from automatic device backups or cloud sync (Steam Cloud, iCloud, etc.), point `BFSavePath`'s default directory elsewhere or manage that exclusion yourself; the system doesn't do it for you.
- Save files written before the fully-qualified-type-name key change won't match on load. Each registered saveable will log a harmless "no saved state found" and keep its current state, same as any other unmatched key; nothing throws.
