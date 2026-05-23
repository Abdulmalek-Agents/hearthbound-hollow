// Distant Lands 2025
// All contents in this file are protected by the Unity Asset Store EULA

using UnityEngine;

namespace DistantLands.Zephyr
{
    /// <summary>
    /// Provides cached shader property IDs for all Zephyr shader variables.
    /// Use these properties to avoid repeated string lookups when setting shader values.
    /// </summary>
    public static class ZephyrShaderIDs
    {
        static int _DirectionID;
        public static int Direction
        {
            get
            {
                if (_DirectionID == 0)
                    _DirectionID = Shader.PropertyToID("ZEPHYR_Direction");
                return _DirectionID;
            }
        }
        static int _BaseWindStrengthID;
        public static int BaseWindStrength
        {
            get
            {
                if (_BaseWindStrengthID == 0)
                    _BaseWindStrengthID = Shader.PropertyToID("ZEPHYR_BaseWindStrength");
                return _BaseWindStrengthID;
            }
        }

        static int _GustStrengthID;
        public static int GustStrength
        {
            get
            {
                if (_GustStrengthID == 0)
                    _GustStrengthID = Shader.PropertyToID("ZEPHYR_GustStrength");
                return _GustStrengthID;
            }
        }

        static int _GustScaleID;
        public static int GustScale
        {
            get
            {
                if (_GustScaleID == 0)
                    _GustScaleID = Shader.PropertyToID("ZEPHYR_GustScale");
                return _GustScaleID;
            }
        }

        static int _GustSpeedID;
        public static int GustSpeed
        {
            get
            {
                if (_GustSpeedID == 0)
                    _GustSpeedID = Shader.PropertyToID("ZEPHYR_GustSpeed");
                return _GustSpeedID;
            }
        }

        static int _GustingOffsetID;
        public static int GustingOffset
        {
            get
            {
                if (_GustingOffsetID == 0)
                    _GustingOffsetID = Shader.PropertyToID("ZEPHYR_GustingOffset");
                return _GustingOffsetID;
            }
        }
        static int _GustPullbackID;
        public static int GustPullback
        {
            get
            {
                if (_GustPullbackID == 0)
                    _GustPullbackID = Shader.PropertyToID("ZEPHYR_GustPullback");
                return _GustPullbackID;
            }
        }

        static int _WindNoiseTextureID;
        public static int WindNoiseTexture
        {
            get
            {
                if (_WindNoiseTextureID == 0)
                    _WindNoiseTextureID = Shader.PropertyToID("ZEPHYR_WindNoiseTexture");
                return _WindNoiseTextureID;
            }
        }

        static int _FlutterStrengthID;
        public static int FlutterStrength
        {
            get
            {
                if (_FlutterStrengthID == 0)
                    _FlutterStrengthID = Shader.PropertyToID("ZEPHYR_FlutterStrength");
                return _FlutterStrengthID;
            }
        }

        static int _FlutterScaleID;
        public static int FlutterScale
        {
            get
            {
                if (_FlutterScaleID == 0)
                    _FlutterScaleID = Shader.PropertyToID("ZEPHYR_FlutterScale");
                return _FlutterScaleID;
            }
        }

        static int _FlutterSpeedID;
        public static int FlutterSpeed
        {
            get
            {
                if (_FlutterSpeedID == 0)
                    _FlutterSpeedID = Shader.PropertyToID("ZEPHYR_FlutterSpeed");
                return _FlutterSpeedID;
            }
        }

        static int _FlutterOffsetID;
        public static int FlutterOffset
        {
            get
            {
                if (_FlutterOffsetID == 0)
                    _FlutterOffsetID = Shader.PropertyToID("ZEPHYR_FlutterOffset");
                return _FlutterOffsetID;
            }
        }


    }
}