// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Mission / CharacterAppearance
//
// Phase 53 — applies the player's chosen look (skin tone, outfit colour,
// accessory) to the in-world avatar. Choices are authored in CharacterCreationUI
// and persisted in SettingsService (PlayerPrefs); this component reads them on
// every gameplay scene load and tints the player's renderers + spawns a small
// procedural accessory. All-procedural — no new art assets required (D-066).
//
//   • Outfit colour   — tints the player's body renderers (always visible).
//   • Skin tone       — tints material slots whose name looks like skin
//                        (best-effort; outfit is the guaranteed-visible knob).
//   • Accessory       — a tiny code-built cap / flower / scarf on the head.
//
// Robust: finds the player by tag "Player" with a short retry window (the
// player rig may finish spawning a frame or two after this Start()).

using System.Collections;
using UnityEngine;
using HearthboundHollow.Core;

namespace HearthboundHollow.Mission
{
    public static class CharacterPalette
    {
        // Warm, cozy, inclusive ranges. Indices are persisted in SettingsService.
        public static readonly Color[] Skin =
        {
            new Color(0.96f, 0.84f, 0.72f), // porcelain
            new Color(0.92f, 0.76f, 0.62f), // light
            new Color(0.83f, 0.64f, 0.48f), // tan
            new Color(0.65f, 0.46f, 0.33f), // brown
            new Color(0.45f, 0.31f, 0.22f), // deep
            new Color(0.30f, 0.20f, 0.15f), // ebony
        };

        public static readonly Color[] Outfit =
        {
            new Color(0.66f, 0.32f, 0.27f), // hearth red
            new Color(0.36f, 0.45f, 0.55f), // dusk blue
            new Color(0.45f, 0.52f, 0.34f), // sage
            new Color(0.74f, 0.60f, 0.32f), // ochre
            new Color(0.52f, 0.40f, 0.58f), // plum
            new Color(0.86f, 0.83f, 0.76f), // cream
        };

        // Accessory 0 = none. 1..3 = cap / flower / scarf.
        public const int AccessoryCount = 4;

        public static Color Clamp(Color[] arr, int i) => arr[Mathf.Clamp(i, 0, arr.Length - 1)];
    }

    public class CharacterAppearanceApplier : MonoBehaviour
    {
        private const string AccessoryNodeName = "_KeeperAccessory";

        private void Start() => StartCoroutine(ApplyWhenReady());

        private IEnumerator ApplyWhenReady()
        {
            // Wait up to ~1 s for the player to exist (rig spawns may lag a frame).
            GameObject player = null;
            for (int i = 0; i < 60 && player == null; i++)
            {
                player = SafeFindPlayer();
                if (player != null) break;
                yield return null;
            }
            if (player == null)
            {
                Hh.Log(LogCategory.Mission, "CharacterAppearanceApplier: no Player found — skipping (cozy no-op).");
                yield break;
            }
            Apply(player);
        }

        private static GameObject SafeFindPlayer()
        {
            try { return GameObject.FindGameObjectWithTag("Player"); }
            catch { return null; }
        }

        public void Apply(GameObject player)
        {
            var s = ServiceLocator.Get<SettingsService>();
            int skinIdx   = s != null ? s.PlayerSkinTone  : SettingsService.DefaultSkinTone;
            int outfitIdx = s != null ? s.PlayerOutfit     : SettingsService.DefaultOutfit;
            int accIdx    = s != null ? s.PlayerAccessory  : SettingsService.DefaultAccessory;

            Color skin   = CharacterPalette.Clamp(CharacterPalette.Skin, skinIdx);
            Color outfit = CharacterPalette.Clamp(CharacterPalette.Outfit, outfitIdx);

            TintRenderers(player, skin, outfit);
            BuildAccessory(player, accIdx, outfit);

            Hh.Log(LogCategory.Mission,
                $"CharacterAppearanceApplier: skin={skinIdx} outfit={outfitIdx} accessory={accIdx} applied.");
        }

        private static void TintRenderers(GameObject player, Color skin, Color outfit)
        {
            var renderers = player.GetComponentsInChildren<Renderer>(includeInactive: true);
            foreach (var r in renderers)
            {
                if (r == null) continue;
                // Use .materials (instances) so we never mutate shared assets.
                var mats = r.materials;
                for (int m = 0; m < mats.Length; m++)
                {
                    if (mats[m] == null) continue;
                    bool isSkin = LooksLikeSkin(r.name) || LooksLikeSkin(mats[m].name);
                    Color c = isSkin ? skin : outfit;
                    if (mats[m].HasProperty("_BaseColor")) mats[m].SetColor("_BaseColor", c); // URP Lit
                    if (mats[m].HasProperty("_Color"))     mats[m].SetColor("_Color", c);     // legacy/standard
                }
                r.materials = mats;
            }
        }

        private static bool LooksLikeSkin(string n)
        {
            if (string.IsNullOrEmpty(n)) return false;
            n = n.ToLowerInvariant();
            return n.Contains("skin") || n.Contains("head") || n.Contains("face") ||
                   n.Contains("hand") || n.Contains("arm")  || n.Contains("leg")  ||
                   n.Contains("foot") || n.Contains("body");
        }

        private static void BuildAccessory(GameObject player, int accIdx, Color tint)
        {
            // Remove any previous accessory (idempotent re-apply).
            var existing = FindDeep(player.transform, AccessoryNodeName);
            if (existing != null) Destroy(existing.gameObject);
            if (accIdx <= 0) return; // None

            // Anchor: a head-ish bone if present, else the top of the player bounds.
            Transform anchor = FindHead(player.transform);
            Vector3 headTop;
            if (anchor != null) headTop = anchor.position + Vector3.up * 0.12f;
            else
            {
                var rends = player.GetComponentsInChildren<Renderer>();
                float top = player.transform.position.y + 1.6f;
                foreach (var r in rends) top = Mathf.Max(top, r.bounds.max.y);
                headTop = new Vector3(player.transform.position.x, top + 0.02f, player.transform.position.z);
                anchor = player.transform;
            }

            var holder = new GameObject(AccessoryNodeName);
            holder.transform.SetParent(anchor, true);
            holder.transform.position = headTop;

            PrimitiveType prim = accIdx switch
            {
                1 => PrimitiveType.Cylinder, // Cap (squashed)
                2 => PrimitiveType.Sphere,   // Flower (small bloom)
                3 => PrimitiveType.Capsule,  // Scarf (around the neck)
                _ => PrimitiveType.Sphere,
            };
            var go = GameObject.CreatePrimitive(prim);
            go.name = "Accessory";
            go.transform.SetParent(holder.transform, false);
            var col = go.GetComponent<Collider>(); if (col != null) Destroy(col);

            switch (accIdx)
            {
                case 1: // Cap
                    go.transform.localScale = new Vector3(0.28f, 0.06f, 0.28f);
                    go.transform.localPosition = new Vector3(0, 0.04f, 0);
                    break;
                case 2: // Flower
                    go.transform.localScale = new Vector3(0.10f, 0.10f, 0.10f);
                    go.transform.localPosition = new Vector3(0.10f, 0.02f, 0.04f);
                    break;
                case 3: // Scarf
                    go.transform.localScale = new Vector3(0.26f, 0.10f, 0.26f);
                    go.transform.localPosition = new Vector3(0, -0.34f, 0);
                    break;
            }

            // Warm accent: flower pops, others echo the outfit colour.
            Color accColor = accIdx == 2 ? new Color(0.92f, 0.62f, 0.66f) : tint;
            var rend = go.GetComponent<Renderer>();
            if (rend != null)
            {
                var mat = rend.material;
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", accColor);
                if (mat.HasProperty("_Color"))     mat.SetColor("_Color", accColor);
            }
        }

        private static Transform FindHead(Transform root)
        {
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
            {
                string n = t.name.ToLowerInvariant();
                if (n.Contains("head") || n.Contains("skull")) return t;
            }
            return null;
        }

        private static Transform FindDeep(Transform t, string name)
        {
            if (t.name == name) return t;
            for (int i = 0; i < t.childCount; i++)
            {
                var hit = FindDeep(t.GetChild(i), name);
                if (hit != null) return hit;
            }
            return null;
        }
    }
}
