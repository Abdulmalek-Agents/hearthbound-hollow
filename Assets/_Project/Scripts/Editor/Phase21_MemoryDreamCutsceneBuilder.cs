// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / Phase21_MemoryDreamCutsceneBuilder
//
// Phase 21 — Memory Dream Cutscenes.
//
// Builds a Timeline asset (`Dream1_Doris.playable`) + a prefab
// (`MemoryDreamRig.prefab`) that hosts:
//   • a PlayableDirector wired to the Timeline
//   • a MemoryDreamSequencer (HearthboundHollow.Cutscene)
//   • a black-letterbox UI overlay
//   • a TextMeshProUGUI for the dream's prose (Doris's "First Loaves" memory)
//   • a fullscreen Image used to fade in/out
//
// The Timeline has 3 tracks (Animation, Audio, Activation) authored
// procedurally with default clips matching the Mission_1_2_Focus/05 spec
// ("kitchen at first light" set-piece, ~60 s, JOY lens).
//
// At end-of-day in Mission01Director, the EveningLedger's
// OnEndOfDayConfirmed → could route through MemoryDreamSequencer.PlayDream1()
// before loading MainMenu. Phase 22 wires that hook.
//
// USE: Menu → Hearthbound → Phase 21 — Build Memory Dream Cutscene

using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using TMPro;
using HearthboundHollow.Cutscene;

namespace HearthboundHollow.EditorTools
{
    public static class Phase21_MemoryDreamCutsceneBuilder
    {
        private const string TimelinesDir = "Assets/_Project/Animations";
        private const string Dream1TimelinePath = TimelinesDir + "/Dream1_Doris.playable";

        private const string PrefabsDir = "Assets/_Project/Prefabs/Cutscene";
        private const string DreamRigPrefabPath = PrefabsDir + "/MemoryDreamRig.prefab";

        [MenuItem("Hearthbound/⚙️ Advanced/Phase 21 — Build Memory Dream Cutscene", priority = 208)]
        public static void Build()
        {
            EnsureFolder(TimelinesDir);
            EnsureFolder(PrefabsDir);

            var timeline = BuildOrLoadDream1Timeline();
            BuildDreamRigPrefab(timeline);

            EditorUtility.DisplayDialog(
                "Phase 21 — Done",
                $"Timeline: {Dream1TimelinePath}\n" +
                $"Prefab:   {DreamRigPrefabPath}\n\n" +
                "Re-run 'Hearthbound → Build Playable Mission 1 (One Click)' — the End-of-Day flow " +
                "will now play the Memory Dream before returning to MainMenu (Phase 22 wires the hook).",
                "OK");
        }

        // ─── Timeline asset ───────────────────────────────────────

        private static TimelineAsset BuildOrLoadDream1Timeline()
        {
            var existing = AssetDatabase.LoadAssetAtPath<TimelineAsset>(Dream1TimelinePath);
            if (existing != null) return existing;

            var timeline = ScriptableObject.CreateInstance<TimelineAsset>();
            timeline.name = "Dream1_Doris";
            timeline.durationMode = TimelineAsset.DurationMode.FixedLength;
            timeline.fixedDuration = 60.0; // 60 s per Mission_1_2_Focus § 05

            // Add a control track for activating the rig's letterbox + text reveal
            var activationTrack = timeline.CreateTrack<ActivationTrack>(null, "Letterbox Activation");
            var actClip = activationTrack.CreateDefaultClip();
            actClip.start = 0;
            actClip.duration = 60.0;

            // Animation track for camera moves (the actual Animator is wired in the prefab)
            var animTrack = timeline.CreateTrack<AnimationTrack>(null, "Camera Anim");
            animTrack.trackOffset = TrackOffset.ApplyTransformOffsets;

            // Audio track for the composer cue
            timeline.CreateTrack<AudioTrack>(null, "Composer Cue");

            AssetDatabase.CreateAsset(timeline, Dream1TimelinePath);
            Debug.Log($"[Hearthbound/Phase 21] (created timeline) {Dream1TimelinePath}");
            return timeline;
        }

        // ─── Rig prefab (PlayableDirector + UI) ─────────────────────────

        private static void BuildDreamRigPrefab(TimelineAsset timeline)
        {
            var root = new GameObject("MemoryDreamRig");

            // PlayableDirector
            var director = root.AddComponent<PlayableDirector>();
            director.playableAsset = timeline;
            director.playOnAwake = false;

            // Sequencer
            var seq = root.AddComponent<MemoryDreamSequencer>();
            seq.director = director;
            seq.dream1 = timeline;

            // Canvas overlay (letterbox + dream prose)
            var canvasGO = new GameObject("DreamCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGO.transform.SetParent(root.transform, false);
            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;
            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            // Top letterbox bar
            var topBar = MakeBlackBar(canvasGO.transform, "LetterboxTop",
                new Vector2(0, 0.88f), new Vector2(1, 1f));
            // Bottom letterbox bar
            var bottomBar = MakeBlackBar(canvasGO.transform, "LetterboxBottom",
                new Vector2(0, 0f), new Vector2(1, 0.12f));

            // Fade overlay (black)
            var fadeGO = new GameObject("FadeOverlay", typeof(RectTransform));
            fadeGO.transform.SetParent(canvasGO.transform, false);
            var fade = fadeGO.AddComponent<Image>();
            fade.color = new Color(0, 0, 0, 0.0f);
            var fadeRT = fade.rectTransform;
            fadeRT.anchorMin = Vector2.zero; fadeRT.anchorMax = Vector2.one;
            fadeRT.offsetMin = Vector2.zero; fadeRT.offsetMax = Vector2.zero;

            // Dream prose label
            var proseGO = new GameObject("DreamProse", typeof(RectTransform));
            proseGO.transform.SetParent(canvasGO.transform, false);
            var prose = proseGO.AddComponent<TextMeshProUGUI>();
            prose.fontSize = 36;
            prose.fontStyle = FontStyles.Italic;
            prose.color = new Color(0.97f, 0.85f, 0.62f);
            prose.alignment = TextAlignmentOptions.Center;
            prose.enableWordWrapping = true;
            prose.text = "The kitchen at first light. Flour on the table. Her mother's apron on the hook.";
            var proseRT = prose.rectTransform;
            proseRT.anchorMin = new Vector2(0.15f, 0.78f); proseRT.anchorMax = new Vector2(0.85f, 0.86f);
            proseRT.offsetMin = Vector2.zero; proseRT.offsetMax = Vector2.zero;

            // Save the rig prefab
            PrefabUtility.SaveAsPrefabAsset(root, DreamRigPrefabPath);
            Object.DestroyImmediate(root);
            Debug.Log($"[Hearthbound/Phase 21] (created rig) {DreamRigPrefabPath}");
        }

        private static Image MakeBlackBar(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 1f);
            var rt = img.rectTransform;
            rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            return img;
        }

        // ─── Folder helpers ──────────────────────────────────────

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = Path.GetDirectoryName(path).Replace('\\', '/');
            var leaf = Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }

        public static GameObject TryGetDreamRigPrefab() =>
            AssetDatabase.LoadAssetAtPath<GameObject>(DreamRigPrefabPath);
    }
}
