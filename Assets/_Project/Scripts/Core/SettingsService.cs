// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Core / SettingsService
//
// PlayerPrefs-backed persistence for accessibility + audio + comfort settings.
// All getters return safe defaults when the key is absent. All setters persist
// immediately via PlayerPrefs.Save() so settings survive a hard quit.
//
// Subscribers can listen to `OnSettingsChanged` to refresh UI state.
//
// Scope: Mission 1-2. New keys may be added in Mission 3+ without breaking the
// existing schema (PlayerPrefs is forward-compatible).
//
// Phase 37 update (2026-05-26): added `VoiceVolume` + `AudioChannel.Voice`
// for the new MumbleVoicePlayer + future commercial-VO replacement.

using System;
using UnityEngine;

namespace HearthboundHollow.Core
{
    public class SettingsService
    {
        // ───── Keys ────────────────────────────────────────────────

        private const string K_MasterVolume   = "hh.audio.master";
        private const string K_MusicVolume    = "hh.audio.music";
        private const string K_SfxVolume      = "hh.audio.sfx";
        private const string K_AmbientVolume  = "hh.audio.ambient";
        private const string K_VoiceVolume    = "hh.audio.voice";
        private const string K_TextCps        = "hh.text.cps";
        private const string K_GentleMode     = "hh.comfort.gentle";
        private const string K_AutoPolish     = "hh.comfort.autoPolish";
        private const string K_AutoCleanse    = "hh.comfort.autoCleanse";
        private const string K_SubtitleSize   = "hh.comfort.subtitleTier";
        private const string K_ColorPalette   = "hh.comfort.palette";
        private const string K_OneHandMode    = "hh.comfort.oneHand";
        private const string K_ContentWarn    = "hh.comfort.contentWarnings";

        // ───── Defaults ────────────────────────────────────────────

        public const float  DefaultMasterVolume  = 0.85f;
        public const float  DefaultMusicVolume   = 0.70f;
        public const float  DefaultSfxVolume     = 0.85f;
        public const float  DefaultAmbientVolume = 0.55f;
        public const float  DefaultVoiceVolume   = 0.65f;
        public const int    DefaultTextCps       = 45;     // matches DialogueUI default
        public const bool   DefaultGentleMode    = false;
        public const bool   DefaultAutoPolish    = false;
        public const bool   DefaultAutoCleanse   = false;
        public const int    DefaultSubtitleSize  = 1;      // Medium
        public const string DefaultColorPalette  = "default";
        public const bool   DefaultOneHandMode   = false;
        public const bool   DefaultContentWarn   = true;

        // ───── Events ──────────────────────────────────────────────

        /// <summary>Fired any time any setting changes. Subscribers should re-read the values they care about.</summary>
        public event Action OnSettingsChanged;

        // ───── Audio ───────────────────────────────────────────────

        public float MasterVolume
        {
            get => PlayerPrefs.GetFloat(K_MasterVolume, DefaultMasterVolume);
            set { PlayerPrefs.SetFloat(K_MasterVolume, Mathf.Clamp01(value)); Persist(); }
        }

        public float MusicVolume
        {
            get => PlayerPrefs.GetFloat(K_MusicVolume, DefaultMusicVolume);
            set { PlayerPrefs.SetFloat(K_MusicVolume, Mathf.Clamp01(value)); Persist(); }
        }

        public float SfxVolume
        {
            get => PlayerPrefs.GetFloat(K_SfxVolume, DefaultSfxVolume);
            set { PlayerPrefs.SetFloat(K_SfxVolume, Mathf.Clamp01(value)); Persist(); }
        }

        public float AmbientVolume
        {
            get => PlayerPrefs.GetFloat(K_AmbientVolume, DefaultAmbientVolume);
            set { PlayerPrefs.SetFloat(K_AmbientVolume, Mathf.Clamp01(value)); Persist(); }
        }

        public float VoiceVolume
        {
            get => PlayerPrefs.GetFloat(K_VoiceVolume, DefaultVoiceVolume);
            set { PlayerPrefs.SetFloat(K_VoiceVolume, Mathf.Clamp01(value)); Persist(); }
        }

        /// <summary>
        /// Effective volume = master * channel. Use this when applying to AudioSources.
        /// </summary>
        public float EffectiveVolume(AudioChannel channel) => channel switch
        {
            AudioChannel.Music   => MasterVolume * MusicVolume,
            AudioChannel.Sfx     => MasterVolume * SfxVolume,
            AudioChannel.Ambient => MasterVolume * AmbientVolume,
            AudioChannel.Voice   => MasterVolume * VoiceVolume,
            _ => MasterVolume,
        };

        // ───── Text + comfort ──────────────────────────────────────

        public int TextCps
        {
            get => PlayerPrefs.GetInt(K_TextCps, DefaultTextCps);
            set { PlayerPrefs.SetInt(K_TextCps, Mathf.Clamp(value, 12, 120)); Persist(); }
        }

        public bool GentleMode
        {
            get => PlayerPrefs.GetInt(K_GentleMode, DefaultGentleMode ? 1 : 0) == 1;
            set { PlayerPrefs.SetInt(K_GentleMode, value ? 1 : 0); Persist(); }
        }

        public bool AutoCompletePolish
        {
            get => PlayerPrefs.GetInt(K_AutoPolish, DefaultAutoPolish ? 1 : 0) == 1;
            set { PlayerPrefs.SetInt(K_AutoPolish, value ? 1 : 0); Persist(); }
        }

        public bool AutoCompleteCleanse
        {
            get => PlayerPrefs.GetInt(K_AutoCleanse, DefaultAutoCleanse ? 1 : 0) == 1;
            set { PlayerPrefs.SetInt(K_AutoCleanse, value ? 1 : 0); Persist(); }
        }

        public int SubtitleSizeTier
        {
            get => PlayerPrefs.GetInt(K_SubtitleSize, DefaultSubtitleSize);
            set { PlayerPrefs.SetInt(K_SubtitleSize, Mathf.Clamp(value, 0, 3)); Persist(); }
        }

        public string ColorPalette
        {
            get => PlayerPrefs.GetString(K_ColorPalette, DefaultColorPalette);
            set { PlayerPrefs.SetString(K_ColorPalette, value ?? DefaultColorPalette); Persist(); }
        }

        public bool OneHandMode
        {
            get => PlayerPrefs.GetInt(K_OneHandMode, DefaultOneHandMode ? 1 : 0) == 1;
            set { PlayerPrefs.SetInt(K_OneHandMode, value ? 1 : 0); Persist(); }
        }

        public bool ContentWarningsEnabled
        {
            get => PlayerPrefs.GetInt(K_ContentWarn, DefaultContentWarn ? 1 : 0) == 1;
            set { PlayerPrefs.SetInt(K_ContentWarn, value ? 1 : 0); Persist(); }
        }

        // ───── Reset ───────────────────────────────────────────────

        public void ResetToDefaults()
        {
            PlayerPrefs.DeleteKey(K_MasterVolume);
            PlayerPrefs.DeleteKey(K_MusicVolume);
            PlayerPrefs.DeleteKey(K_SfxVolume);
            PlayerPrefs.DeleteKey(K_AmbientVolume);
            PlayerPrefs.DeleteKey(K_VoiceVolume);
            PlayerPrefs.DeleteKey(K_TextCps);
            PlayerPrefs.DeleteKey(K_GentleMode);
            PlayerPrefs.DeleteKey(K_AutoPolish);
            PlayerPrefs.DeleteKey(K_AutoCleanse);
            PlayerPrefs.DeleteKey(K_SubtitleSize);
            PlayerPrefs.DeleteKey(K_ColorPalette);
            PlayerPrefs.DeleteKey(K_OneHandMode);
            PlayerPrefs.DeleteKey(K_ContentWarn);
            Persist();
        }

        private void Persist()
        {
            PlayerPrefs.Save();
            OnSettingsChanged?.Invoke();
        }
    }

    public enum AudioChannel
    {
        Master,
        Music,
        Sfx,
        Ambient,
        Voice,
    }
}
