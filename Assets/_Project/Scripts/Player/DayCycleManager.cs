// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Player / DayCycleManager
//
// Drives time-of-day from "Morning" to "Evening" within a single mission
// scene. Lerps the sun's rotation + intensity + ambient color in editor-set
// curves. In Mission 1 the scene transitions Afternoon → Evening during the
// shop-counter beat.

using System;
using UnityEngine;
using HearthboundHollow.Core;

namespace HearthboundHollow.Player
{
    public enum TimeOfDay { Morning, Afternoon, Evening, Night }

    [ExecuteAlways]
    public class DayCycleManager : MonoBehaviour
    {
        [Header("Sun")]
        public Light sun;

        [Header("Time")]
        [Range(0f, 1f)] public float dayProgress01 = 0.6f;
        public bool autoAdvance = false;
        public float secondsPerDay = 600f;

        [Header("Sun rotation curve (X angle 0–1)")]
        public AnimationCurve sunXAngleByProgress = AnimationCurve.Linear(0f, 5f, 1f, 75f);

        [Header("Sun intensity curve")]
        public AnimationCurve sunIntensityByProgress = AnimationCurve.EaseInOut(0f, 0.6f, 1f, 1.2f);

        [Header("Sun color")]
        public Gradient sunColor;

        [Header("Ambient")]
        public Gradient ambientColor;

        public event Action<TimeOfDay> OnTimeOfDayChanged;
        public TimeOfDay CurrentTimeOfDay { get; private set; }

        private TimeOfDay _lastBroadcast;

        private void Update()
        {
            if (autoAdvance && Application.isPlaying)
                dayProgress01 = Mathf.Repeat(dayProgress01 + (Time.deltaTime / Mathf.Max(0.01f, secondsPerDay)), 1f);
            ApplyVisuals();

            CurrentTimeOfDay = ProgressToTimeOfDay(dayProgress01);
            if (CurrentTimeOfDay != _lastBroadcast)
            {
                _lastBroadcast = CurrentTimeOfDay;
                OnTimeOfDayChanged?.Invoke(CurrentTimeOfDay);
                Hh.Log(LogCategory.Performance, $"TimeOfDay → {CurrentTimeOfDay} (progress {dayProgress01:F2})");
            }
        }

        private void ApplyVisuals()
        {
            if (sun != null)
            {
                float x = sunXAngleByProgress.Evaluate(dayProgress01);
                sun.transform.rotation = Quaternion.Euler(x, 30f, 0f);
                sun.intensity = sunIntensityByProgress.Evaluate(dayProgress01);
                if (sunColor != null && sunColor.colorKeys != null && sunColor.colorKeys.Length > 0)
                    sun.color = sunColor.Evaluate(dayProgress01);
            }
            if (ambientColor != null && ambientColor.colorKeys != null && ambientColor.colorKeys.Length > 0)
                RenderSettings.ambientLight = ambientColor.Evaluate(dayProgress01);
        }

        private static TimeOfDay ProgressToTimeOfDay(float p)
        {
            if (p < 0.25f) return TimeOfDay.Morning;
            if (p < 0.55f) return TimeOfDay.Afternoon;
            if (p < 0.85f) return TimeOfDay.Evening;
            return TimeOfDay.Night;
        }

        public void SetTimeOfDay(TimeOfDay tod)
        {
            dayProgress01 = tod switch
            {
                TimeOfDay.Morning => 0.15f,
                TimeOfDay.Afternoon => 0.40f,
                TimeOfDay.Evening => 0.70f,
                TimeOfDay.Night => 0.92f,
                _ => 0.40f,
            };
        }
    }
}
