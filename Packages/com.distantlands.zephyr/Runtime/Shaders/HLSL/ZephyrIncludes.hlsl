//Distant Lands | 2025

#ifndef ZEPHYR_WIND
#define ZEPHYR_WIND

CBUFFER_START(WindParams)
uniform float3 ZEPHYR_Direction;
uniform float ZEPHYR_BaseWindStrength;
uniform float ZEPHYR_GustStrength;
uniform float ZEPHYR_GustScale;
uniform float ZEPHYR_GustSpeed;
uniform float ZEPHYR_GustPullback;
uniform float3 ZEPHYR_GustingOffset;
uniform float ZEPHYR_FlutterStrength;
uniform float ZEPHYR_FlutterScale;
uniform float ZEPHYR_FlutterSpeed;
uniform float3 ZEPHYR_FlutterOffset;
CBUFFER_END

struct ZephyrWindZoneData
{
    float3 position;
    float3 direction;
    float radius;
    float strength;
    float variationTime;
    float variationMagnitude;
    float variationOffsetX;
    float variationOffsetY;
    int id;
    float auxOne;
    float auxTwo;
    float auxThree;
};

StructuredBuffer<ZephyrWindZoneData> _WindZones;

#ifndef ZEPHYR_NOISETEX
#define ZEPHYR_NOISETEX
TEXTURE2D(ZEPHYR_WindNoiseTexture);
SAMPLER(samplerZEPHYR_WindNoiseTexture);
#endif


#include "Packages/com.distantlands.zephyr/Runtime/Shaders/HLSL/Wind Zones/ApplyWindZones.hlsl"


void GetWindComponentsAtPoint(float3 position, out float3 windVector, out float3 baseWind, out float3 gust, out float3 flutter, out float3 windzones)
{
    #if defined(_ZEPHYR_USEINERTIA)
    windVector = _ZEPHYR_WindHistory;
    baseWind = _ZEPHYR_WindHistory;
    gust = 0;
    flutter = 0;
    windzones = 0;
    return;
    #endif

    float3 wind = 0;

    // Base wind
    baseWind = ZEPHYR_Direction * ZEPHYR_BaseWindStrength;

    // Calculate Gusting
    float2 gustUV = float2(
    (position.x + ZEPHYR_Direction.x + ZEPHYR_GustingOffset.x) / ZEPHYR_GustScale,
    (position.z + ZEPHYR_Direction.z + ZEPHYR_GustingOffset.z) / ZEPHYR_GustScale
    );

    float gustSample = ZEPHYR_WindNoiseTexture.SampleLevel(
    samplerZEPHYR_WindNoiseTexture,
    frac(gustUV),
    0
    ).r;

    float gusting = (gustSample - ZEPHYR_GustPullback) * 2.0 * ZEPHYR_GustStrength * ZEPHYR_BaseWindStrength;
    gust = gusting * ZEPHYR_Direction;

    // Calculate Flutter
    float2 flutterUV = float2(
    (position.x + ZEPHYR_Direction.x + ZEPHYR_FlutterOffset.x) / ZEPHYR_FlutterScale,
    (position.z + ZEPHYR_Direction.z + ZEPHYR_FlutterOffset.z) / ZEPHYR_FlutterScale
    );

    float flutterSample = ZEPHYR_WindNoiseTexture.SampleLevel(
    samplerZEPHYR_WindNoiseTexture,
    frac(flutterUV),
    0
    ).g;

    flutter = cross(ZEPHYR_Direction, float3(0, 1, 0)) * (flutterSample - 0.5) * 2.0 * ZEPHYR_FlutterStrength * ZEPHYR_BaseWindStrength;

    // Combine base, gust, flutter
    windVector = baseWind + gust + flutter;
    windzones = windVector;

    // -- - Apply Wind Zones -- -
    [unroll]
    for (int i = 0; i < 8; i ++)
    {
        ZephyrWindZoneData zone = _WindZones[i];

        if (zone.id == 0 || zone.radius <= 0) continue;

        float3 toPos = position - zone.position;
        float sqrDist = dot(toPos, toPos);
        if (sqrDist > zone.radius * zone.radius) continue;

        windVector = ApplyWindZone(windVector, zone, position, toPos, sqrDist);
    }

    windzones = windVector - windzones;


    //Check if length of the wind vector is too close to zero. If so, return 0. Otherwise, return the transformed vector
    float len = length(windVector);
    if (len > 0.0001) // small epsilon to avoid float issues
    {
        wind = TransformWorldToObjectDir(windVector / len) * len;
    }
    else
    {
        wind = 0; // zero vector in local space
    }

}

float3 GetWindAtPoint(float3 position)
{
    float3 windVector, baseWind, gust, flutter, windzones;
    GetWindComponentsAtPoint(position, windVector, baseWind, gust, flutter, windzones);
    return windVector;
}

#endif