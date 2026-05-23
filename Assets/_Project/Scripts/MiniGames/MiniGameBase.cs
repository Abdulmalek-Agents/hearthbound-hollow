// SPDX-License-Identifier: MIT
// Hearthbound Hollow — MiniGames / MiniGameBase
//
// Abstract base for every workbench mini-game. M1-2 ships PolishMiniGame +
// CleanseMiniGame. Future Weave / Sever / Listen / Read / Translate / etc.
// (Codex 13) subclass this once unlocked in Mission 3+.

using System;
using UnityEngine;
using HearthboundHollow.Core;
using HearthboundHollow.Player;

namespace HearthboundHollow.MiniGames
{
    public enum MiniGameState
    {
        Idle,
        Engaging,
        Active,
        Completing,
        Complete,
        Aborted,
    }

    public abstract class MiniGameBase : MonoBehaviour
    {
        [Header("Mini-Game (shared)")]
        public MemoryOrbInteractable targetOrb;

        [Header("Auto-Complete (Codex 06 — mandatory for cozy-target audience)")]
        public bool autoCompleteAvailable = true;

        public MiniGameState State { get; protected set; } = MiniGameState.Idle;
        public float RuntimeProgress01 { get; protected set; }
        public float ElapsedSeconds { get; protected set; }

        public event Action<MiniGameBase> OnGameStarted;
        public event Action<MiniGameBase> OnGameFinished;

        public virtual void BeginGame(MemoryOrbInteractable orb)
        {
            targetOrb = orb;
            State = MiniGameState.Engaging;
            RuntimeProgress01 = 0f;
            ElapsedSeconds = 0f;
            Hh.Log(LogCategory.MiniGame, $"{GetType().Name} began on memory '{(orb != null && orb.memory != null ? orb.memory.id : "<null>")}'.");
            OnGameStarted?.Invoke(this);
        }

        protected virtual void Update()
        {
            if (State == MiniGameState.Active || State == MiniGameState.Engaging)
                ElapsedSeconds += Time.deltaTime;
        }

        public abstract void DoAutoComplete();

        public virtual void Abort()
        {
            if (State == MiniGameState.Complete || State == MiniGameState.Aborted) return;
            State = MiniGameState.Aborted;
            Hh.Log(LogCategory.MiniGame, $"{GetType().Name} aborted at progress {RuntimeProgress01:F2}.");
            OnGameFinished?.Invoke(this);
        }

        protected void FinishGame()
        {
            if (State == MiniGameState.Complete) return;
            State = MiniGameState.Complete;
            Hh.Log(LogCategory.MiniGame, $"{GetType().Name} completed in {ElapsedSeconds:F1}s at progress {RuntimeProgress01:F2}.");
            OnGameFinished?.Invoke(this);
        }
    }
}
