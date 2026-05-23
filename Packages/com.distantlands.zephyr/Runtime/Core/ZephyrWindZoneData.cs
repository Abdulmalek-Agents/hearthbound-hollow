// Distant Lands 2025
// Struct that holds wind zone data 
// All contents in this file are protected by the Unity Asset Store EULA

using UnityEngine;

namespace DistantLands.Zephyr
{
    /// <summary>
    /// Struct that holds wind zone data for Zephyr wind system.
    /// </summary>
    public struct ZephyrWindZoneData
    {
        /// <summary>World position of the wind zone.</summary>
        [Tooltip("World position of the wind zone.")]
        public Vector3 position;
        /// <summary>Direction vector of the wind zone.</summary>
        [Tooltip("Direction vector of the wind zone.")]
        public Vector3 direction;
        /// <summary>Radius of the wind zone's effect.</summary>
        [Tooltip("Radius of the wind zone's effect.")]
        public float radius;
        /// <summary>Strength of the wind zone.</summary>
        [Tooltip("Strength of the wind zone.")]
        public float strength;
        /// <summary>Variation frequency for wind zone.</summary>
        [Tooltip("Variation frequency for wind zone.")]
        public float variationTime;
        /// <summary>Variation magnitude for wind zone.</summary>
        [Tooltip("Variation magnitude for wind zone.")]
        public float variationMagnitude;
        /// <summary>Variation offset X for wind zone.</summary>
        [Tooltip("Variation offset X for wind zone.")]
        public float variationOffsetX;
        /// <summary>Variation offset Y for wind zone.</summary>
        [Tooltip("Variation offset Y for wind zone.")]
        public float VariationOffsetY;
        /// <summary>Unique ID for the wind zone.</summary>
        [Tooltip("Unique ID for the wind zone.")]
        public int id;
        /// <summary>Auxiliary value one for custom data.</summary>
        [Tooltip("Auxiliary value one for custom data.")]
        public float auxOne;
        /// <summary>Auxiliary value two for custom data.</summary>
        [Tooltip("Auxiliary value two for custom data.")]
        public float auxTwo;
        /// <summary>Auxiliary value three for custom data.</summary>
        [Tooltip("Auxiliary value three for custom data.")]
        public float auxThree;
    }
}