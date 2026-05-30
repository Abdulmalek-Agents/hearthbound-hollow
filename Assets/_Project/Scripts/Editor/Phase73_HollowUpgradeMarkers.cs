// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / Phase73_HollowUpgradeMarkers
//
// PHASE 73 — "buying an upgrade visibly changes the Hollow" (Depth-audit
// Engagement-Engine-3 hardening). Pre-places each built-in HollowCatalog
// upgrade's cozy prop in the Hollow scene, HIDDEN (inactive) + tagged with a
// HollowUpgradeMarker carrying the upgrade's sceneMarkerId. When the player buys
// the upgrade, HollowProgressionService finds the (inactive) marker by component
// and activates it — so the shop purchase becomes a real object that appears in
// the room and persists across save/load (ReapplyAll on scene load).
//
// Markers mirror HollowProgressionService's built-in starter catalog ids:
//   _HollowUpgrade_ShelfWindow · _HollowUpgrade_ShelfHearth ·
//   _HollowUpgrade_Upstairs · _HollowUpgrade_GardenBed · _HollowUpgrade_Cushion
//
// FULLY DEFENSIVE + REVERSIBLE: one managed root "_Phase73_UpgradeMarkers" you
// can delete to revert; each prop raycast-grounded; missing-prefab safe (falls
// back to a small primitive); every marker starts INACTIVE so nothing shows
// until earned. Chained into 🚀 Build Everything (Step 23). Authored-catalog
// markers (if a HollowCatalog SO ships) are the designer's responsibility; this
// builder covers the built-in set that ships today.

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using HearthboundHollow.Mission;

namespace HearthboundHollow.EditorTools
{
    public static class Phase73_HollowUpgradeMarkers
    {
        private const string MV          = "Assets/MeshingunStudio";
        private const string HollowScene = "Assets/_Project/Scenes/03_Mission01_Hollow.unity";
        private const string Root        = "_Phase73_UpgradeMarkers";

        [MenuItem("Hearthbound/⚙️ Advanced/🪑 Phase 73 — Hollow Upgrade Markers", priority = 650)]
        public static void Build()
        {
            if (!System.IO.File.Exists(HollowScene)) { Debug.LogWarning("[Phase73] Hollow scene missing — skip."); return; }
            string original = EditorSceneManager.GetActiveScene().path;
            var scene = EditorSceneManager.OpenScene(HollowScene, OpenSceneMode.Single);

            var existing = FindRoot(scene, Root);
            if (existing != null) Object.DestroyImmediate(existing);
            var root = new GameObject(Root);
            SceneManager.MoveGameObjectToScene(root, scene);
            root.transform.position = Vector3.zero;

            float gy = ProbeGroundY(scene);
            int n = 0;

            // (markerId, prop keywords, local position, yaw, scale, isLight)
            n += Place(root.transform, "_HollowUpgrade_ShelfWindow", new[] { "Cupboard_01a", "Cupboard", "Shelf" },
                       new Vector3(-2.6f, 0f, 2.4f),  95f, 1f, gy, light: false);
            n += Place(root.transform, "_HollowUpgrade_ShelfHearth", new[] { "Cupboard_01b", "Cupboard", "Shelf" },
                       new Vector3( 2.6f, 0f, 2.2f), -95f, 1f, gy, light: false);
            n += Place(root.transform, "_HollowUpgrade_GardenBed",   new[] { "TerraPots_01a", "TerraPots", "Pot" },
                       new Vector3(-2.7f, 0f, -1.6f),  0f, 1.3f, gy, light: false);
            n += Place(root.transform, "_HollowUpgrade_Cushion",     new[] { "Stool_01a", "Stool", "Chair" },
                       new Vector3( 1.7f, 0f, -1.1f), 20f, 1f, gy, light: false);
            // The "relight the upstairs" upgrade is a warm glow, not a prop.
            n += Place(root.transform, "_HollowUpgrade_Upstairs",    null,
                       new Vector3( 0f, 3.1f, -0.2f),  0f, 1f, gy, light: true);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            if (!string.IsNullOrEmpty(original) && System.IO.File.Exists(original))
                EditorSceneManager.OpenScene(original, OpenSceneMode.Single);

            Debug.Log($"[Phase73] Placed {n} hidden Hollow upgrade marker(s). Delete '{Root}' to revert.");
        }

        private static int Place(Transform parent, string markerId, string[] propKw,
                                 Vector3 pos, float yaw, float scale, float groundY, bool light)
        {
            // The marker GameObject IS the thing that gets toggled; it carries the tag.
            var marker = new GameObject(markerId);
            marker.transform.SetParent(parent, false);
            marker.transform.localPosition = pos;
            marker.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            marker.AddComponent<HollowUpgradeMarker>().markerId = markerId;

            try
            {
                if (light)
                {
                    var l = marker.AddComponent<Light>();
                    l.type = LightType.Point; l.color = new Color(1f, 0.85f, 0.6f);
                    l.range = 8f; l.intensity = 1.6f; l.shadows = LightShadows.None;
                }
                else
                {
                    var prefab = FindPrefab(propKw);
                    if (prefab != null)
                    {
                        var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                        inst.name = markerId + "_Prop";
                        inst.transform.SetParent(marker.transform, false);
                        inst.transform.localScale = Vector3.one * scale;
                        GroundToFloor(inst.transform, marker.transform.position.y, groundY);
                        EnsureCollider(inst);
                    }
                    else
                    {
                        // Fallback so the upgrade still reveals *something* warm.
                        var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        box.name = markerId + "_Box";
                        box.transform.SetParent(marker.transform, false);
                        box.transform.localScale = new Vector3(0.6f, 0.3f, 0.6f);
                    }
                }
            }
            catch (System.Exception ex) { Debug.LogWarning($"[Phase73] {markerId}: {ex.Message}"); }

            marker.SetActive(false);   // hidden until purchased
            return 1;
        }

        private static void GroundToFloor(Transform t, float markerWorldY, float fallbackY)
        {
            var rends = t.GetComponentsInChildren<Renderer>(true);
            if (rends.Length == 0) return;
            Bounds b = rends[0].bounds;
            for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);
            // Seat the prop base on the floor under the marker.
            Vector3 from = new Vector3(b.center.x, b.max.y + 0.5f, b.center.z);
            float floorY = Physics.Raycast(from, Vector3.down, out RaycastHit hit, 60f, ~0, QueryTriggerInteraction.Ignore)
                         ? hit.point.y : fallbackY;
            t.position += new Vector3(0f, (floorY + 0.01f) - b.min.y, 0f);
        }

        private static void EnsureCollider(GameObject go)
        {
            if (go.GetComponentInChildren<Collider>(true) != null) return;
            var rends = go.GetComponentsInChildren<Renderer>(true);
            if (rends.Length == 0) return;
            Bounds b = rends[0].bounds;
            for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);
            var bc = go.AddComponent<BoxCollider>();
            bc.center = go.transform.InverseTransformPoint(b.center);
            bc.size = new Vector3(
                b.size.x / Mathf.Max(0.0001f, go.transform.lossyScale.x),
                b.size.y / Mathf.Max(0.0001f, go.transform.lossyScale.y),
                b.size.z / Mathf.Max(0.0001f, go.transform.lossyScale.z));
        }

        private static GameObject FindPrefab(string[] keywords)
        {
            if (keywords == null || !System.IO.Directory.Exists(MV)) return null;
            GameObject best = null; int bestScore = 0;
            foreach (var g in AssetDatabase.FindAssets("t:Prefab", new[] { MV }))
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                var lower = path.ToLowerInvariant();
                if (lower.Contains("/scenes/") || lower.Contains("/demo") || lower.Contains("preview")) continue;
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;
                int score = 0; string pn = prefab.name.ToLowerInvariant();
                for (int i = 0; i < keywords.Length; i++)
                {
                    string k = keywords[i].ToLowerInvariant();
                    if (pn.Contains(k)) score += (keywords.Length - i) * 20;
                    else if (lower.Contains(k)) score += (keywords.Length - i) * 8;
                }
                if (score > bestScore) { best = prefab; bestScore = score; }
            }
            return best;
        }

        private static float ProbeGroundY(Scene scene)
        {
            if (Physics.Raycast(new Vector3(0f, 20f, 0f), Vector3.down, out RaycastHit hit, 80f, ~0, QueryTriggerInteraction.Ignore))
                return hit.point.y;
            foreach (var nm in new[] { "WoodFloor", "Ground", "Floor" })
            {
                var t = FindDeep(scene, nm);
                if (t != null) return t.position.y;
            }
            return 0f;
        }

        private static GameObject FindRoot(Scene scene, string name)
        {
            foreach (var r in scene.GetRootGameObjects())
                if (r.name == name) return r;
            return null;
        }

        private static Transform FindDeep(Scene scene, string name)
        {
            foreach (var r in scene.GetRootGameObjects())
                foreach (var t in r.GetComponentsInChildren<Transform>(true))
                    if (t.name == name) return t;
            return null;
        }
    }
}
