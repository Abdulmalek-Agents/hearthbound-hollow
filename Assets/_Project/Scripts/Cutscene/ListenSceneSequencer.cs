// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Cutscene / ListenSceneSequencer
//
// Drives the 3-minute "Listen" cutscene in Mission 2 when the player picks
// MoralChoice.Listen on Gerrold's choice card. Per Focus 06 § 7.

using UnityEngine;
using UnityEngine.Playables;
using HearthboundHollow.Core;

namespace HearthboundHollow.Cutscene
{
    public class ListenSceneSequencer : MonoBehaviour
    {
        [Header("Timeline")]
        public PlayableDirector director;
        public PlayableAsset listenTimeline;

        [Header("Camera (optional Cinemachine virtual cam)")]
        [Tooltip("Set this camera priority high during the cutscene; restore after.")]
        public Behaviour listenVirtualCamera;
        public int activePriority = 50;
        public int inactivePriority = 0;

        [Header("Routing")]
        public MemoryDreamSequencer dreamSequencer;

        public event System.Action OnListenStarted;
        public event System.Action OnListenCompleted;

        public bool IsPlaying { get; private set; }

        public void PlayListenScene()
        {
            if (IsPlaying) { Hh.Warn(LogCategory.Cutscene, "ListenSceneSequencer already playing."); return; }
            if (director == null || listenTimeline == null)
            {
                Hh.Err(LogCategory.Cutscene, "ListenSceneSequencer missing director or listenTimeline.");
                FinishImmediate();
                return;
            }

            IsPlaying = true;
            SetCameraActive(true);
            director.playableAsset = listenTimeline;
            director.stopped += OnDirectorStopped;
            director.Play();

            Hh.Log(LogCategory.Cutscene, "Listen scene started.");
            OnListenStarted?.Invoke();
        }

        private void OnDirectorStopped(PlayableDirector d)
        {
            d.stopped -= OnDirectorStopped;
            SetCameraActive(false);
            IsPlaying = false;

            Hh.Log(LogCategory.Cutscene, "Listen scene completed.");
            OnListenCompleted?.Invoke();

            // Transition to Dream 2 Variant D (the Listen variant).
            if (dreamSequencer != null)
            {
                dreamSequencer.PlayDream2(HearthboundHollow.Memory.MoralChoice.Listen,
                                         HearthboundHollow.Memory.CleanseOutcome.Perfect);
            }
        }

        private void FinishImmediate()
        {
            IsPlaying = false;
            OnListenCompleted?.Invoke();
        }

        private void SetCameraActive(bool active)
        {
            if (listenVirtualCamera == null) return;
            // Reflection-style priority bump so we don't take a Cinemachine compile dep.
            // Cinemachine 3 virtual cams expose `Priority` as a public field.
            var field = listenVirtualCamera.GetType().GetField("Priority")
                       ?? listenVirtualCamera.GetType().GetField("m_Priority");
            if (field != null)
            {
                try { field.SetValue(listenVirtualCamera, active ? activePriority : inactivePriority); }
                catch { /* ignore — fallback below */ }
            }
            listenVirtualCamera.enabled = active;
        }
    }
}
