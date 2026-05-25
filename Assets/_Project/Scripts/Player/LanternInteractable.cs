// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Player / LanternInteractable
//
// A lantern the player can light/dim with the E key. Two-state toggle:
// "warm bright" (default — Marin left it lit on the door) and "soft dim".
//
// The first time a unique noteId is toggled in a save, predecessorTrailWarmth
// gets a tiny +1 nudge — *someone* lit the lantern, and the world remembers.
//
// ── Asmdef-locality (D-035) ───────────────────────────────────────
// Lives in HearthboundHollow.Player. No UI/Dialogue dependencies —
// the lantern doesn't open any panel, it just changes a Light + plays
// an optional AudioSource and writes to VillageState. All three of
// those types resolve from Core/Player only.

using UnityEngine;
using HearthboundHollow.Core;

namespace HearthboundHollow.Player
{
    [DisallowMultipleComponent]
    public class LanternInteractable : Interactable
    {
        [Header("Light source")]
        [Tooltip("Optional. If unset, the script will find a Light component on " +
                 "this GameObject or any child at Awake().")]
        public Light bulb;

        [Header("Intensity tuning")]
        [Range(0.5f, 8f)] public float brightIntensity = 3.2f;
        [Range(0.0f, 4f)] public float dimIntensity = 0.65f;
        [Range(0.5f, 8f)] public float rangeBright = 8f;
        [Range(0.5f, 8f)] public float rangeDim    = 4f;
        public Color warmColor = new Color(1.0f, 0.78f, 0.42f);

        [Header("Audio (optional)")]
        public AudioSource chimeSource;
        public AudioClip toggleClip;
        [Range(0f, 1f)] public float chimeVolume = 0.5f;

        [Header("Save-state")]
        [Tooltip("Unique id used for the predecessorTrailWarmth bump. " +
                 "Each lantern with a different id grants +1 once.")]
        public string lanternId = "LANTERN_DOOR_HOLLOW";

        [Header("Initial state")]
        [Tooltip("If true, the lantern starts bright. If false, starts dim.")]
        public bool startBright = true;

        // ---- runtime ----
        private bool _isBright;

        private void Awake()
        {
            if (bulb == null) bulb = GetComponentInChildren<Light>(true);

            // Apply initial state once on Awake (cheap, no coroutine).
            _isBright = startBright;
            ApplyIntensity(_isBright);
        }

        public override string GetDynamicPromptText()
            => _isBright ? "Dim the lantern (E)" : "Light the lantern (E)";

        public override void Activate(GameObject player)
        {
            if (!IsInteractable) return;
            _isBright = !_isBright;
            ApplyIntensity(_isBright);

            // Soft chime feedback.
            if (chimeSource != null && toggleClip != null)
                chimeSource.PlayOneShot(toggleClip, chimeVolume);

            // First-time nudge to predecessorTrailWarmth (per the design plan).
            // We use VillageState.readMarinNoteIds as a poor-man's "already seen"
            // set — it's the same persistence layer Marin's Note writes to.
            var vs = ServiceLocator.Get<VillageState>();
            if (vs != null && !string.IsNullOrEmpty(lanternId)
                && !vs.readMarinNoteIds.Contains(lanternId))
            {
                vs.readMarinNoteIds.Add(lanternId);
                vs.predecessorTrailWarmth = Mathf.Clamp(vs.predecessorTrailWarmth + 1, 0, 100);
                Hh.Log(LogCategory.Pickle, $"Lantern '{lanternId}' first-touched — predecessor trail nudged +1.");
            }

            Hh.Log(LogCategory.UI, $"Lantern '{lanternId}' toggled → {(_isBright ? "BRIGHT" : "DIM")}.");
        }

        private void ApplyIntensity(bool bright)
        {
            if (bulb == null) return;
            bulb.color = warmColor;
            bulb.intensity = bright ? brightIntensity : dimIntensity;
            bulb.range = bright ? rangeBright : rangeDim;
        }
    }
}
