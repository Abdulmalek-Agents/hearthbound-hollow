// SPDX-License-Identifier: MIT
// Hearthbound Hollow — MiniGames / CleanseMiniGame (Mission 2)
//
// The player traces cracks on a memory orb without touching the core.
// Reading: Focus 04 § 3.
//
// Outcomes: Perfect | Acceptable | Sloppy | CrossedCore (Focus 04 § 3.5)
// The narrative absorbs the failure state; we never display "Failure" text.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using HearthboundHollow.Core;
using HearthboundHollow.Memory;
using HearthboundHollow.Player;

namespace HearthboundHollow.MiniGames
{
    [System.Serializable]
    public struct CleanseCrackDefinition
    {
        public Vector2 startUv;
        public Vector2 endUv;
        public float thickness;
        public string id;
    }

    public class CleanseMiniGame : MiniGameBase
    {
        [Header("Difficulty profile")]
        [Range(0.01f, 0.3f)] public float crackTolerance = 0.08f;
        [Range(0.05f, 0.5f)] public float coreRadius = 0.25f;
        [Range(0.01f, 0.4f)] public float coreWarningOuterRadius = 0.32f;
        [Range(0, 6)] public int coreTouchesForCrossed = 3;
        public bool gentleMode = false;

        [Header("Crack definitions for GER-007 (Focus 04 § 3.3)")]
        public List<CleanseCrackDefinition> cracks = new()
        {
            new CleanseCrackDefinition { id = "bedside_a", startUv = new Vector2(0.20f, 0.78f), endUv = new Vector2(0.34f, 0.72f), thickness = 0.06f },
            new CleanseCrackDefinition { id = "bedside_b", startUv = new Vector2(0.62f, 0.78f), endUv = new Vector2(0.76f, 0.72f), thickness = 0.06f },
            new CleanseCrackDefinition { id = "kitchen",   startUv = new Vector2(0.18f, 0.38f), endUv = new Vector2(0.34f, 0.32f), thickness = 0.07f },
            new CleanseCrackDefinition { id = "seventh",   startUv = new Vector2(0.66f, 0.30f), endUv = new Vector2(0.82f, 0.20f), thickness = 0.07f },
        };

        [Header("Input")]
        [SerializeField] private InputActionReference pointerPositionAction;
        [SerializeField] private InputActionReference pointerActiveAction;
        [SerializeField] private RectTransform orbScreenRect;

        public CleanseOutcome ComputedOutcome { get; private set; }

        private int _sealedCracks;
        private int _crossedCoreTouches;
        private bool _engagedOnCrack;
        private int _currentCrackIndex = -1;
        private float _currentCrackProgress;
        private float _sustainedCoreSeconds;
        private Vector2 _lastPointer;

        public event System.Action<int> OnCrackSealed;
        public event System.Action OnCoreWarning;
        public event System.Action OnCoreDamage;
        public event System.Action<CleanseOutcome> OnCleanseOutcomeDetermined;

        public override void BeginGame(MemoryOrbInteractable orb)
        {
            base.BeginGame(orb);
            _sealedCracks = 0;
            _crossedCoreTouches = 0;
            _engagedOnCrack = false;
            _currentCrackIndex = -1;
            _currentCrackProgress = 0f;
            _sustainedCoreSeconds = 0f;
            State = MiniGameState.Active;
            pointerPositionAction?.action?.Enable();
            pointerActiveAction?.action?.Enable();
        }

        protected override void Update()
        {
            base.Update();
            if (State != MiniGameState.Active) return;

            Vector2 pos = pointerPositionAction != null && pointerPositionAction.action != null
                ? pointerPositionAction.action.ReadValue<Vector2>()
                : (Vector2)Input.mousePosition;
            bool down = pointerActiveAction == null || pointerActiveAction.action == null || pointerActiveAction.action.IsPressed();

            if (!down)
            {
                if (_engagedOnCrack) _engagedOnCrack = false;
                _lastPointer = pos;
                return;
            }

            Vector2 uv = ScreenToOrbUv(pos);
            float coreDist = Vector2.Distance(uv, new Vector2(0.5f, 0.5f));

            if (coreDist < coreRadius)
            {
                _sustainedCoreSeconds += Time.deltaTime;
                if (_sustainedCoreSeconds > 0.05f)
                {
                    if (!_engagedOnCrack || _sustainedCoreSeconds > 1.0f)
                    {
                        _crossedCoreTouches++;
                        OnCoreDamage?.Invoke();
                        var vs = ServiceLocator.Get<VillageState>();
                        if (vs != null)
                            vs.memoryIntegrityGerrold = VillageState.Adjust(vs.memoryIntegrityGerrold, -8);
                        _sustainedCoreSeconds = 0f;
                        if (_crossedCoreTouches >= coreTouchesForCrossed)
                        {
                            ComputedOutcome = CleanseOutcome.CrossedCore;
                            Complete();
                            return;
                        }
                    }
                }
                _lastPointer = pos;
                return;
            }
            else if (coreDist < coreWarningOuterRadius)
            {
                OnCoreWarning?.Invoke();
                _sustainedCoreSeconds = 0f;
            }
            else
            {
                _sustainedCoreSeconds = 0f;
            }

            if (!_engagedOnCrack)
            {
                int idx = FindCrackUnder(uv);
                if (idx >= 0)
                {
                    _engagedOnCrack = true;
                    _currentCrackIndex = idx;
                    _currentCrackProgress = 0f;
                }
            }
            else
            {
                var c = cracks[_currentCrackIndex];
                float prog = ProgressAlongLine(c.startUv, c.endUv, uv);
                float perp = PerpendicularDistance(c.startUv, c.endUv, uv);
                if (perp > c.thickness)
                {
                    _engagedOnCrack = false;
                }
                else
                {
                    if (prog > _currentCrackProgress) _currentCrackProgress = prog;
                    if (_currentCrackProgress > 0.97f)
                    {
                        _engagedOnCrack = false;
                        _sealedCracks++;
                        OnCrackSealed?.Invoke(_sealedCracks);
                        RuntimeProgress01 = (float)_sealedCracks / Mathf.Max(1, cracks.Count);
                        if (_sealedCracks >= cracks.Count) { DetermineOutcome(); Complete(); }
                    }
                }
            }
            _lastPointer = pos;
        }

        private Vector2 ScreenToOrbUv(Vector2 screen)
        {
            if (orbScreenRect == null)
                return new Vector2(screen.x / Mathf.Max(1, Screen.width), screen.y / Mathf.Max(1, Screen.height));
            var rect = orbScreenRect.rect;
            var origin = orbScreenRect.position;
            return new Vector2(
                Mathf.InverseLerp(origin.x, origin.x + rect.width, screen.x),
                Mathf.InverseLerp(origin.y, origin.y + rect.height, screen.y));
        }

        private int FindCrackUnder(Vector2 uv)
        {
            for (int i = 0; i < cracks.Count; i++)
            {
                var c = cracks[i];
                if (Vector2.Distance(uv, c.startUv) < c.thickness * 1.5f) return i;
            }
            return -1;
        }

        private static float ProgressAlongLine(Vector2 a, Vector2 b, Vector2 p)
        {
            Vector2 ab = b - a;
            float denom = ab.sqrMagnitude;
            if (denom < 1e-6f) return 0f;
            return Mathf.Clamp01(Vector2.Dot(p - a, ab) / denom);
        }

        private static float PerpendicularDistance(Vector2 a, Vector2 b, Vector2 p)
        {
            float t = ProgressAlongLine(a, b, p);
            Vector2 proj = Vector2.Lerp(a, b, t);
            return Vector2.Distance(proj, p);
        }

        private void DetermineOutcome()
        {
            if (_sealedCracks >= cracks.Count && _crossedCoreTouches == 0)
                ComputedOutcome = CleanseOutcome.Perfect;
            else if (_sealedCracks >= cracks.Count - 1 && _crossedCoreTouches == 0)
                ComputedOutcome = CleanseOutcome.Acceptable;
            else if (_sealedCracks >= cracks.Count && _crossedCoreTouches > 0 && _crossedCoreTouches < coreTouchesForCrossed)
                ComputedOutcome = CleanseOutcome.Sloppy;
            else
                ComputedOutcome = CleanseOutcome.CrossedCore;

            OnCleanseOutcomeDetermined?.Invoke(ComputedOutcome);
        }

        public override void DoAutoComplete()
        {
            if (State == MiniGameState.Complete) return;
            ComputedOutcome = gentleMode ? CleanseOutcome.Perfect : CleanseOutcome.Acceptable;
            _sealedCracks = cracks.Count - (gentleMode ? 0 : 1);
            OnCleanseOutcomeDetermined?.Invoke(ComputedOutcome);
            Complete(autoCompleted: true);
        }

        private void Complete(bool autoCompleted = false)
        {
            if (State == MiniGameState.Complete) return;
            FinishGame();
            EventBus.Publish(new MemoryCleansedEvent(targetOrb != null ? targetOrb.memory : null, (int)ComputedOutcome, autoCompleted));
            Hh.Log(LogCategory.MiniGame, $"Cleanse outcome: {ComputedOutcome} (cracks {_sealedCracks}/{cracks.Count}, core touches {_crossedCoreTouches}).");
        }
    }
}
