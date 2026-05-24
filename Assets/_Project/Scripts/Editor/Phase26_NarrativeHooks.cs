// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / Phase26_NarrativeHooks
//
// Phase 26 — wires runtime narrative hooks into existing scenes without
// rebuilding them. Current hooks:
//   1. Marin's Note on the workbench in 03_Mission01_Hollow.
//
// Idempotent — safe to re-run. Each hook checks whether its target is
// already present and skips if so. Intended to be called after Phase 23
// (which builds the polished scenes) and before the player presses Play.
//
// USE: Menu → Hearthbound → Phase 26 — Wire Narrative Hooks

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HearthboundHollow.Player;
using HearthboundHollow.UI;

namespace HearthboundHollow.EditorTools
{
    public static class Phase26_NarrativeHooks
    {
        private const string SceneHollow = "Assets/_Project/Scenes/03_Mission01_Hollow.unity";

        [MenuItem("Hearthbound/Phase 26 — Wire Narrative Hooks", priority = 5)]
        public static void Wire()
        {
            if (!System.IO.File.Exists(SceneHollow))
            {
                EditorUtility.DisplayDialog("Phase 26",
                    "03_Mission01_Hollow.unity is missing. Run Phase 23 first.", "OK");
                return;
            }

            var scene = EditorSceneManager.OpenScene(SceneHollow, OpenSceneMode.Single);
            bool added = AddMarinNoteToWorkbench();

            if (added)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }

            EditorUtility.DisplayDialog("Phase 26 — Narrative Hooks",
                added
                    ? "✨ Marin's Note added to the workbench in the Hollow.\n\n" +
                      "Press Play → enter the Hollow → walk to the workbench → press E.\n" +
                      "Read all 4 passages to set VillageState.readMarinNoteIds " +
                      "and nudge predecessorTrailWarmth +5."
                    : "Marin's Note was already present — nothing to do.",
                "OK");
        }

        // ---- Marin's Note wiring ----

        private static bool AddMarinNoteToWorkbench()
        {
            // Skip if already present.
            if (Object.FindFirstObjectByType<MarinNoteInteractable>() != null)
            {
                Debug.Log("[Hearthbound/Phase 26] Marin's Note already present — skipping.");
                return false;
            }

            var workbench = GameObject.Find("Workbench");
            if (workbench == null)
            {
                Debug.LogWarning("[Hearthbound/Phase 26] Workbench not found in Hollow scene. " +
                                 "Run Phase 23 to rebuild the scene first.");
                return false;
            }

            // Build a small "folded parchment" visual — a thin warm-yellow quad
            // on the workbench surface. The real visual polish comes when we
            // swap in a Bamao note prefab in a later phase; this is a
            // placeholder that reads correctly from any camera angle.
            var noteGO = GameObject.CreatePrimitive(PrimitiveType.Quad);
            noteGO.name = "MarinsNote";
            noteGO.transform.SetParent(workbench.transform, false);
            // Lay it flat on top of the workbench. Workbench is at y=0.5 with
            // localScale.y=1, so the top is at y=1. Slightly raise to avoid z-fight.
            noteGO.transform.localPosition = new Vector3(-0.30f, 0.52f, 0.10f);
            noteGO.transform.localRotation = Quaternion.Euler(90f, 18f, 0f);
            noteGO.transform.localScale = new Vector3(0.55f, 0.35f, 1f);

            // Warm parchment colour. Use URP/Lit if present, else Standard.
            var quadMR = noteGO.GetComponent<MeshRenderer>();
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            var noteMat = new Material(shader);
            noteMat.SetColor("_BaseColor", new Color(0.96f, 0.86f, 0.58f));
            noteMat.SetColor("_Color", new Color(0.96f, 0.86f, 0.58f));
            noteMat.SetFloat("_Smoothness", 0.10f);
            quadMR.sharedMaterial = noteMat;

            // Replace the quad collider with a small box so the interaction
            // raycast hits something larger than the visual.
            var quadCol = noteGO.GetComponent<MeshCollider>();
            if (quadCol != null) Object.DestroyImmediate(quadCol);
            var box = noteGO.AddComponent<BoxCollider>();
            box.size = new Vector3(1.2f, 1.2f, 0.4f);
            box.isTrigger = false;

            // The interactable script + wire its DialogueUI reference.
            var note = noteGO.AddComponent<MarinNoteInteractable>();
            note.noteId = "MARIN_NOTE_01_OPENING";
            note.speakerLabel = "Marin's Note";
            note.dialogueUI = Object.FindFirstObjectByType<DialogueUI>();

            // A small floating sigil tag just above the note so the player
            // notices it. Uses TextMeshPro 3D text (so it follows the world).
            var tagGO = new GameObject("NoticeTag");
            tagGO.transform.SetParent(noteGO.transform, false);
            tagGO.transform.localPosition = new Vector3(0f, 0.05f, -0.6f);
            tagGO.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            tagGO.transform.localScale = Vector3.one * 0.6f;
            var tag = tagGO.AddComponent<TextMeshPro>();
            tag.text = "✨ a note in Marin's hand";
            tag.fontSize = 2.2f;
            tag.alignment = TextAlignmentOptions.Center;
            tag.color = new Color(0.97f, 0.85f, 0.62f);

            Debug.Log("[Hearthbound/Phase 26] Marin's Note added to workbench — noteId=MARIN_NOTE_01_OPENING.");
            return true;
        }
    }
}
