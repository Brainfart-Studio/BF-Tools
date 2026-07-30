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
await BFSaveManager.SaveAsync("slot1");
bool loaded = await BFSaveManager.LoadAsync("slot1");
```

`LoadAsync` returns `false` (without throwing) if the slot doesn't exist yet or its data couldn't be recovered, so a `false` result on first boot is expected and not itself an error.

## How it works

- `BFSaveManager` keeps an in-memory list of registered `ISaveable`s. `SaveAsync` calls `CaptureState()` on each and stores the result in a `Dictionary<string, object>` on `BFSaveData`, keyed by the saveable's runtime type name.
- The `BFSaveData` (states plus `BFSaveMetadata`, which holds version, UTC timestamp, and playtime) is serialized to JSON (`BFSaveSerializer`, Newtonsoft), encrypted (`BFSaveEncryptor`, AES), checksummed (`BFSaveChecksum`, HMAC-SHA256), and written as two files per slot, `save_<slotName>.dat` (encrypted state) and `save_<slotName>.dat.chk` (checksum). Both go through `BFSaveFileIO`'s write-to-`.tmp`-then-rename pattern, so a crash mid-write can't leave a half-written file at the real path.
- `LoadAsync` reads both files, validates the checksum first, then decrypts and deserializes. On success it migrates the data to `BFSaveVersionMigrator.CurrentVersion` if needed, then calls `RestoreState()` on every registered `ISaveable` whose type has a matching entry. A saveable with no matching entry (e.g. it was added after the save was written) is left at its current runtime state rather than erroring.
- Slots are also tracked in memory. `BFSaveManager.RegisterSlot`/`TryGetSlot` cache each slot's `BFSaveMetadata` after every successful save or load, so you can list/inspect known slots without re-reading from disk.
- Default location for everything (`save.key`, save files, checksums) is `Application.persistentDataPath`. Both `SaveAsync` and `LoadAsync` have an overload that takes an explicit directory instead.
- Everything logs under the `Save` tag via `BFLogger` at `Trace` (routine steps), `Debug` (capture/restore counts), `Info` (migrations, key generation), or `Warning` (recoverable load failures).

## Security

The save system encrypts and integrity-checks saves, but what that protects against and what it doesn't matters, since everything needed to decrypt a save lives on the same device as the save itself.

- **Encryption.** `BFSaveEncryptor` uses AES with a 256-bit key. A fresh random IV is generated per `Encrypt()` call (never reused across saves) and prepended to the ciphertext; `Decrypt()` reads it back off the front of the byte array before decrypting the rest.
- **Key storage.** `BFSaveKeyProvider` generates the AES key once, on first use, via a cryptographic RNG, and persists it to `save.key` in the same directory as the save files (`Application.persistentDataPath` by default). Every subsequent encrypt/decrypt on that install reuses it.
- **What this does and doesn't buy you.** The key sits right next to the data it protects, so this is not designed to hide save contents from the device's owner. Anyone with local file-system access can read `save.key` and decrypt every save on that install. It still provides the following.
  - Casual tamper-resistance. A save file isn't plain, editable JSON, so hand-editing stats/currency requires deliberately locating and decrypting the key first, not just opening the file in a text editor.
  - Combined with the checksum below, a save that's been edited or corrupted fails to load cleanly instead of silently corrupting game state.
  - It is **not** anti-cheat. A player motivated enough to find `save.key`, decrypt, edit, and re-encrypt (or grab a tool someone else already wrote) can. Anything that needs to be authoritative against a motivated local attacker (competitive stats, purchasable currency, etc.) needs server-side validation, not client-side save encryption.
- **Integrity.** `BFSaveChecksum` computes an HMAC-SHA256 over the encrypted bytes, keyed by a MAC key derived from the save key (`BFSaveKeyProvider.GetMacKey()`, itself an HMAC of a fixed context string using the encryption key). Using a *keyed* MAC instead of a plain hash matters. With a plain SHA-256, anyone editing the ciphertext could just recompute and overwrite the checksum to match. With a keyed MAC, they'd need the same on-disk key to produce a checksum that validates. As with encryption, this defends against corruption, bit-rot, and naive tampering, not against someone who already has `save.key`.
- **Deserialization safety.** Save data is dictionary-of-`object` keyed by type name, so the serializer needs `TypeNameHandling.Auto` to round-trip it. That setting is normally an insecure-deserialization risk on its own, since it lets a crafted file name arbitrary types to instantiate. `BFSaveTypeAllowlistBinder` closes that off. Only types that have actually been registered as an `ISaveable.StateType` are resolvable during deserialization (`BFSaveManager.Register` allowlists a type the first time a saveable of that type registers). Any other type name in the file throws and fails the load rather than instantiating something unexpected.
- **Failure handling.** Every decrypt/deserialize step in `LoadAsync` is wrapped in a `try`/`catch`; a wrong key, corrupted bytes, a disallowed type, or malformed JSON all end the same way, a `Warning` log and a `false` return, never an unhandled exception. Both files missing is treated as "no save yet" (no warning); one file present without the other is logged as an incomplete/interrupted write.

## Notes
- `BFSaveMetadata.playtimeSeconds` is currently always written as `0f`. `SaveAsync` doesn't track elapsed playtime yet; treat the field as a placeholder until that's wired up.
- `BFSaveVersionMigrator` is scaffolding only, `CurrentVersion` is `1` and `Migrate` is currently a pass-through. Add real migration steps there as the save format changes.
- No editor tooling yet, no config asset creators, no in-editor save inspector.
- `save.key` lives in the same directory as save data by default. If you need it excluded from automatic device backups or cloud sync (Steam Cloud, iCloud, etc.), point `BFSavePath`'s default directory elsewhere or manage that exclusion yourself; the system doesn't do it for you.