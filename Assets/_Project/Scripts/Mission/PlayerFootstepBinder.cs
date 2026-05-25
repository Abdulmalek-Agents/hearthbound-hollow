// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Mission / PlayerFootstepBinder
//
// PHASE 26 polish layer — adds footstep SFX via Animation Events.
//
// LIVES IN THE MISSION ASMDEF — not Player, not Audio — because it needs
// types from both:
//   • `HearthboundHollow.Player.PlayerController`  (to read IsSprinting)
//   • `HearthboundHollow.Audio.SfxPlayer` + `SfxLibrarySO`  (to play clips)
// Mission asmdef references both. Player ↔ Audio cross-cutting components
// belong here (per D-005 + D-035).
//
// USAGE
//   1. Add this component to the Player root GameObject.
//   2. The component auto-locates the Player's Animator + an SfxPlayer service.
//   3. Open each Walk / Run / Jump / Land Mixamo clip's Animation tab and
//      add Animation Events at the foot-strike frames:
//         function name: "OnFootstepLeft"   OR  "OnFootstepRight"
//      Unity dispatches the event to the GameObject hosting the Animator's
//      hierarchy. If you place the binder on the same root as the Animator,
//      the event arrives. If the Animator is on a `Body` child (BoZo wrapper
//      layout), Unity automatically searches parents — still arrives.
//   4. The binder picks the SfxLibrary entry id based on:
//         • Sprint state (run vs walk volume curve)
//         • Surface tag of the collider beneath the player
//
// NO mandatory Animation Events — if a Mixamo clip ships without them, the
// binder is silent. BoZo's bundled BMAC_M_Walk / BMAC_F_Walk also lack events
// out of the box. The Phase 26 doc explains how to add them in 30 seconds.

using UnityEngine;
using HearthboundHollow.Audio;
using HearthboundHollow.Core;
using HearthboundHollow.Player;

namespace HearthboundHollow.Mission
{
    [DisallowMultipleComponent]
    public class PlayerFootstepBinder : MonoBehaviour
    {
        [Header("SFX library entry ids (resolved against SfxLibrarySO via SfxPlayer)")]
        [Tooltip("Default footstep id — used when no surface-specific id matches.")]
        public string defaultFootstepId = "footstep_grass";

        [Tooltip("Sprint footstep id — louder/heavier variant when IsSprinting.")]
        public string sprintFootstepId = "footstep_grass_sprint";

        [Tooltip("Surface-specific overrides keyed by collider tag (e.g. 'Wood', 'Stone'). " +
                 "Falls back to defaultFootstepId if no override matches.")]
        public SurfaceOverride[] surfaceOverrides = new SurfaceOverride[]
        {
            new SurfaceOverride { surfaceTag = "Wood",  footstepId = "footstep_wood"  },
            new SurfaceOverride { surfaceTag = "Stone", footstepId = "footstep_stone" },
        };

        [Header("Tuning")]
        [Range(0f, 1.5f)]
        public float walkVolume = 0.55f;
        [Range(0f, 1.5f)]
        public float sprintVolume = 0.85f;
        [Range(0f, 0.3f)]
        public float pitchJitter = 0.07f;

        [Header("Probe (surface detection)")]
        [Tooltip("Distance below the player root to probe for a surface collider.")]
        public float surfaceProbeDistance = 1.2f;
        public LayerMask surfaceMask = ~0;

        [Header("Optional refs (auto-found if null)")]
        public PlayerController playerController;
        public SfxPlayer sfxPlayer;

        // ───── Lifecycle ───────────────────────────────────────────

        private void Awake()
        {
            if (playerController == null) playerController = GetComponentInChildren<PlayerController>(true);
            if (sfxPlayer == null) sfxPlayer = ServiceLocator.Get<SfxPlayer>();
        }

        private void Start()
        {
            // Late-bind SfxPlayer if it wasn't registered at Awake (the
            // SettingsService / SfxPlayer bootstraps execute on a Bootstrap
            // scene that may not be loaded when a smoke-test scene is
            // played directly).
            if (sfxPlayer == null) sfxPlayer = ServiceLocator.Get<SfxPlayer>();
        }

        // ───── Animation Event entry points ───────────────────────

        /// <summary>Called from an Animation Event placed on the left-foot strike frame.</summary>
        public void OnFootstepLeft()  => PlayFootstep(isLeftFoot: true);

        /// <summary>Called from an Animation Event placed on the right-foot strike frame.</summary>
        public void OnFootstepRight() => PlayFootstep(isLeftFoot: false);

        /// <summary>
        /// Generic entry point — useful when a clip only has a single "Footstep" event
        /// instead of left/right.
        /// </summary>
        public void OnFootstep() => PlayFootstep(isLeftFoot: false);

        // ───── Playback ───────────────────────────────────────────

        private void PlayFootstep(bool isLeftFoot)
        {
            if (sfxPlayer == null) return;

            bool sprinting = playerController != null && playerController.IsSprinting;
            string id = ResolveFootstepId(sprinting);
            float volume = sprinting ? sprintVolume : walkVolume;
            float pitch = 1f + Random.Range(-pitchJitter, pitchJitter);

            // The L/R alternation isn't acoustically meaningful (the clip
            // content is the same) but we expose the param so a future
            // designer can author asymmetric L/R variants.
            _ = isLeftFoot;

            try
            {
                sfxPlayer.PlayById(id, volume, pitch);
            }
            catch
            {
                // SfxPlayer may not expose PlayById in older builds; silently
                // ignore. The binder is opt-in polish.
            }
        }

        private string ResolveFootstepId(bool sprinting)
        {
            // Surface probe — only when the player has a transform we can probe from.
            if (surfaceOverrides != null && surfaceOverrides.Length > 0)
            {
                Ray r = new Ray(transform.position + Vector3.up * 0.1f, Vector3.down);
                if (Physics.Raycast(r, out var hit, surfaceProbeDistance, surfaceMask, QueryTriggerInteraction.Ignore))
                {
                    var col = hit.collider;
                    if (col != null && !string.IsNullOrEmpty(col.tag))
                    {
                        foreach (var s in surfaceOverrides)
                        {
                            if (!string.IsNullOrEmpty(s.surfaceTag) && col.CompareTag(s.surfaceTag))
                                return string.IsNullOrEmpty(s.footstepId) ? defaultFootstepId : s.footstepId;
                        }
                    }
                }
            }
            return sprinting && !string.IsNullOrEmpty(sprintFootstepId)
                ? sprintFootstepId
                : defaultFootstepId;
        }

        // ───── Inspector-shaped struct ────────────────────────────

        [System.Serializable]
        public struct SurfaceOverride
        {
            [Tooltip("Unity collider tag, e.g. 'Wood' or 'Stone'.")]
            public string surfaceTag;
            [Tooltip("SfxLibrary entry id to play when the surface matches.")]
            public string footstepId;
        }
    }
}
