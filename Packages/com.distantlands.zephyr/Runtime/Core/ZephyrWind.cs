// Distant Lands 2025
// Main Zephyr Manager
// All contents in this file are protected by the Unity Asset Store EULA

using System;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using WindZone = DistantLands.Zephyr.ZephyrWindZone;

#if THE_VISUAL_ENGINE
using TheVisualEngine;
#endif


namespace DistantLands.Zephyr
{
    [AddComponentMenu("Distant Lands/Zephyr/Wind Manager", -10)]
    [ExecuteAlways]
    public class ZephyrWind : MonoBehaviour
    {
        /// <summary>
        /// Access the current wind direction. To set with smoothness, use TargetDirection instead.
        /// </summary>
        public Vector3 Direction
        {
            get
            {
                return direction;
            }
            set
            {
                switch (directionSource)
                {
                    case DirectionSource.TransformForward:
                        transform.forward = value;
                        break;
                    default:
                        direction = value;
                        break;
                }
            }
        }
        /// <summary>
        /// Access the current target wind direction. To access the current wind direction, use Direction instead.
        /// </summary>
        public Vector3 TargetDirection
        {
            get
            {
                switch (directionSource)
                {
                    case DirectionSource.TransformForward:
                        return transform.forward;
                    case DirectionSource.GlobalVector:
                        return targetDirection.normalized;
                    case DirectionSource.LocalVector:
                        return transform.TransformDirection(targetDirection).normalized;
                    default:
                        return transform.forward;
                }
            }
            set
            {
                switch (directionSource)
                {
                    case DirectionSource.TransformForward:
                        transform.forward = value;
                        break;
                    default:
                        targetDirection = value;
                        break;
                }
            }
        }
        [Tooltip("Specifies the current direction of the wind")]
        [SerializeField]
        private Vector3 direction;
        [Tooltip("Specifies the target direction of the wind. The wind will shift to match this direction over time.")]
        [SerializeField]
        private Vector3 targetDirection;
        /// <summary>
        /// Transform Forward - set the wind direction based on the current forward vector (+Z) of the Zephyr Manager.
        /// Global Vector - set the wind direction to the targetDirection vector.
        /// Local Vector - transforms the targetDirection vector into local space and then sets the wind direction to that new vector.
        /// </summary>
        public enum DirectionSource { TransformForward, GlobalVector, LocalVector }
        /// <summary>
        /// Set the source of the wind direction.
        /// </summary>
        [Tooltip("Set the source of the wind direction.")]
        public DirectionSource directionSource;
        /// <summary>
        /// Sets an acceleration for the wind delta. The wind will use this for changing speed and direction (on the global wind manager only)
        /// </summary>
        [Tooltip("Sets an acceleration for the wind delta. The wind will use this for changing speed and direction (on the global wind manager only).")]
        public float windAccelerationCoefficient = 4;
        /// <summary>
        /// Gets the current wind strength.
        /// </summary>
        public float WindStrength => currentWindStrength;
        /// <summary>
        /// Gets the target wind strength.
        /// </summary>
        [Tooltip("Sets the target wind speed. Use a 0-5 scale for the best results!")]
        public float targetWindStrength = 1f;
        private float currentWindStrength = 1f;
        /// <summary>
        /// Sets the current gust strength. This is multiplied by the current wind strength.
        /// </summary>
        [Tooltip("Sets the current gust strength. This is multiplied by the current wind strength.")]
        public float gustStrength = 1f;
        /// <summary>
        /// Sets the scale of the gust noise texture.
        /// </summary>
        [Tooltip("Sets the scale of the gust noise texture.")]
        public float gustScale = 150f;
        /// <summary>
        /// Sets the speed of the scrolling for the gust noise texture.
        /// </summary>
        [Tooltip("Sets the speed of the scrolling for the gust noise texture.")]
        public float gustSpeed = 2f;
        /// <summary>
        /// Adds a "pullback" modifier to the gust force. This lets low gust values <i>subtract</i> from the main wind resulting in chaotic wind.
        /// </summary>
        [Tooltip("Adds a 'pullback' modifier to the gust force. This lets low gust values <i>subtract</i> from the main wind resulting in chaotic wind.")]
        [Range(0, 1)]
        public float gustPullback = 0.25f;
        private Vector3 gustingOffset;
        /// <summary>
        /// Sets the noise texture used for gusting and flutter.
        /// </summary>
        [Tooltip("Sets the noise texture used for gusting and flutter.")]
        public Texture2D windNoiseTexture;
        /// <summary>
        /// Sets the strength of a flutter added to the wind. Flutter is a perpendicular force added to the main wind vector that simulates wind curling.
        /// </summary>
        [Tooltip("Sets the strength of a flutter added to the wind. Flutter is a perpendicular force added to the main wind vector that simulates wind curling.")]
        public float flutterStrength = 0.2f;
        /// <summary>
        /// Sets the scale of the flutter noise texture.
        /// </summary>
        [Tooltip("Sets the scale of the flutter noise texture.")]
        public float flutterScale = 60f;
        /// <summary>
        /// Sets the speed of the flutter noise texture.
        /// </summary>
        [Tooltip("Sets the speed of the flutter noise texture.")]
        public float flutterSpeed = 1f;
        private Vector3 flutterOffset;

        [Tooltip("The maximum number of wind zones that can affect a point at once.")]
        /// <summary>
        /// Sets the maximum amount of wind zones calculated on the CPU.
        /// </summary>
        public int maxWindZones = 8;
        /// <summary>
        /// The radius around the world center (usually the camera) to search for active wind zones.
        /// </summary>
        [Tooltip("The radius around the world center (usually the camera) to search for active wind zones.")]
        public float windZoneSearchRadius = 500f;

        private Vector3 worldCenter;
        private static ZephyrWind instance;
        public static ZephyrWind Instance
        {
            get
            {
                if (!instance)
                {
                    instance = FindFirstObjectByType<ZephyrWind>();
                }
                return instance;
            }
        }

        private PointOctree<WindZone> windZoneOctree;
        List<WindZone> nearbyZones;
        private WindZone[] activeWindZones;
        private float[] activeDists;
        private bool updatingWindZones = false;

        private ZephyrWindZoneData[] windZoneDataArray;
        private ComputeBuffer windBuffer;


        private enum DebugMode { Off, Magnitude, Direction, Components }
        [SerializeField] private DebugMode debugMode = DebugMode.Off;
        [SerializeField] private float debugScale = 1;
        static Gradient WindSpeedColor => new Gradient()
        {
            colorKeys = new GradientColorKey[] {
                new GradientColorKey(Color.blue, 0f),
                new GradientColorKey(Color.green, 0.5f),
                new GradientColorKey(Color.red, 1f),
            }
        };

#if THE_VISUAL_ENGINE
        private TVEManager tveManager;
#endif

        void OnEnable()
        {
            gustingOffset = Vector3.zero;
            flutterOffset = Vector3.zero;

            windZoneOctree = new PointOctree<WindZone>(2000, Vector3.zero);
            StartCoroutine(UpdateWindZones());
            activeWindZones = new WindZone[maxWindZones];
            activeDists = new float[maxWindZones];

            windZoneDataArray = new ZephyrWindZoneData[maxWindZones];
            windBuffer = new ComputeBuffer(maxWindZones, System.Runtime.InteropServices.Marshal.SizeOf(typeof(ZephyrWindZoneData)));
            Shader.SetGlobalBuffer("_WindZones", windBuffer);

#if UNITY_EDITOR
            EditorApplication.update += UpdateWindInEditor;
#endif

        }


        void WriteToGPU()
        {
            Shader.SetGlobalVector(ZephyrShaderIDs.Direction, Direction);
            Shader.SetGlobalFloat(ZephyrShaderIDs.BaseWindStrength, WindStrength);
            Shader.SetGlobalFloat(ZephyrShaderIDs.GustStrength, gustStrength);
            Shader.SetGlobalFloat(ZephyrShaderIDs.GustScale, gustScale);
            Shader.SetGlobalFloat(ZephyrShaderIDs.GustSpeed, gustSpeed);
            Shader.SetGlobalVector(ZephyrShaderIDs.GustingOffset, gustingOffset);
            Shader.SetGlobalFloat(ZephyrShaderIDs.GustPullback, gustPullback);
            Shader.SetGlobalTexture(ZephyrShaderIDs.WindNoiseTexture, windNoiseTexture);
            Shader.SetGlobalFloat(ZephyrShaderIDs.FlutterStrength, flutterStrength);
            Shader.SetGlobalFloat(ZephyrShaderIDs.FlutterScale, flutterScale);
            Shader.SetGlobalFloat(ZephyrShaderIDs.FlutterSpeed, flutterSpeed);
            Shader.SetGlobalVector(ZephyrShaderIDs.FlutterOffset, flutterOffset);

            windBuffer.SetData(windZoneDataArray);
        }

        /// <summary>
        /// Gets all components of the current wind vector at a particular point.
        /// </summary>
        /// <param name="position">The world-space position to test the current wind.</param>
        /// <param name="windVector">The final combined wind vector.</param>
        /// <param name="baseWind">Isolates the base wind</param>
        /// <param name="gusting">Isolates the gusting wind</param>
        /// <param name="fluttering">Isolates the fluttering wind</param>
        /// <param name="windZones">Isolates the wind from windzones</param>
        public void GetWindComponentsAtPoint(Vector3 position, out Vector3 windVector, out Vector3 baseWind, out Vector3 gusting, out Vector3 fluttering, out Vector3 windZones)
        {
            baseWind = Direction.normalized * currentWindStrength;

            // Sample gusting from the texture
            Color gustSample = SampleWindNoiseTexture(
                (position.x + gustingOffset.x) / gustScale,
                (position.z + gustingOffset.z) / gustScale
            );

            // Sample flutter from the texture
            Color flutterSample = SampleWindNoiseTexture(
                (position.x + flutterOffset.x) / flutterScale,
                (position.z + flutterOffset.z) / flutterScale
            );

            // Using red channel for gust value
            gusting = (gustSample.r - gustPullback) * 2f * gustStrength * currentWindStrength * Direction.normalized;

            //Green value for fluttering
            fluttering = Vector3.Cross(Direction, Vector3.up).normalized * (flutterSample.g - 0.5f) * 2f * flutterStrength * currentWindStrength;

            windVector = baseWind + gusting + fluttering;
            windZones = -windVector;

            foreach (WindZone zone in activeWindZones)
                zone?.ApplyWind(position, ref windVector);

            windZones += windVector;

        }

        /// <summary>
        /// Gets the current wind vector at a particular point.
        /// </summary>
        /// <param name="position">The world-space position to test the current wind.</param>
        public Vector3 GetWindAtPoint(Vector3 position)
        {
            GetWindComponentsAtPoint(position, out Vector3 windVector, out Vector3 baseWind, out Vector3 gusting, out Vector3 fluttering, out Vector3 windZones);
            return windVector;
        }

        /// <summary>
        /// Gets the current wind vector at a particular point <i>without</i> windzones for faster calculation.
        /// </summary>
        /// <param name="position">The world-space position to test the current wind.</param>
        public Vector3 FastWindCalculation(Vector3 position)
        {
            Vector3 baseWind = Direction.normalized * currentWindStrength;

            // Sample gusting from the texture
            Color gustSample = SampleWindNoiseTexture(
                (position.x + gustingOffset.x) / gustScale,
                (position.z + gustingOffset.z) / gustScale
            );

            // Sample flutter from the texture
            Color flutterSample = SampleWindNoiseTexture(
                (position.x + flutterOffset.x) / flutterScale,
                (position.z + flutterOffset.z) / flutterScale
            );

            // Using red channel for gust value
            float gusting = (gustSample.r - gustPullback) * 2f * gustStrength * currentWindStrength;
            Vector3 directionalGusting = gusting * Direction.normalized;

            //Green value for fluttering
            Vector3 fluttering = Vector3.Cross(Direction, Vector3.up).normalized * (flutterSample.g - 0.5f) * 2f * flutterStrength * currentWindStrength;

            Vector3 windVector = baseWind + directionalGusting + fluttering;

            return windVector;
        }

        Color SampleWindNoiseTexture(float x, float y)
        {
            if (!windNoiseTexture)
                return Color.black;

            float u = Mathf.Repeat(x, 1f);
            float v = Mathf.Repeat(y, 1f);

            return windNoiseTexture.GetPixelBilinear(u, v);
        }

        Color SampleWindNoiseTexture(Vector2 position)
        {
            return SampleWindNoiseTexture(position.x, position.y);
        }

        internal void AddWindZone(WindZone zone)
        {
            if (windZoneOctree == null)
                windZoneOctree = new PointOctree<WindZone>(2000, Vector3.zero);

            windZoneOctree.Add(zone, zone.transform.position);
        }

        internal void RemoveWindZone(WindZone zone)
        {
            windZoneOctree.Remove(zone);
        }

        private IEnumerator UpdateWindZones()
        {
            updatingWindZones = true;
            yield return null;

            List<WindZone> allZones = windZoneOctree.GetAllItems();
            windZoneOctree.Clear();

            foreach (var zone in allZones)
            {
                if (zone != null)
                {
                    windZoneOctree.Add(zone, zone.transform.position);
                }
            }

            updatingWindZones = false;
        }

        void UpdateWindZoneLOD()
        {
            System.Array.Clear(activeWindZones, 0, activeWindZones.Length);
            int count = 0;

            nearbyZones = windZoneOctree.GetNearby(worldCenter, windZoneSearchRadius);

            foreach (WindZone wz in nearbyZones)
            {
                if (wz == null) continue;

                Vector3 pos = wz.transform.position;
                float distSq = (pos - worldCenter).sqrMagnitude;

                if (count < maxWindZones)
                {
                    InsertSorted(ref count, wz, distSq);
                }
                else if (distSq < activeDists[count - 1] ||
                         (distSq == activeDists[count - 1] && wz.ApplicationOrder < activeWindZones[count - 1].ApplicationOrder))
                {
                    count--;
                    InsertSorted(ref count, wz, distSq);
                }
            }

            for (int i = 0; i < maxWindZones; i++)
            {
                if (activeWindZones[i] != null)
                {
                    windZoneDataArray[i].position = activeWindZones[i].Position;
                    windZoneDataArray[i].direction = activeWindZones[i].Direction;
                    windZoneDataArray[i].radius = activeWindZones[i].Radius;
                    windZoneDataArray[i].strength = activeWindZones[i].Strength;
                    windZoneDataArray[i].variationMagnitude = activeWindZones[i].VariationMagnitude;
                    windZoneDataArray[i].variationTime = activeWindZones[i].VariationTime;
                    windZoneDataArray[i].variationOffsetX = activeWindZones[i].VariationOffsetX;
                    windZoneDataArray[i].VariationOffsetY = activeWindZones[i].VariationOffsetY;
                    windZoneDataArray[i].id = activeWindZones[i].ID;
                    windZoneDataArray[i].auxOne = activeWindZones[i].AuxOne;
                    windZoneDataArray[i].auxTwo = activeWindZones[i].AuxTwo;
                    windZoneDataArray[i].auxThree = activeWindZones[i].AuxThree;
                }
                else
                {
                    // Fill with zeros if no zone
                    windZoneDataArray[i] = default;
                }
            }

        }

        private void InsertSorted(ref int count, WindZone wz, float distSq)
        {
            int i = count - 1;
            while (i >= 0)
            {
                if (distSq > activeDists[i] ||
                    (distSq == activeDists[i] && wz.ApplicationOrder >= activeWindZones[i].ApplicationOrder))
                    break;

                activeWindZones[i + 1] = activeWindZones[i];
                activeDists[i + 1] = activeDists[i];
                i--;
            }

            activeWindZones[i + 1] = wz;
            activeDists[i + 1] = distSq;
            count++;
        }

        void Update()
        {
            if (Application.isPlaying)
                UpdateWindSimulation();
        }

#if UNITY_EDITOR
        void UpdateWindInEditor()
        {
            if (Application.isPlaying) return;
            if (SceneView.lastActiveSceneView != null && SceneView.lastActiveSceneView.hasFocus)
            {
                currentWindStrength = targetWindStrength;
                direction = TargetDirection;

                gustingOffset -= gustSpeed * 0.03f * Direction;
                flutterOffset -= flutterSpeed * 0.03f * Direction;
                gustingOffset = new Vector3(Mathf.Repeat(gustingOffset.x, gustScale), Mathf.Repeat(gustingOffset.y, gustScale), Mathf.Repeat(gustingOffset.z, gustScale));
                flutterOffset = new Vector3(Mathf.Repeat(flutterOffset.x, flutterScale), Mathf.Repeat(flutterOffset.y, flutterScale), Mathf.Repeat(flutterOffset.z, flutterScale));

                if (Camera.main != null)
                {
                    worldCenter = Camera.main.transform.position;
                }

                UpdateWindZoneLOD();
                WriteToGPU();
#if THE_VISUAL_ENGINE
            UpdateTVE();
#endif
            }
        }
#endif

        void UpdateWindSimulation()
        {
            UpdateWindDirection();

            gustingOffset -= gustSpeed * currentWindStrength * Time.deltaTime * Direction;
            flutterOffset -= flutterSpeed * currentWindStrength * Time.deltaTime * Direction;
            gustingOffset = new Vector3(Mathf.Repeat(gustingOffset.x, gustScale), Mathf.Repeat(gustingOffset.y, gustScale), Mathf.Repeat(gustingOffset.z, gustScale));
            flutterOffset = new Vector3(Mathf.Repeat(flutterOffset.x, flutterScale), Mathf.Repeat(flutterOffset.y, flutterScale), Mathf.Repeat(flutterOffset.z, flutterScale));

            if (Camera.main != null)
            {
                worldCenter = Camera.main.transform.position;
            }

            UpdateWindZoneLOD();
            WriteToGPU();
#if THE_VISUAL_ENGINE
            UpdateTVE();
#endif
        }

        void UpdateWindDirection()
        {
            direction += TargetDirection * Time.deltaTime * windAccelerationCoefficient;

            direction.Normalize();

            if (targetWindStrength != currentWindStrength)
            {
                if (Mathf.Abs(targetWindStrength - currentWindStrength) < Time.deltaTime * windAccelerationCoefficient * 0.1f)
                {
                    currentWindStrength = targetWindStrength;
                }
                else
                {
                    if (targetWindStrength > currentWindStrength)
                    {
                        currentWindStrength += Time.deltaTime * windAccelerationCoefficient * 0.1f;
                    }
                    else
                    {
                        currentWindStrength -= Time.deltaTime * windAccelerationCoefficient * 0.1f;
                    }
                }
            }
        }

#if THE_VISUAL_ENGINE

        void UpdateTVE()
        {
            if (!tveManager)
            {
                if (TVEManager.Instance)
                    tveManager = TVEManager.Instance;
                else
                    return;
            }

            tveManager.motionControl = Mathf.Clamp01(WindStrength / 5f);
            tveManager.transform.forward = Direction;
        }

#endif

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            Vector3[] points = new Vector3[] {
                new Vector3(0f,0f,2f),
                new Vector3(1f,0f,-1f),
                new Vector3(1f,0f,-1f),
                new Vector3(0f,0f,0f),
                new Vector3(0f,0f,0f),
                new Vector3(-1,0,-1),
                new Vector3(-1,0,-1),
                new Vector3(0f,0f,2f),
            };
            Handles.color = Color.white;
            Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, Direction.normalized);

            // Apply the rotation to each point
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = transform.position + rotation * points[i];
            }

            Handles.DrawLines(points);

            if (debugMode != DebugMode.Off)
            {
                for (int x = -10; x < 10; x++)
                {
                    for (int z = -10; z < 10; z++)
                    {
                        Vector3 pos = transform.position + new Vector3(x, 0, z) * debugScale;
                        GetWindComponentsAtPoint(pos, out Vector3 windVector, out Vector3 baseWind, out Vector3 gusting, out Vector3 fluttering, out Vector3 windZones);
                        switch (debugMode)
                        {
                            case DebugMode.Direction:
                                Vector3 normal = windVector.normalized;
                                Handles.color = new Color((normal.x + 1) * 0.5f, (normal.y + 1) * 0.5f, (normal.z + 1) * 0.5f);
                                Handles.DrawLine(pos, pos + windVector * 0.5f);
                                break;
                            case DebugMode.Components:
                                Handles.color = new Color(0.93f, 0.12f, 0.23f);
                                Handles.DrawLine(pos, pos + baseWind * 0.5f);
                                Handles.color = new Color(0.13f, 0.22f, 0.93f);
                                Handles.DrawLine(pos, pos + gusting * 0.5f);
                                Handles.color = new Color(0.13f, 0.92f, 0.23f);
                                Handles.DrawLine(pos, pos + fluttering * 0.5f);
                                Handles.color = new Color(0.33f, 0.52f, 0.93f);
                                Handles.DrawLine(pos, pos + windZones * 0.5f);
                                break;
                            default:
                                if (windVector.magnitude > 0 && windVector.magnitude < 100)
                                    Handles.color = WindSpeedColor.Evaluate(Mathf.Clamp01(windVector.magnitude / 5f));
                                Handles.DrawLine(pos, pos + windVector * 0.5f);
                                break;
                        }

                    }
                }

            }

            if (SceneView.lastActiveSceneView != null && SceneView.lastActiveSceneView.hasFocus)
            {
                SceneView.lastActiveSceneView.Repaint();
            }

        }
#endif

        void OnDisable()
        {
            if (windBuffer != null)
            {
                windBuffer.Release();
                windBuffer = null;
            }


            Shader.SetGlobalVector(ZephyrShaderIDs.Direction, Direction);
            Shader.SetGlobalFloat(ZephyrShaderIDs.BaseWindStrength, 0);
            Shader.SetGlobalFloat(ZephyrShaderIDs.GustStrength, 0);
            Shader.SetGlobalFloat(ZephyrShaderIDs.GustPullback, 0);
            Shader.SetGlobalTexture(ZephyrShaderIDs.WindNoiseTexture, windNoiseTexture);
            Shader.SetGlobalFloat(ZephyrShaderIDs.FlutterStrength, 0);

#if UNITY_EDITOR
            EditorApplication.update -= UpdateWindInEditor;
#endif
        }

    }
}
