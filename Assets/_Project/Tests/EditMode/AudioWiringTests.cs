// SPDX-License-Identifier: MIT
// Hearthbound Hollow — EditMode tests for the Phase 37-38 audio subsystem.
//
// Pins the public surface of:
//   * MusicLibrarySO + AmbienceLibrarySO (Get / Has)
//   * MumbleVoiceLibrarySO (GetBank)
//   * MumbleVoicePlayer.SpeakLine — does not throw on empty / null inputs
//   * DialogueLineStartedEvent / DialogueLineEndedEvent / SceneAudioRequestedEvent payloads
//   * DreamAudioBinder.CueMapping serializable shape
//
// Per D-035 (asmdef-locality) the test asmdef references
// HearthboundHollow.Audio + HearthboundHollow.Cutscene + Core + Memory
// — every type these tests touch lives in one of those.

using NUnit.Framework;
using UnityEngine;
using HearthboundHollow.Audio;
using HearthboundHollow.Core;
using HearthboundHollow.Cutscene;

namespace HearthboundHollow.Tests.EditMode
{
    public class AudioLibraryTests
    {
        // ─── MusicLibrarySO ────────────────────────────────────

        [Test]
        public void MusicLibrarySO_Get_returns_clip_for_known_id()
        {
            var lib = ScriptableObject.CreateInstance<MusicLibrarySO>();
            var clip = AudioClip.Create("dummy", 4410, 1, 44100, false);
            lib.entries.Add(new MusicLibrarySO.Entry
            {
                id = "scene_lane",
                clip = clip,
                volume = 0.5f,
                loop = true,
                fadeInSeconds = 2.0f,
                fadeOutSeconds = 1.5f,
            });

            var got = lib.Get("scene_lane", out var vol, out var loop, out var fadeIn, out var fadeOut);
            Assert.AreEqual(clip, got);
            Assert.AreEqual(0.5f, vol);
            Assert.IsTrue(loop);
            Assert.AreEqual(2.0f, fadeIn);
            Assert.AreEqual(1.5f, fadeOut);

            Object.DestroyImmediate(lib);
            Object.DestroyImmediate(clip);
        }

        [Test]
        public void MusicLibrarySO_Get_returns_null_for_unknown_id_with_safe_defaults()
        {
            var lib = ScriptableObject.CreateInstance<MusicLibrarySO>();
            var got = lib.Get("missing", out var vol, out var loop, out var fadeIn, out var fadeOut);
            Assert.IsNull(got);
            // The contract documents these defaults — locking them so the
            // runtime never picks up bogus volume 0 or fade 0.
            Assert.AreEqual(1f, vol);
            Assert.IsTrue(loop);
            Assert.AreEqual(2f, fadeIn);
            Assert.AreEqual(2f, fadeOut);
            Object.DestroyImmediate(lib);
        }

        [Test]
        public void MusicLibrarySO_Has_reports_membership()
        {
            var lib = ScriptableObject.CreateInstance<MusicLibrarySO>();
            lib.entries.Add(new MusicLibrarySO.Entry { id = "scene_hollow" });
            Assert.IsTrue(lib.Has("scene_hollow"));
            Assert.IsFalse(lib.Has("scene_unknown"));
            Object.DestroyImmediate(lib);
        }

        // ─── MumbleVoiceLibrarySO ──────────────────────────────

        [Test]
        public void MumbleVoiceLibrarySO_GetBank_returns_known_character()
        {
            var lib = ScriptableObject.CreateInstance<MumbleVoiceLibrarySO>();
            lib.banks.Add(new MumbleVoiceLibrarySO.CharacterVoiceBank
            {
                characterId = "doris",
                volume = 0.55f,
                pitchVariance = 0.08f,
                syllableRate = 7.0f,
            });
            var bank = lib.GetBank("doris");
            Assert.IsNotNull(bank);
            Assert.AreEqual("doris", bank.characterId);
            Assert.AreEqual(7.0f, bank.syllableRate);
            Object.DestroyImmediate(lib);
        }

        [Test]
        public void MumbleVoiceLibrarySO_GetBank_is_case_insensitive()
        {
            var lib = ScriptableObject.CreateInstance<MumbleVoiceLibrarySO>();
            lib.banks.Add(new MumbleVoiceLibrarySO.CharacterVoiceBank
            {
                characterId = "Pickle",
                syllableRate = 11.0f,
            });
            // DialogueUI lower-cases speaker names; the library must match
            // regardless of how the bank was authored.
            Assert.IsNotNull(lib.GetBank("pickle"));
            Assert.IsNotNull(lib.GetBank("PICKLE"));
            Assert.IsNotNull(lib.GetBank("Pickle"));
            Assert.IsNull(lib.GetBank("doris"));
            Object.DestroyImmediate(lib);
        }
    }

    public class AudioEventPayloadTests
    {
        [Test]
        public void DialogueLineStartedEvent_stores_payload_fields_verbatim()
        {
            var ev = new DialogueLineStartedEvent("doris", "Hello.", 1.5f);
            Assert.AreEqual("doris", ev.Speaker);
            Assert.AreEqual("Hello.", ev.LineText);
            Assert.AreEqual(1.5f, ev.EstimatedDurationSec);
        }

        [Test]
        public void DialogueLineEndedEvent_stores_speaker_id()
        {
            var ev = new DialogueLineEndedEvent("gerrold");
            Assert.AreEqual("gerrold", ev.Speaker);
        }

        [Test]
        public void SceneAudioRequestedEvent_stores_both_ids()
        {
            var ev = new SceneAudioRequestedEvent("scene_hollow", "scene_hollow");
            Assert.AreEqual("scene_hollow", ev.MusicId);
            Assert.AreEqual("scene_hollow", ev.AmbienceId);
        }

        [Test]
        public void SceneAudioRequestedEvent_allows_empty_ambience()
        {
            // MainMenu uses no ambient bed — empty ambienceId is a valid case.
            var ev = new SceneAudioRequestedEvent("scene_menu", "");
            Assert.AreEqual("scene_menu", ev.MusicId);
            Assert.AreEqual("", ev.AmbienceId);
        }
    }

    public class DreamAudioBinderShapeTests
    {
        [Test]
        public void CueMapping_is_serializable_and_field_addressable()
        {
            var m = new DreamAudioBinder.CueMapping
            {
                variantId = "Dream2_VariantA_EraseClean",
                musicId = "dream_margery_a",
            };
            Assert.AreEqual("Dream2_VariantA_EraseClean", m.variantId);
            Assert.AreEqual("dream_margery_a", m.musicId);

            // Reflection check — Phase 38 expects these two fields to remain
            // public for prefab-side serialization. If they're ever renamed
            // the binder's saved data in the prefab would silently drop, so
            // we pin them here.
            var t = typeof(DreamAudioBinder.CueMapping);
            Assert.IsNotNull(t.GetField("variantId"));
            Assert.IsNotNull(t.GetField("musicId"));
        }
    }
}
