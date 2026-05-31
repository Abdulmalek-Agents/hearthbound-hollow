// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / Phase55_EnvironmentPlacementAudit
//
// PHASE 54/55 (Phase D — environment + asset-placement accuracy).
//
// The QA video review flagged three placement issues in the Hollow interior:
//   • red placeholder QUADS on the floor (trigger-zone visualizers left visible),
//   • an OVERSIZED fire/flame VFX floating mid-air,
//   • a NOTE prop reading as a floating sprite.
//
// Rather than blindly rewrite scenes, this tool gives the in-Editor QA +
// Technical-Artist team a precise, actionable punch-list (read-only Audit) AND a
// conservative, opt-in auto-fix for the items we can correct with high confidence
// (hiding renderers on named trigger zones). Both are idempotent and menu-driven —
// nothing runs automatically, nothing is ever deleted.
//
// Detected per gameplay scene (00→05):
//   1. Trigger-zone objects (name contains Zone/Trigger/Approach/Ambiance/Marker)
//      that still have an ENABLED MeshRenderer  → the red quads. (Fixable.)
//   2. Renderers whose shared material looks like a placeholder (shader null /
//      Hidden/InternalErrorShader / "red"/"magenta" named).
//   3. Objects floating well above the nearest surface below them (raycast gap
//      > 0.75 m) — likely notes / props / VFX that need grounding.
//   4. Objects with an extreme lossyScale (> 4 on any axis) — likely a mis-scaled
//      VFX (e.g. the oversized flame).
//   5. Visible mesh renderers with NO Collider (candidates for collision/grounding).
//
// Usage:
//   Hearthbound → ⚙️ Advanced → Phase 55 — Audit Environment Placement (read-only)
//   Hearthbound → ⚙️ Advanced → Phase 55 — Fix Trigger-Zone Quads (safe, opt-in)

using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HearthboundHollow.EditorTools
{
    public static class Phase55_EnvironmentPlacementAudit
    {
        private static readonly string[] GameplayScenes =
        {
            "Assets/_Project/Scenes/02_Mission01_Lane.unity",
            "Assets/_Project/Scenes/03_Mission01_Hollow.unity",
            "Assets/_Project/Scenes/04_Mission02_Garden.unity",
            "Assets/_Project/Scenes/05_Mission02_Cottage.unity",
        };

        private static readonly string[] ZoneKeywords =
            { "Zone", "Trigger", "Approach", "Ambiance", "Ambience", "Marker", "Volume" };

        [MenuItem("Hearthbound/⚙️ Advanced/Phase 55 — Audit Environment Placement (read-only)")]
        public static void Audit()
        {
            var report = new StringBuilder();
            report.AppendLine("═══ Phase 55 — Environment Placement Audit ═══");
            string original = EditorSceneManager.GetActiveScene().path;

            int totalQuads = 0, totalFloating = 0, totalScale = 0, totalNoCollider = 0, totalPlaceholder = 0;

            foreach (var scenePath in GameplayScenes)
            {
                if (!System.IO.File.Exists(scenePath)) continue;
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                report.AppendLine($"\n── {System.IO.Path.GetFileNameWithoutExtension(scenePath)} ──");

                foreach (var root in scene.GetRootGameObjects())
                {
                    foreach (var mr in root.GetComponentsInChildren<MeshRenderer>(true))
                    {
                        var go = mr.gameObject;

                        // 1) Visible renderer on a named trigger zone → red quad.
                        if (mr.enabled && IsZoneName(go.name) && go.GetComponent<Collider>() != null)
                        {
                            totalQuads++;
                            report.AppendLine($"  [QUAD]   {Path(go)} — visible renderer on a trigger zone (hide it).");
                        }

                        // 2) Placeholder-looking material.
                        if (LooksPlaceholder(mr.sharedMaterial))
                        {
                            totalPlaceholder++;
                            report.AppendLine($"  [MAT]    {Path(go)} — placeholder/unsupported material '{MatName(mr.sharedMaterial)}'.");
                        }

                        // 5) Visible mesh with no Collider.
                        if (mr.enabled && go.GetComponent<Collider>() == null && !IsZoneName(go.name))
                        {
                            totalNoCollider++;
                            if (totalNoCollider <= 40)
                                report.AppendLine($"  [NOCOL]  {Path(go)} — visible mesh with no Collider.");
                        }
                    }

                    // 3) + 4) Floating / mis-scaled transforms (renderers only).
                    foreach (var rend in root.GetComponentsInChildren<Renderer>(true))
                    {
                        var t = rend.transform;
                        Vector3 s = t.lossyScale;
                        if (Mathf.Max(Mathf.Abs(s.x), Mathf.Abs(s.y), Mathf.Abs(s.z)) > 4f)
                        {
                            totalScale++;
                            report.AppendLine($"  [SCALE]  {Path(rend.gameObject)} — extreme lossyScale {s} (mis-scaled VFX?).");
                        }

                        float gap = GroundGap(rend);
                        if (gap > 0.75f)
                        {
                            totalFloating++;
                            report.AppendLine($"  [FLOAT]  {Path(rend.gameObject)} — {gap:F2} m above the surface below (grounding?).");
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(original)) EditorSceneManager.OpenScene(original, OpenSceneMode.Single);

            report.Insert(0, $"\nSUMMARY: {totalQuads} zone-quads · {totalFloating} floating · " +
                             $"{totalScale} mis-scaled · {totalPlaceholder} placeholder-mat · {totalNoCollider} no-collider\n");
            Debug.Log(report.ToString());
            EditorUtility.DisplayDialog("Phase 55 — Environment Audit",
                $"Audit complete (read-only). See the Console for the full punch-list.\n\n" +
                $"Zone quads: {totalQuads}\nFloating: {totalFloating}\nMis-scaled: {totalScale}\n" +
                $"Placeholder materials: {totalPlaceholder}\nNo-collider meshes: {totalNoCollider}", "OK");
        }

        [MenuItem("Hearthbound/⚙️ Advanced/Phase 55 — Fix Trigger-Zone Quads (safe, opt-in)")]
        public static void FixTriggerZoneQuads()
        {
            string original = EditorSceneManager.GetActiveScene().path;
            int fixedCount = 0;

            foreach (var scenePath in GameplayScenes)
            {
                if (!System.IO.File.Exists(scenePath)) continue;
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                bool dirty = false;

                foreach (var root in scene.GetRootGameObjects())
                foreach (var mr in root.GetComponentsInChildren<MeshRenderer>(true))
                {
                    var go = mr.gameObject;
                    // Only act on clearly-named trigger zones that also carry a
                    // trigger Collider — these are gameplay volumes that must be
                    // invisible. Disabling the renderer kills the red placeholder
                    // quad with zero gameplay impact.
                    var col = go.GetComponent<Collider>();
                    if (mr.enabled && IsZoneName(go.name) && col != null && col.isTrigger)
                    {
                        mr.enabled = false;
                        EditorUtility.SetDirty(mr);
                        fixedCount++;
                        dirty = true;
                        Debug.Log($"[Phase55] Hid trigger-zone renderer: {Path(go)}");
                    }
                }

                if (dirty)
                {
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                }
            }

            if (!string.IsNullOrEmpty(original)) EditorSceneManager.OpenScene(original, OpenSceneMode.Single);
            EditorUtility.DisplayDialog("Phase 55 — Fix Trigger-Zone Quads",
                $"Hid {fixedCount} trigger-zone renderer(s) (the red floor quads).\n\n" +
                "Re-run the Audit for the remaining (judgement-call) items: floating props, " +
                "mis-scaled VFX, and no-collider meshes — dress those by hand for accuracy.", "OK");
        }

        // ───── helpers ─────────────────────────────────────

        private static bool IsZoneName(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            foreach (var k in ZoneKeywords)
                if (name.IndexOf(k, System.StringComparison.OrdinalIgnoreCase) >= 0) return true;
            return false;
        }

        private static bool LooksPlaceholder(Material m)
        {
            if (m == null) return true;
            if (m.shader == null) return true;
            string sh = m.shader.name ?? "";
            if (sh.Contains("InternalErrorShader") || sh.Contains("Hidden/")) return true;
            string n = (m.name ?? "").ToLowerInvariant();
            return n.Contains("placeholder") || n.Contains("magenta") || n == "red" || n.Contains("debug");
        }

        private static string MatName(Material m) => m == null ? "<null>" : (m.name ?? "<unnamed>");

        // Distance from the renderer's bounds-bottom to the first collider below it.
        private static float GroundGap(Renderer rend)
        {
            Vector3 bottom = rend.bounds.center;
            bottom.y = rend.bounds.min.y;
            // Start slightly inside the bounds and cast down.
            if (Physics.Raycast(bottom + Vector3.up * 0.05f, Vector3.down, out RaycastHit hit, 50f,
                                ~0, QueryTriggerInteraction.Ignore))
                return Mathf.Max(0f, hit.distance - 0.05f);
            return 0f; // nothing below / unbounded — don't flag
        }

        private static string Path(GameObject go)
        {
            var sb = new StringBuilder(go.name);
            var t = go.transform.parent;
            while (t != null) { sb.Insert(0, t.name + "/"); t = t.parent; }
            return sb.ToString();
        }
    }
}
