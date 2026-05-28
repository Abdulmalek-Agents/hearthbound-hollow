// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / Phase49_EchoHologramBuilder
//
// PHASE 49 — One-click installer for the first Echo Hologram of Marin.
//
// Drops `_EchoHologram_Marin01` into the Hollow scene (03_Mission01_Hollow),
// next to the workbench where Phase 26 already placed Marin's parchment
// Note. The hologram has:
//
//   - a translucent silhouette UI overlay (procedural soft-radial sprite,
//     placed on a world-space Canvas attached to the workbench)
//   - a soft Point Light tinted Marin's signature pale-blue (#9DB6CB)
//   - a BoxCollider trigger that fires the player's interact prompt
//
// On first Activate(), it plays the 3-line Vellis-tier monologue via
// DialogueUI + VoicePlayer (lineIds echo_marin_welcome_01/02/03), bumps
// predecessorTrailWarmth, and persists `echoHologramHeard = true`.
//
// Idempotent — re-running deletes the previously-built node (matched by
// name) and rebuilds it. Falls back gracefully when the Hollow scene
// hasn't been built yet (Phase 23 prerequisite).

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using HearthboundHollow.Mission;
using HearthboundHollow.UI;

namespace HearthboundHollow.EditorTools
{
    public static class Phase49_EchoHologramBuilder
    {
        private const string HollowScenePath = "Assets/_Project/Scenes/03_Mission01_Hollow.unity";
        private const string EchoNodeName    = "_EchoHologram_Marin01";
        private const string AnchorNameHint  = "Workbench"; // best-effort anchor in Hollow

        [MenuItem("Hearthbound/⚙️ Advanced/👻 Phase 49 — Wire Echo Hologram (Marin)", priority = 4900)]
        public static void Build()
        {
            if (!System.IO.File.Exists(HollowScenePath))
            {
                EditorUtility.DisplayDialog(
                    "Phase 49 — Echo Hologram",
                    "Hollow scene not found at:\n" + HollowScenePath +
                    "\n\nRun 🚀 Build Everything first.", "OK");
                return;
            }

            var scene = EditorSceneManager.GetSceneByPath(HollowScenePath);
            if (!scene.IsValid() || !scene.isLoaded)
            {
                scene = EditorSceneManager.OpenScene(HollowScenePath, OpenSceneMode.Single);
            }

            RemoveOld(scene);
            var node = BuildHologramNode(scene);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            EditorUtility.DisplayDialog(
                "Phase 49 — Echo Hologram",
                "✓ Marin's Echo Hologram installed on the Hollow workbench.\n\n" +
                "Walk up + press E in play mode → Marin speaks the 3-line welcome.\n" +
                "Subsequent listens: faint hum-only confirmation.\n\n" +
                "Persists `echoHologramHeard = true` and bumps " +
                "predecessorTrailWarmth by +12.",
                "OK");
        }

        private static void RemoveOld(UnityEngine.SceneManagement.Scene scene)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.name == EchoNodeName)
                {
                    Object.DestroyImmediate(root);
                    continue;
                }
                // Also clean up nested copies under any anchor.
                var stale = root.transform.Find(EchoNodeName);
                if (stale != null) Object.DestroyImmediate(stale.gameObject);
            }
        }

        private static GameObject BuildHologramNode(UnityEngine.SceneManagement.Scene scene)
        {
            // 1) Find a workbench-ish anchor in the scene; fall back to
            //    world origin at +1.2m if nothing matches.
            var anchor = FindAnchor(scene);
            var pos = anchor != null
                ? anchor.position + Vector3.up * 1.2f
                : new Vector3(0f, 1.2f, 0f);

            var root = new GameObject(EchoNodeName);
            root.transform.position = pos;
            SceneManager.MoveGameObjectToScene(root, scene);
            if (anchor != null) root.transform.SetParent(anchor, true);

            // 2) BoxCollider trigger for player interaction range.
            var col = root.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(1.6f, 1.6f, 1.6f);

            // 3) Soft Point Light, Marin's pale-blue.
            var lightGO = new GameObject("_HologramLight");
            lightGO.transform.SetParent(root.transform, false);
            lightGO.transform.localPosition = new Vector3(0f, 0.6f, 0f);
            var light = lightGO.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.62f, 0.71f, 0.80f, 1f);
            light.intensity = 1.4f;
            light.range = 3.2f;
            light.enabled = false;

            // 4) World-space Canvas for the hologram silhouette.
            var canvasGO = new GameObject("_HologramCanvas");
            canvasGO.transform.SetParent(root.transform, false);
            canvasGO.transform.localPosition = new Vector3(0f, 0.3f, 0f);
            canvasGO.transform.localRotation = Quaternion.identity;
            canvasGO.transform.localScale = new Vector3(0.005f, 0.005f, 0.005f);
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 100;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();

            var group = canvasGO.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            group.blocksRaycasts = false;

            var silhouetteGO = new GameObject("_Silhouette", typeof(RectTransform));
            silhouetteGO.transform.SetParent(canvasGO.transform, false);
            var rt = silhouetteGO.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(400, 700);
            var img = silhouetteGO.AddComponent<Image>();
            img.color = new Color(0.62f, 0.71f, 0.80f, 0.78f);
            img.sprite = BuildSilhouetteSprite();
            img.raycastTarget = false;
            silhouetteGO.SetActive(false);

            // 5) The actual interactable script.
            var interactable = root.AddComponent<EchoHologramInteractable>();
            interactable.hologramVisual = img;
            interactable.hologramLight = light;
            interactable.hologramGroup = group;
            interactable.dialogueUI = Object.FindFirstObjectByType<DialogueUI>();

            return root;
        }

        private static Transform FindAnchor(UnityEngine.SceneManagement.Scene scene)
        {
            // Look for any transform with "Workbench" in the name (case-
            // insensitive), at any depth. First hit wins.
            foreach (var root in scene.GetRootGameObjects())
            {
                var hit = SearchByName(root.transform, AnchorNameHint);
                if (hit != null) return hit;
            }
            return null;
        }

        private static Transform SearchByName(Transform t, string needle)
        {
            if (t.name.IndexOf(needle, System.StringComparison.OrdinalIgnoreCase) >= 0)
                return t;
            for (int i = 0; i < t.childCount; i++)
            {
                var hit = SearchByName(t.GetChild(i), needle);
                if (hit != null) return hit;
            }
            return null;
        }

        private static Sprite BuildSilhouetteSprite()
        {
            // Tall, narrow human-ish silhouette — vertical gradient with
            // soft edges. Used as a single Image so we don't need a mesh.
            int W = 64, H = 112;
            var tex = new Texture2D(W, H, TextureFormat.RGBA32, false);
            tex.name = "EchoHologramSilhouette";
            var px = new Color[W * H];
            float cx = W * 0.5f;
            for (int y = 0; y < H; y++)
            {
                float ny = y / (float)(H - 1); // 0 bottom → 1 top
                // shoulders bulge mid-way; head pinch at top.
                float w;
                if (ny > 0.85f) w = Mathf.Lerp(0.18f, 0.10f, (ny - 0.85f) / 0.15f);
                else if (ny > 0.70f) w = 0.18f;
                else if (ny > 0.55f) w = Mathf.Lerp(0.32f, 0.18f, (0.70f - ny) / 0.15f);
                else                 w = Mathf.Lerp(0.28f, 0.32f, ny / 0.55f);

                // alpha fade from feet to head end (legs more transparent).
                float vAlpha = Mathf.Lerp(0.4f, 1f, Mathf.Clamp01(ny));

                for (int x = 0; x < W; x++)
                {
                    float dx = Mathf.Abs(x - cx) / cx;
                    float a = Mathf.Clamp01(1f - dx / Mathf.Max(0.001f, w));
                    a = a * a;
                    px[y * W + x] = new Color(1f, 1f, 1f, a * vAlpha);
                }
            }
            tex.SetPixels(px);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, W, H), new Vector2(0.5f, 0f));
        }
    }
}
