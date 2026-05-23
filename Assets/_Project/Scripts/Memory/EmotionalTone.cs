// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Memory / EmotionalTone
//
// The seven canonical emotional tones from Codex 02. Used as the orb's
// palette tint key and the dream's lens selector.

namespace HearthboundHollow.Memory
{
    public enum EmotionalTone
    {
        Joy,
        Grief,
        Shame,
        Awe,
        Longing,
        Dread,
        Grace,
    }

    public static class EmotionalToneExtensions
    {
        /// <summary>The orb's color tint per tone (Codex 11 § 3 palette).</summary>
        public static UnityEngine.Color GetPaletteTint(this EmotionalTone tone) => tone switch
        {
            EmotionalTone.Joy     => new UnityEngine.Color(1.00f, 0.78f, 0.42f), // warm amber
            EmotionalTone.Grief   => new UnityEngine.Color(0.40f, 0.55f, 0.78f), // dusk blue
            EmotionalTone.Shame   => new UnityEngine.Color(0.78f, 0.46f, 0.50f), // muted rose
            EmotionalTone.Awe     => new UnityEngine.Color(0.62f, 0.82f, 0.92f), // dawn sky
            EmotionalTone.Longing => new UnityEngine.Color(0.85f, 0.68f, 0.90f), // lavender
            EmotionalTone.Dread   => new UnityEngine.Color(0.32f, 0.30f, 0.36f), // graphite
            EmotionalTone.Grace   => new UnityEngine.Color(0.96f, 0.92f, 0.82f), // candle ivory
            _ => UnityEngine.Color.white,
        };
    }
}
