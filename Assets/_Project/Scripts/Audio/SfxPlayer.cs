// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Audio / SfxPlayer
//
// MonoBehaviour service that plays clips from an SfxLibrarySO by ID.
// Spawns short-lived AudioSource children for one-shots; reuses a single
// looping AudioSource for ambient + loop tracks. Registers itself with
// ServiceLocator so any subsystem can call:
//
//   ServiceLocator.Get<SfxPlayer>()?.PlayOneShot("polish_rub_start");
//
// In Phase 18 the SceneBuilder spawns one of these per playable scene
// and wires it to the SfxLibrary asset built by Phase18_AudioBuilder.

using System.Collections.Generic;
using UnityEngine;
using HearthboundHollow.Core;

namespace HearthboundHollow.Audio
{
    [DisallowMultipleComponent]
    public class SfxPlayer : MonoBehaviour
    {
        [Header("Library (set by SceneBuilder or in Inspector)")]
        public SfxLibrarySO library;

        [Header("Mixer routing (optional)")]
        public UnityEngine.Audio.AudioMixerGroup oneShotGroup;
        public UnityEngine.Audio.AudioMixerGroup loopGroup;

        private readonly Dictionary<string, AudioSource> _loopSources = new();

        private void Awake()
        {
            ServiceLocator.Register(this);
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<SfxPlayer>();
        }

        public void PlayOneShot(string id, float volumeScale = 1f)
        {
            if (library == null) { Hh.Warn(LogCategory.Audio, $"SfxPlayer has no library; cannot play '{id}'."); return; }
            var clip = library.Get(id, out var vol, out var loop);
            if (clip == null) { Hh.Warn(LogCategory.Audio, $"SfxLibrary has no entry for id '{id}'."); return; }
            if (loop) { PlayLoop(id); return; }

            var go = new GameObject($"OneShot:{id}");
            go.transform.SetParent(transform, false);
            var src = go.AddComponent<AudioSource>();
            src.clip = clip;
            src.volume = vol * volumeScale;
            src.spatialBlend = 0f;
            src.outputAudioMixerGroup = oneShotGroup;
            src.Play();
            Destroy(go, clip.length + 0.5f);
        }

        public void PlayLoop(string id, float volumeScale = 1f)
        {
            if (library == null) return;
            if (_loopSources.TryGetValue(id, out var existing) && existing != null)
            {
                if (!existing.isPlaying) existing.Play();
                return;
            }
            var clip = library.Get(id, out var vol, out var loop);
            if (clip == null) { Hh.Warn(LogCategory.Audio, $"SfxLibrary has no entry for loop '{id}'."); return; }

            var go = new GameObject($"Loop:{id}");
            go.transform.SetParent(transform, false);
            var src = go.AddComponent<AudioSource>();
            src.clip = clip;
            src.volume = vol * volumeScale;
            src.loop = true;
            src.spatialBlend = 0f;
            src.outputAudioMixerGroup = loopGroup;
            src.Play();
            _loopSources[id] = src;
        }

        public void StopLoop(string id)
        {
            if (_loopSources.TryGetValue(id, out var src) && src != null)
            {
                src.Stop();
                Destroy(src.gameObject);
                _loopSources.Remove(id);
            }
        }

        public void StopAllLoops()
        {
            foreach (var kv in _loopSources)
                if (kv.Value != null) { kv.Value.Stop(); Destroy(kv.Value.gameObject); }
            _loopSources.Clear();
        }
    }
}
