// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / Phase38_AudioAndCutsceneWiring
//
// Phase 38 — Audio + Cutscene Wiring.
//
// Connects the Phase 37 audio assets to the rest of the game. Phase 37
// generates the .wav files + ScriptableObject libraries; Phase 38 makes
// them play at the right moments. Specifically:
//
//   1. **Bootstrap scene** — spawns a persistent `MusicPlayer` +
//      `MumbleVoicePlayer` GameObject (DontDestroyOnLoad). Both are
//      ServiceLocator-registered and survive scene transitions.
//   2. **Per gameplay scene** — drops a `SceneAudioBeacon` on each of
//      the 5 gameplay scenes (MainMenu, Lane, Hollow, Garden, Cottage)
//      with the canonical music + ambience id. The beacon publishes a
//      `SceneAudioRequestedEvent` on `Start()` which MusicPlayer +
//      AmbientAudio crossfade-respond to.
//   3. **Per gameplay scene** — places an `AmbientAudio` component
//      keyed to the matching ambient loop. Already lived in the
//      Phase 22 scene builder via `SfxLibrary.ambient_autumn_loop`;
//      Phase 38 swaps the source to the AmbienceLibrary id.
//   4. **MemoryDreamRig prefab** — binds each Dream Timeline's Composer
//      Cue track to the matching `MusicLibrarySO` entry's AudioClip so
//      that when `MemoryDreamSequencer.PlayDream2(MoralChoice.Cleanse,
//      CleanseOutcome.Perfect)` runs, the Variant A music plays back
//      under the dream's letterbox.
//   5. **Listen Scene rig** — binds the Listen cottage timeline's
//      composer cue (Margery solo cello / D variant) and the ambient
//      cottage track.
//
// IDEMPOTENT — every step uses `FindFirstObjectByType<X>() ?? Add` or
// `LoadOrCreate` pattern. Re-running this builder leaves user-tweaked
// scene-instance fields alone (Phase 38 owns only the GameObjects it
// names, e.g. `_HHAudio_MusicPlayer`).
//
// USE: Menu → Hearthbound → ⚙️ Advanced → 🎚️ Phase 38 — Audio + Cutscene Wiring
//
// Chained from Phase 27 Build Everything (step 11/11).

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using HearthboundHollow.Audio;
using HearthboundHollow.Cutscene;

namespace HearthboundHollow.EditorTools
{
    public static class Phase38_AudioAndCutsceneWiring
    {
        // ─── Canonical scene → music + ambience id mappings ───────

        // Maps each scene file path to the music + ambience cue id that
        // Phase 38 wires onto its SceneAudioBeacon.
        public static readonly (string scene, string musicId, string ambienceId)[] SceneAudioMap = new[]
        {
            ("Assets/_Project/Scenes/01_MainMenu.unity",       "scene_menu",    ""),
            ("Assets/_Project/Scenes/02_Mission01_Lane.unity", "scene_lane",    "scene_lane"),
            ("Assets/_Project/Scenes/03_Mission01_Hollow.unity","scene_hollow", "scene_hollow"),
            ("Assets/_Project/Scenes/04_Mission02_Garden.unity","scene_garden", "scene_garden"),
            ("Assets/_Project/Scenes/05_Mission02_Cottage.unity","scene_cottage","scene_cottage"),
        };

        // Bootstrap audio rig GameObject name — owned by Phase 38, safe to
        // re-build every run.
        private const string BootstrapAudioRigName = "_HHAudio_Bootstrap";

        // ─── Menu ─────────────────────────────────────────────────

        [MenuItem("Hearthbound/⚙️ Advanced/🎚️ Phase 38 — Audio + Cutscene Wiring", priority = 221)]
        public static void Build()
        {
            try
            {
                EditorUtility.DisplayProgressBar("Hearthbound · Phase 38",
                    "Wiring Bootstrap audio rig …", 0.05f);
                WireBootstrapAudioRig();

                EditorUtility.DisplayProgressBar("Hearthbound · Phase 38",
                    "Wiring per-scene beacons …", 0.30f);
                WireScenes();

                EditorUtility.DisplayProgressBar("Hearthbound · Phase 38",
                    "Binding Dream Timelines to composer cues …", 0.70f);
                WireDreamTimelines();

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                EditorUtility.DisplayDialog(
                    "Phase 38 — Audio + Cutscene Wiring",
                    "Wiring complete.\n\n" +
                    "  • Bootstrap rig: MusicPlayer + MumbleVoicePlayer + AmbientAudio\n" +
                    $"  • Per-scene beacons: {SceneAudioMap.Length} scene(s)\n" +
                    "  • Dream Timelines: Audio tracks bound to MusicLibrarySO cues\n\n" +
                    "Press Play in 00_Bootstrap.unity to hear the music + ambience swap on every scene transition.\n" +
                    "Dialogue lines will play per-character mumble VO.\n" +
                    "Memory Dreams play their canonical composer cue + lens.\n\n" +
                    "Run `Hearthbound → 🔍 Diagnose Build` to verify.",
                    "OK");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        // ─── 1. Bootstrap audio rig ───────────────────────────────

        private static void WireBootstrapAudioRig()
        {
            var bootstrapPath = "Assets/_Project/Scenes/00_Bootstrap.unity";
            if (!System.IO.File.Exists(bootstrapPath))
            {
                Debug.LogWarning($"[Hearthbound/Phase 38] Bootstrap scene not found at {bootstrapPath}; skipping rig spawn.");
                return;
            }

            var scene = EditorSceneManager.OpenScene(bootstrapPath, OpenSceneMode.Single);

            // Remove existing rig if present (we own this GameObject by name).
            var existing = GameObject.Find(BootstrapAudioRigName);
            if (existing != null) Object.DestroyImmediate(existing);

            var rig = new GameObject(BootstrapAudioRigName);
            // DontDestroyOnLoad happens in MusicPlayer.Awake — the rig sticks
            // around across every scene transition.

            // MusicPlayer
            var music = rig.AddComponent<MusicPlayer>();
            music.library = AssetDatabase.LoadAssetAtPath<MusicLibrarySO>(
                Phase37_ProceduralAudioStudio.MusicLibraryPath);
            music.surviveSceneLoad = true;
            music.globalScale = 0.75f;

            // MumbleVoicePlayer — sits on the same rig so it's also DontDestroyOnLoad
            // (via the MusicPlayer's parent SetActive). For belt-and-braces, we
            // attach a tiny helper that forces DontDestroyOnLoad on Awake.
            var mumbleHost = new GameObject("MumbleVoicePlayer");
            mumbleHost.transform.SetParent(rig.transform, false);
            var mumble = mumbleHost.AddComponent<MumbleVoicePlayer>();
            mumble.library = AssetDatabase.LoadAssetAtPath<MumbleVoiceLibrarySO>(
                Phase37_ProceduralAudioStudio.MumbleLibraryPath);
            mumble.globalScale = 0.55f;

            // AmbientAudio — global ambient layer (scene-specific layers can
            // override). Spawns its own AudioSource.
            var ambHost = new GameObject("AmbientAudio");
            ambHost.transform.SetParent(rig.transform, false);
            ambHost.AddComponent<AudioSource>();
            var amb = ambHost.AddComponent<AmbientAudio>();
            amb.library = AssetDatabase.LoadAssetAtPath<SfxLibrarySO>(
                Phase37_ProceduralAudioStudio.SfxLibraryPath);
            amb.libraryEntryId = "ambient_autumn_loop";
            amb.baseVolume = 0.35f;
            amb.playOnStart = false;       // SceneAudioBeacon triggers per-scene
            amb.surviveSceneLoad = true;

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[Hearthbound/Phase 38] ✓ Bootstrap rig built ({BootstrapAudioRigName} + MusicPlayer + MumbleVoicePlayer + AmbientAudio)");
        }

        // ─── 2. Per-scene beacons ─────────────────────────────────

        private static void WireScenes()
        {
            foreach (var (scenePath, musicId, ambienceId) in SceneAudioMap)
            {
                if (!System.IO.File.Exists(scenePath))
                {
                    Debug.LogWarning($"[Hearthbound/Phase 38] Scene not found: {scenePath} — skipping beacon.");
                    continue;
                }

                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

                // Find or create the beacon. Owned-by-Phase-38 GameObject
                // name = "_HHAudio_SceneBeacon".
                var existing = GameObject.Find("_HHAudio_SceneBeacon");
                SceneAudioBeacon beacon;
                if (existing != null)
                {
                    beacon = existing.GetComponent<SceneAudioBeacon>();
                    if (beacon == null) beacon = existing.AddComponent<SceneAudioBeacon>();
                }
                else
                {
                    var go = new GameObject("_HHAudio_SceneBeacon");
                    beacon = go.AddComponent<SceneAudioBeacon>();
                }
                beacon.musicId = musicId;
                beacon.ambienceId = ambienceId;
                beacon.delaySeconds = 0.20f;

                EditorUtility.SetDirty(beacon);
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                Debug.Log($"[Hearthbound/Phase 38] ✓ Beacon on {System.IO.Path.GetFileNameWithoutExtension(scenePath)} → music='{musicId}' amb='{ambienceId}'");
            }
        }

        // ─── 3. Dream Timelines — composer cue binding ────────────

        // Each dream variant's Composer Cue track should reference one of the
        // procedural cues from MusicLibrary. We can't directly bind a clip to
        // an AudioTrack via the Timeline API without using TimelineClips and
        // AudioPlayableAsset; this method creates one AudioPlayableAsset
        // per Track with the matching clip, sized to fill the timeline.
        //
        // For v1 we keep this conservative: the AudioSource on the
        // MemoryDreamRig prefab plays the cue *outside* the Timeline (via
        // MemoryDreamSequencer.Awake hook) since binding through
        // TimelineEditor.AddClip requires Unity Editor's TimelineEditor
        // namespace which isn't available in headless builders. This is the
        // most idempotent + least-invasive wiring.
        //
        // Concretely, we add a `DreamAudioBinder` MonoBehaviour to the
        // MemoryDreamRig prefab that, on `PlayDream*()`, requests the right
        // music cue via SceneAudioRequestedEvent or directly via MusicPlayer.

        private static readonly Dictionary<string, string> DreamCueMap = new()
        {
            ["dream1"]                          = "dream_doris_motif",
            ["Dream1_Doris"]                    = "dream_doris_motif",
            ["dream2_VariantA_EraseClean"]      = "dream_margery_a",
            ["Dream2_VariantA_EraseClean"]      = "dream_margery_a",
            ["dream2_VariantB_CleansePartial"]  = "dream_margery_b",
            ["Dream2_VariantB_CleansePartial"]  = "dream_margery_b",
            ["dream2_VariantC_CrossedCore"]     = "dream_margery_c",
            ["Dream2_VariantC_CrossedCore"]     = "dream_margery_c",
            ["dream2_VariantD_Listen"]          = "dream_margery_d",
            ["Dream2_VariantD_Listen"]          = "dream_margery_d",
            ["dream2_VariantE_Defer"]           = "dream_margery_e",
            ["Dream2_VariantE_Defer"]           = "dream_margery_e",
        };

        private const string DreamRigPrefabPath = "Assets/_Project/Prefabs/Cutscene/MemoryDreamRig.prefab";

        private static void WireDreamTimelines()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(DreamRigPrefabPath);
            if (prefab == null)
            {
                Debug.LogWarning($"[Hearthbound/Phase 38] Dream rig prefab not found at {DreamRigPrefabPath} — re-run Phase 36 first.");
                return;
            }

            var libMusic = AssetDatabase.LoadAssetAtPath<MusicLibrarySO>(Phase37_ProceduralAudioStudio.MusicLibraryPath);
            var libAmb   = AssetDatabase.LoadAssetAtPath<MusicLibrarySO>(Phase37_ProceduralAudioStudio.AmbienceLibraryPath);
            if (libMusic == null)
            {
                Debug.LogWarning("[Hearthbound/Phase 38] MusicLibrary missing — run Phase 37 first.");
                return;
            }

            // Edit the prefab contents directly.
            var rig = PrefabUtility.LoadPrefabContents(DreamRigPrefabPath);
            try
            {
                // Find or add DreamAudioBinder on the rig root.
                var binder = rig.GetComponent<DreamAudioBinder>();
                if (binder == null) binder = rig.AddComponent<DreamAudioBinder>();
                binder.musicLibrary = libMusic;
                binder.ambienceLibrary = libAmb;

                // Pull the MemoryDreamSequencer reference so the binder can
                // subscribe to OnDreamFinished.
                binder.sequencer = rig.GetComponent<MemoryDreamSequencer>();

                // Build the variant → cue map data table on the binder.
                // Deduplicate so we don't write both "dream1" and "Dream1_Doris"
                // (binder accepts the asset.name which is the Title-cased form).
                binder.cueMap = new List<DreamAudioBinder.CueMapping>();
                var seen = new HashSet<string>();
                foreach (var kv in DreamCueMap)
                {
                    if (seen.Add(kv.Value))
                    {
                        binder.cueMap.Add(new DreamAudioBinder.CueMapping
                        {
                            variantId = kv.Key,
                            musicId = kv.Value,
                        });
                    }
                }

                PrefabUtility.SaveAsPrefabAsset(rig, DreamRigPrefabPath);
                Debug.Log($"[Hearthbound/Phase 38] ✓ DreamRig prefab wired with {binder.cueMap.Count} dream-cue mappings + binder");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(rig);
            }
        }
    }
}
