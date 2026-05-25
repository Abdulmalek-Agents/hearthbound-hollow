// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Player / HearthAmbianceTrigger
//
// A purely-environmental trigger: when the player CharacterController enters
// the trigger volume, the hearth's AudioSource volume crossfades up to its
// target. When the player leaves, it fades back down to the resting baseline.
//
// This is the Codex 06 "warm-shows-warm" pattern — no E-press, no prompt,
// no UI. The world rewards proximity by getting cozier.
//
// ── Asmdef-locality (D-035) ───────────────────────────────────────
// Lives in HearthboundHollow.Player. No UI/Dialogue dependencies —
// trigger + AudioSource are both built-in Unity types.

using UnityEngine;
using HearthboundHollow.Core;

namespace HearthboundHollow.Player
{
    [RequireComponent(typeof(Collider))]
    public class HearthAmbianceTrigger : MonoBehaviour
    {
        [Header("Audio target")]
        [Tooltip("AudioSource whose volume gets crossfaded. Typically the hearth crackle loop.")]
        public AudioSource hearthSource;

        [Header("Volume tuning")]
        [Range(0f, 1f)] public float idleVolume   = 0.20f;
        [Range(0f, 1f)] public float closeVolume  = 0.70f;
        [Range(0.1f, 6f)] public float crossfadeSeconds = 1.6f;

        [Header("Player detection")]
        [Tooltip("Tag of the player CharacterController. Empty = any rigidbody/CharacterController matches.")]
        public string playerTag = "Player";

        // ---- runtime ----
        private float _targetVolume;
        private bool  _playerInside;

        private void Awake()
        {
            // Ensure the collider is a trigger.
            var col = GetComponent<Collider>();
            if (col != null && !col.isTrigger) col.isTrigger = true;

            _targetVolume = idleVolume;
            if (hearthSource != null)
            {
                hearthSource.volume = idleVolume;
                if (!hearthSource.isPlaying && hearthSource.clip != null) hearthSource.Play();
            }
        }

        private void Update()
        {
            if (hearthSource == null) return;
            // Smooth volume toward _targetVolume.
            float rate = 1f / Mathf.Max(0.1f, crossfadeSeconds);
            hearthSource.volume = Mathf.MoveTowards(hearthSource.volume, _targetVolume, rate * Time.deltaTime);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsPlayer(other)) return;
            _playerInside = true;
            _targetVolume = closeVolume;
            Hh.Log(LogCategory.Audio, "Hearth: player approached — fading crackle UP.");
        }

        private void OnTriggerExit(Collider other)
        {
            if (!IsPlayer(other)) return;
            _playerInside = false;
            _targetVolume = idleVolume;
            Hh.Log(LogCategory.Audio, "Hearth: player left — fading crackle DOWN.");
        }

        private bool IsPlayer(Collider other)
        {
            if (other == null) return false;
            if (string.IsNullOrEmpty(playerTag)) return other.GetComponentInParent<PlayerController>() != null;
            return other.CompareTag(playerTag) || (other.attachedRigidbody != null && other.attachedRigidbody.CompareTag(playerTag));
        }

        // Editor convenience — read back state in inspector during play.
        public bool IsPlayerInside => _playerInside;
    }
}
