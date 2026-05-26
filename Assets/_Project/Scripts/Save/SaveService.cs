// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Save / SaveService
//
// JSON, atomic, 3 manual slots + 1 autosave.
// Atomic write: write to slot_N.json.tmp → fsync → rename to slot_N.json.
// Survives power-fail (per Focus 07 § 4.5).

using System;
using System.IO;
using UnityEngine;
using HearthboundHollow.Core;

namespace HearthboundHollow.Save
{
    public class SaveService
    {
        private const string AutosaveFileName = "autosave.json";

        private readonly string _saveRoot;

        public SaveService()
        {
            _saveRoot = Path.Combine(Application.persistentDataPath, "saves");
            try { Directory.CreateDirectory(_saveRoot); }
            catch (Exception ex) { Hh.Err(LogCategory.Save, $"Could not create save dir '{_saveRoot}': {ex}"); }
        }

        public string GetSlotPath(int slot) =>
            slot < 0 ? Path.Combine(_saveRoot, AutosaveFileName)
                     : Path.Combine(_saveRoot, $"slot_{slot}.json");

        public bool SlotExists(int slot) => File.Exists(GetSlotPath(slot));

        public bool Save(int slot, VillageState state)
        {
            if (state == null) { Hh.Err(LogCategory.Save, "SaveService.Save: state is null."); return false; }
            try
            {
                var snap = VillageStateSnapshot.FromState(state);
                var json = JsonUtility.ToJson(snap, prettyPrint: true);
                AtomicWrite(GetSlotPath(slot), json);
                Hh.Log(LogCategory.Save, $"Saved slot {slot} ({json.Length} bytes).");
                // BUGFIX: VillageStateSavedEvent ctor parameter is named `autosave`, not `isAutosave`.
                // Using positional argument here for clarity — autosave when slot < 0.
                EventBus.Publish(new VillageStateSavedEvent(slot, slot < 0));
                return true;
            }
            catch (Exception ex)
            {
                Hh.Err(LogCategory.Save, $"SaveService.Save failed: {ex}");
                return false;
            }
        }

        public bool Load(int slot, VillageState state)
        {
            if (state == null) { Hh.Err(LogCategory.Save, "SaveService.Load: state is null."); return false; }
            var path = GetSlotPath(slot);
            if (!File.Exists(path)) { Hh.Warn(LogCategory.Save, $"No save at {path}."); return false; }
            try
            {
                var json = File.ReadAllText(path);
                var snap = JsonUtility.FromJson<VillageStateSnapshot>(json);
                if (snap == null) { Hh.Err(LogCategory.Save, "Save JSON parse returned null."); return false; }
                if (snap.schemaVersion != 1) Hh.Warn(LogCategory.Save, $"Save schema version {snap.schemaVersion} differs from current (1).");
                snap.ApplyTo(state);
                EventBus.Publish(new VillageStateLoadedEvent(state));
                Hh.Log(LogCategory.Save, $"Loaded slot {slot} (saved at {snap.savedAtIso}).");
                return true;
            }
            catch (Exception ex)
            {
                Hh.Err(LogCategory.Save, $"SaveService.Load failed: {ex}");
                return false;
            }
        }

        public string GetSlotLabel(int slot)
        {
            var path = GetSlotPath(slot);
            if (!File.Exists(path)) return "<empty>";
            try
            {
                var json = File.ReadAllText(path);
                var snap = JsonUtility.FromJson<VillageStateSnapshot>(json);
                if (snap == null) return "<corrupt>";
                return $"Day {snap.currentDayIndex} · Coin {snap.coin} · {snap.savedAtIso}";
            }
            catch (Exception) { return "<error>"; }
        }

        public void DeleteSlot(int slot)
        {
            var path = GetSlotPath(slot);
            try { if (File.Exists(path)) File.Delete(path); }
            catch (Exception ex) { Hh.Err(LogCategory.Save, $"Delete slot {slot} failed: {ex}"); }
        }

        // ───── Atomic write ────────────────────────────────────────

        private static void AtomicWrite(string targetPath, string contents)
        {
            var tmp = targetPath + ".tmp";
            File.WriteAllText(tmp, contents);
            try
            {
                using var fs = new FileStream(tmp, FileMode.Open, FileAccess.ReadWrite);
                fs.Flush(true);
            }
            catch (Exception ex)
            {
                Hh.Warn(LogCategory.Save, $"fsync flush failed (non-fatal): {ex.Message}");
            }
            if (File.Exists(targetPath))
            {
                try { File.Delete(targetPath); } catch { /* swallow */ }
            }
            File.Move(tmp, targetPath);
        }
    }
}
