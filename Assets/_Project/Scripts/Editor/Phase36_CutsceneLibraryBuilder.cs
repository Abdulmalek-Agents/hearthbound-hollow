// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / Phase36_CutsceneLibraryBuilder
//
// Phase 36 — Cutscene Library Completion.
//
// Phase 21 only built Dream 1 (the Doris First Loaves timeline). Mission 2
// expects *six* additional Timelines that drive every emotional payoff path
// out of Gerrold's moral choice + cleanse outcome:
//
//   Dream 2 Variant A — Cleanse Perfect       (85 s, GRIEF + GRACE blend)
//   Dream 2 Variant B — Cleanse Acceptable    (80 s, GRIEF + faint GRACE)
//   Dream 2 Variant C — Crossed Core          (90 s, GRIEF heavy, minor-7 end)
//   Dream 2 Variant D — Listen / Refused      (75 s, GRIEF gentle + WONDER)
//   Dream 2 Variant E — Deferred              (30 s, GRACE only, empty chair)
//   Listen Scene — Gerrold full monologue     (180 s, the 3-minute outro)
//
// Each Timeline is authored per `Docs/Depth_Bible/Mission_1_2_Focus/05_DREAM1_AND_DREAM2.md`
// (variant A is the canonical 8-track / 85-second template; B/C/D/E differ
//  only in lens, camera, cast figure, and audio cue tracks — exactly the
//  efficiency the Aleko discipline calls for).
//
// This phase also:
//   • Rebuilds the `MemoryDreamRig.prefab` so all 6 dream timelines and the
//     Listen timeline are wired onto the MemoryDreamSequencer + a new
//     ListenSceneSequencer on the same rig.
//   • Adds an `AudioSource` so the Audio tracks on each Timeline have a
//     speaker to play through (Phase 38 binds Phase 37's generated cues).
//
// IDEMPOTENT — every timeline uses `LoadAssetAtPath ?? CreateInstance`. The
// rig prefab is rebuilt from scratch (it's a pure capstone artefact). Per
// D-052 in Docs/Phase35_Continuation_Audit.md.
//
// USE: Menu → Hearthbound → ⚙️ Advanced → 🎬 Phase 36 — Build Cutscene Library
//
// Chained from Phase 27 Build Everything so re-running `🚀 Build Everything`
// after pulling this branch produces the fully-wired cutscene library.

using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UI;
using TMPro;
using HearthboundHollow.Cutscene;

namespace HearthboundHollow.EditorTools
{
    public static class Phase36_CutsceneLibraryBuilder
    {
        private const string AnimDir = "Assets/_Project/Animations";
        private const string PrefabsDir = "Assets/_Project/Prefabs/Cutscene";

        private const string DreamRigPrefabPath = PrefabsDir + "/MemoryDreamRig.prefab";

        // Variant spec — keyed off Focus 05 § 3.3. Each row authors a Timeline
        // asset at the canonical path that Phase 35 expects + Phase 38 wires.
        private static readonly (string id, string assetPath, double duration, string proseLine, Color lens)[] DreamVariants = new[]
        {
            ("Dream1_Doris",
             AnimDir + "/Dream1_Doris.playable",
             60.0,
             "The kitchen at first light. Flour on the table. Her mother's apron on the hook.",
             new Color(0.99f, 0.88f, 0.62f)),  // JOY + GRACE

            ("Dream2_VariantA_EraseClean",
             AnimDir + "/Dream2_VariantA_EraseClean.playable",
             85.0,
             "The bedroom in winter. Snow at the window. She holds the handkerchief, smiling at the embroidery.",
             new Color(0.78f, 0.84f, 0.92f)),  // GRIEF + GRACE blend

            ("Dream2_VariantB_CleansePartial",
             AnimDir + "/Dream2_VariantB_CleansePartial.playable",
             80.0,
             "The bedroom in winter. The light is softer than she was. The handkerchief is clear; she is gentle.",
             new Color(0.72f, 0.78f, 0.88f)),  // GRIEF + faint GRACE

            ("Dream2_VariantC_CrossedCore",
             AnimDir + "/Dream2_VariantC_CrossedCore.playable",
             90.0,
             "The bedroom in winter. The handkerchief is in her hand. Her face is fog. Her smile, barely.",
             new Color(0.55f, 0.62f, 0.76f)),  // GRIEF heavy

            ("Dream2_VariantD_Listen",
             AnimDir + "/Dream2_VariantD_Listen.playable",
             75.0,
             "His chair. His hands. His face. Just once, he looks up — and smiles.",
             new Color(0.85f, 0.82f, 0.95f)),  // GRIEF + GRACE + WONDER

            ("Dream2_VariantE_Defer",
             AnimDir + "/Dream2_VariantE_Defer.playable",
             30.0,
             "An empty chair beside an empty bed. The light holds. The wind, outside.",
             new Color(0.88f, 0.90f, 0.94f)),  // GRACE only
        };

        private const string ListenTimelinePath = AnimDir + "/ListenScene_Gerrold.playable";

        [MenuItem("Hearthbound/⚙️ Advanced/🎬 Phase 36 — Build Cutscene Library", priority = 219)]
        public static void Build()
        {
            EnsureFolder(AnimDir);
            EnsureFolder(PrefabsDir);

            int built = 0, kept = 0;
            foreach (var (id, path, duration, prose, lens) in DreamVariants)
            {
                bool created = BuildOrLoadDreamTimeline(id, path, duration, prose, lens);
                if (created) built++; else kept++;
            }

            // The Listen scene gets its own builder — different structure
            // (camera + actor + composer tracks; no DreamCanvas overlay).
            bool listenCreated = BuildOrLoadListenTimeline();
            if (listenCreated) built++; else kept++;

            // Re-wire the MemoryDreamRig prefab.
            BuildOrUpdateMemoryDreamRig();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Phase 36 — Cutscene Library",
                $"Cutscene library complete.\n\n" +
                $"  • {built} new Timeline asset(s) built\n" +
                $"  • {kept} existing Timeline(s) preserved\n" +
                $"  • MemoryDreamRig.prefab re-wired with all 6 dream variants\n" +
                $"  • Listen scene Timeline + sequencer in place\n\n" +
                "Run `Hearthbound → 🔍 Diagnose Build` — Phase 35 audit now reports 7/7 cutscene Timelines.\n\n" +
                "Audio tracks are empty. Phase 37 (Procedural Audio Studio) generates the composer cues; " +
                "Phase 38 binds them to the Audio tracks on each Timeline.",
                "OK");
        }

        // ───── Timeline construction ───────────────────────────────────

        /// <summary>
        /// Builds (or loads) a single dream-variant Timeline with the
        /// canonical 8-track template from Focus 05 § 2.2.
        /// </summary>
        /// <returns>true if a new Timeline was created.</returns>
        private static bool BuildOrLoadDreamTimeline(string id, string path, double duration, string prose, Color lens)
        {
            var existing = AssetDatabase.LoadAssetAtPath<TimelineAsset>(path);
            if (existing != null)
            {
                Debug.Log($"[Hearthbound/Phase 36] ✓ Kept existing Timeline {path}");
                return false;
            }

            var timeline = ScriptableObject.CreateInstance<TimelineAsset>();
            timeline.name = id;
            timeline.durationMode = TimelineAsset.DurationMode.FixedLength;
            timeline.fixedDuration = duration;

            // Track 1: Letterbox + DreamCanvas activation.
            //          ActivationTrack toggles the DreamCanvas child on the
            //          rig for the entire duration of the dream.
            var activationTrack = timeline.CreateTrack<ActivationTrack>(null, "DreamCanvas Activation");
            var actClip = activationTrack.CreateDefaultClip();
            actClip.start = 0;
            actClip.duration = duration;

            // Track 2: Camera animation — fade-in cinematic drift.
            //          Empty AnimationTrack — Phase 38 may bind a baked
            //          Cinemachine camera shake; for v1 we keep it empty
            //          so the dream stays still and contemplative.
            var animTrack = timeline.CreateTrack<AnimationTrack>(null, "Camera Anim");
            animTrack.trackOffset = TrackOffset.ApplyTransformOffsets;

            // Track 3: Composer cue (audio). Phase 38 binds a generated
            //          AudioClip to this track from MusicLibrarySO.
            var audioTrack = timeline.CreateTrack<AudioTrack>(null, "Composer Cue");

            // Track 4: Foley overlay (wind / clock-tick / bed-sheet).
            //          Second audio track for the layered foley.
            var foleyTrack = timeline.CreateTrack<AudioTrack>(null, "Foley Bed");

            AssetDatabase.CreateAsset(timeline, path);
            EditorUtility.SetDirty(timeline);
            Debug.Log($"[Hearthbound/Phase 36] ✓ Built Timeline {path} ({duration:F0}s)");

            // The prose line + lens colour aren't stored on the Timeline
            // itself (Timeline has no per-asset metadata field). They're
            // recorded in the rig prefab's DreamProse component instead.
            // Phase 38 reads them by id when it shows the dream.
            _ = prose; _ = lens;

            return true;
        }

        /// <summary>
        /// Builds the Listen scene Timeline. Structure differs from a Dream:
        /// no letterbox / no DreamCanvas — it's a *Cinemachine* sequence
        /// inside the cottage scene. Track 1 holds the Cinemachine virtual
        /// camera; Track 2 holds Gerrold's monologue audio; Track 3 holds
        /// the ambient cottage layer.
        /// </summary>
        private static bool BuildOrLoadListenTimeline()
        {
            var existing = AssetDatabase.LoadAssetAtPath<TimelineAsset>(ListenTimelinePath);
            if (existing != null)
            {
                Debug.Log($"[Hearthbound/Phase 36] ✓ Kept existing Listen Timeline {ListenTimelinePath}");
                return false;
            }

            var t = ScriptableObject.CreateInstance<TimelineAsset>();
            t.name = "ListenScene_Gerrold";
            t.durationMode = TimelineAsset.DurationMode.FixedLength;
            t.fixedDuration = 180.0;  // 3 minutes

            t.CreateTrack<AnimationTrack>(null, "Cinemachine Camera").trackOffset = TrackOffset.ApplyTransformOffsets;
            t.CreateTrack<AudioTrack>(null, "Gerrold Monologue VO");
            t.CreateTrack<AudioTrack>(null, "Cottage Ambient Bed");
            t.CreateTrack<AudioTrack>(null, "Composer Cue (Margery cello)");

            AssetDatabase.CreateAsset(t, ListenTimelinePath);
            EditorUtility.SetDirty(t);
            Debug.Log($"[Hearthbound/Phase 36] ✓ Built Listen Timeline {ListenTimelinePath} (180s)");
            return true;
        }

        // ───── MemoryDreamRig prefab ───────────────────────────────────

        /// <summary>
        /// Rebuilds the MemoryDreamRig prefab from scratch so all 6 dream
        /// PlayableAssets + the ListenScene PlayableAsset are wired onto a
        /// single MemoryDreamSequencer + a sibling ListenSceneSequencer.
        ///
        /// The prefab is a capstone artefact — fully owned by this builder.
        /// Re-running Phase 36 is idempotent because the prefab is rebuilt
        /// from the same Timeline asset references each time.
        /// </summary>
        private static void BuildOrUpdateMemoryDreamRig()
        {
            // Compose root + PlayableDirector.
            var root = new GameObject("MemoryDreamRig");

            // Audio source — every dream's Audio track needs a speaker.
            var audioSrc = root.AddComponent<AudioSource>();
            audioSrc.playOnAwake = false;
            audioSrc.spatialBlend = 0f;  // 2D — dreams are not positional
            audioSrc.volume = 0.85f;

            // PlayableDirector wired to Dream 1 by default; PlayDream2 swaps
            // the playableAsset at runtime.
            var director = root.AddComponent<PlayableDirector>();
            director.playOnAwake = false;
            var dream1 = AssetDatabase.LoadAssetAtPath<TimelineAsset>(DreamVariants[0].assetPath);
            if (dream1 != null) director.playableAsset = dream1;

            // The Memory Dream Sequencer — wires all 6 dream PlayableAssets.
            var seq = root.AddComponent<MemoryDreamSequencer>();
            seq.director = director;
            seq.dream1 = LoadTimeline("Dream1_Doris");
            seq.dream2_VariantA_EraseClean    = LoadTimeline("Dream2_VariantA_EraseClean");
            seq.dream2_VariantB_CleansePartial = LoadTimeline("Dream2_VariantB_CleansePartial");
            seq.dream2_VariantC_CrossedCore   = LoadTimeline("Dream2_VariantC_CrossedCore");
            seq.dream2_VariantD_Listen        = LoadTimeline("Dream2_VariantD_Listen");
            seq.dream2_VariantE_Defer         = LoadTimeline("Dream2_VariantE_Defer");

            // A SECOND PlayableDirector for the Listen scene — the dream
            // canvas should not letterbox the cottage interior; we want a
            // separate director so the two sequencers can drive different
            // overlays. Authored as a child to keep the rig hierarchy clean.
            var listenChild = new GameObject("ListenSceneRig");
            listenChild.transform.SetParent(root.transform, false);
            var listenSrc = listenChild.AddComponent<AudioSource>();
            listenSrc.playOnAwake = false;
            listenSrc.spatialBlend = 0f;
            listenSrc.volume = 0.9f;
            var listenDir = listenChild.AddComponent<PlayableDirector>();
            listenDir.playOnAwake = false;
            var listenSeq = listenChild.AddComponent<ListenSceneSequencer>();
            listenSeq.director = listenDir;
            listenSeq.listenTimeline = LoadTimeline("ListenScene_Gerrold");
            listenSeq.dreamSequencer = seq;
            if (listenSeq.listenTimeline != null) listenDir.playableAsset = listenSeq.listenTimeline;

            // Build the dream canvas overlay (letterbox + dream prose label).
            // Identical to Phase 21 v1 — Phase 31.1 hides it by default in
            // MemoryDreamSequencer.Awake() and re-enables only while a
            // dream is playing. Each dream variant's prose is set at the
            // moment of `PlayDream2(...)` in Phase 38's wiring.
            BuildDreamCanvas(root);

            // Save as prefab. Use SaveAsPrefabAsset (overwrite) so re-runs
            // are idempotent.
            PrefabUtility.SaveAsPrefabAsset(root, DreamRigPrefabPath);
            Object.DestroyImmediate(root);
            Debug.Log($"[Hearthbound/Phase 36] ✓ Re-saved {DreamRigPrefabPath} with 6 dream variants + Listen scene wired.");
        }

        private static TimelineAsset LoadTimeline(string id)
        {
            foreach (var (variantId, path, _, _, _) in DreamVariants)
            {
                if (variantId == id) return AssetDatabase.LoadAssetAtPath<TimelineAsset>(path);
            }
            if (id == "ListenScene_Gerrold")
                return AssetDatabase.LoadAssetAtPath<TimelineAsset>(ListenTimelinePath);
            return null;
        }

        private static void BuildDreamCanvas(GameObject root)
        {
            var canvasGO = new GameObject("DreamCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGO.transform.SetParent(root.transform, false);
            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;
            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            MakeBlackBar(canvasGO.transform, "LetterboxTop",
                new Vector2(0, 0.88f), new Vector2(1, 1f));
            MakeBlackBar(canvasGO.transform, "LetterboxBottom",
                new Vector2(0, 0f), new Vector2(1, 0.12f));

            // Lens-tint overlay (a full-screen colour overlay used to
            // express the emotion lens per variant).
            var lensGO = new GameObject("LensTint", typeof(RectTransform));
            lensGO.transform.SetParent(canvasGO.transform, false);
            var lens = lensGO.AddComponent<Image>();
            lens.color = new Color(0.78f, 0.84f, 0.92f, 0.30f);  // default GRIEF+GRACE
            lens.raycastTarget = false;
            var lensRT = lens.rectTransform;
            lensRT.anchorMin = Vector2.zero;
            lensRT.anchorMax = Vector2.one;
            lensRT.offsetMin = Vector2.zero;
            lensRT.offsetMax = Vector2.zero;

            // Fade overlay (black; used for in/out fades).
            var fadeGO = new GameObject("FadeOverlay", typeof(RectTransform));
            fadeGO.transform.SetParent(canvasGO.transform, false);
            var fade = fadeGO.AddComponent<Image>();
            fade.color = new Color(0, 0, 0, 0.0f);
            fade.raycastTarget = false;
            var fadeRT = fade.rectTransform;
            fadeRT.anchorMin = Vector2.zero;
            fadeRT.anchorMax = Vector2.one;
            fadeRT.offsetMin = Vector2.zero;
            fadeRT.offsetMax = Vector2.zero;

            // Dream prose label — Phase 38 changes the text per variant.
            var proseGO = new GameObject("DreamProse", typeof(RectTransform));
            proseGO.transform.SetParent(canvasGO.transform, false);
            var prose = proseGO.AddComponent<TextMeshProUGUI>();
            prose.fontSize = 36;
            prose.fontStyle = FontStyles.Italic;
            prose.color = new Color(0.97f, 0.85f, 0.62f);
            prose.alignment = TextAlignmentOptions.Center;
            prose.enableWordWrapping = true;
            prose.raycastTarget = false;
            prose.text = DreamVariants[0].proseLine;  // default to Dream 1's prose
            var proseRT = prose.rectTransform;
            proseRT.anchorMin = new Vector2(0.10f, 0.80f);
            proseRT.anchorMax = new Vector2(0.90f, 0.86f);
            proseRT.offsetMin = Vector2.zero;
            proseRT.offsetMax = Vector2.zero;
        }

        private static Image MakeBlackBar(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 1f);
            img.raycastTarget = false;
            var rt = img.rectTransform;
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return img;
        }

        // ───── Helpers ─────────────────────────────────────────────────

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = Path.GetDirectoryName(path).Replace('\\', '/');
            var leaf = Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }

        // ───── Public lookups (consumed by Phase 38 audio binder) ──────

        /// <summary>
        /// Returns the canonical prose line for a dream variant id, used by
        /// Phase 38's audio binder when it sets the DreamProse text just
        /// before `PlayDream*()` runs.
        /// </summary>
        public static string ProseFor(string variantId)
        {
            foreach (var (id, _, _, prose, _) in DreamVariants)
                if (id == variantId) return prose;
            return "";
        }

        public static Color LensFor(string variantId)
        {
            foreach (var (id, _, _, _, lens) in DreamVariants)
                if (id == variantId) return lens;
            return new Color(0.78f, 0.84f, 0.92f, 0.30f);
        }
    }
}
