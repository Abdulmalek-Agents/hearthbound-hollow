// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Mission / PickleAI
//
// Pickle the cat. M1-2 ships her with a light AI: idle, head-turn toward
// activity, tail-flick on Polish success, four scripted speech lines.

using UnityEngine;
using HearthboundHollow.Core;

namespace HearthboundHollow.Mission
{
    public class PickleAI : MonoBehaviour
    {
        [Header("Visual")]
        public Transform headBone;
        public Transform tailBone;
        public Transform[] sleepSpots;

        [Header("Speech (4 quotes total in M1-2 — Focus 01 § 6.1)")]
        public string[] m1Lines = new[]
        {
            "Slower.",
            "That was — adequate. I was expecting much worse. Continue, please.",
            "Hmm.",
            "Acceptable.",
        };

        [Header("Look-at priorities")]
        public Transform player;
        public Transform orbInHand;
        public Transform shopShelf;

        [Header("Tuning")]
        [Range(2, 30)] public int blinkPerMin = 10;
        [Range(0.1f, 5f)] public float tailFlickDuration = 0.6f;
        [Range(0f, 10f)] public float idleSpotSwitchSeconds = 30f;

        private float _spotTimer;
        private int _spotIdx;
        private float _tailFlickTimer;
        private float _baseTailY;

        private void OnEnable()
        {
            EventBus.Subscribe<MemoryPolishedEvent>(OnPolished);
            EventBus.Subscribe<MoralChoiceMadeEvent>(OnChoice);
            if (tailBone != null) _baseTailY = tailBone.localEulerAngles.y;
            ChooseSpot();
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<MemoryPolishedEvent>(OnPolished);
            EventBus.Unsubscribe<MoralChoiceMadeEvent>(OnChoice);
        }

        private void Update()
        {
            _spotTimer += Time.deltaTime;
            if (_spotTimer >= idleSpotSwitchSeconds) { _spotTimer = 0f; ChooseSpot(); }

            if (_tailFlickTimer > 0f)
            {
                _tailFlickTimer -= Time.deltaTime;
                if (tailBone != null)
                {
                    float k = Mathf.Sin((_tailFlickTimer / tailFlickDuration) * Mathf.PI * 4f);
                    tailBone.localEulerAngles = new Vector3(tailBone.localEulerAngles.x, _baseTailY + k * 22f, tailBone.localEulerAngles.z);
                }
            }
            else if (tailBone != null)
            {
                tailBone.localEulerAngles = new Vector3(tailBone.localEulerAngles.x, _baseTailY, tailBone.localEulerAngles.z);
            }

            if (headBone != null)
            {
                Transform t = orbInHand != null ? orbInHand : player;
                if (t != null)
                {
                    var dir = t.position - headBone.position;
                    if (dir.sqrMagnitude > 0.01f)
                    {
                        var target = Quaternion.LookRotation(dir, Vector3.up);
                        headBone.rotation = Quaternion.Slerp(headBone.rotation, target, Time.deltaTime * 2.5f);
                    }
                }
            }
        }

        private void ChooseSpot()
        {
            if (sleepSpots == null || sleepSpots.Length == 0) return;
            _spotIdx = (_spotIdx + 1) % sleepSpots.Length;
            var sp = sleepSpots[_spotIdx];
            if (sp != null)
            {
                transform.position = sp.position;
                transform.rotation = sp.rotation;
            }
        }

        public void FlickTail() { _tailFlickTimer = tailFlickDuration; }

        public string PickLine(int index)
        {
            if (m1Lines == null || m1Lines.Length == 0) return string.Empty;
            return m1Lines[Mathf.Clamp(index, 0, m1Lines.Length - 1)];
        }

        private void OnPolished(MemoryPolishedEvent _)
        {
            FlickTail();
            Hh.Log(LogCategory.Pickle, "Pickle: tail flicked (Polish completed).");
        }

        private void OnChoice(MoralChoiceMadeEvent _)
        {
            FlickTail();
            Hh.Log(LogCategory.Pickle, "Pickle reacted to a moral choice.");
        }
    }
}
