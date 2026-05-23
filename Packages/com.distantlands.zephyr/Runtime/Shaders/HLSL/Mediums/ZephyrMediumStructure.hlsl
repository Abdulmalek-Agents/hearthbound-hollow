//Distant Lands | 2025

#ifndef ZEPHYR_MEDIUM_STRUCTURE
#define ZEPHYR_MEDIUM_STRUCTURE


#include "Packages/com.distantlands.zephyr/Runtime/Shaders/HLSL/ZephyrIncludes.hlsl"
#include "Packages/com.distantlands.zephyr/Runtime/Shaders/HLSL/ZephyrUtils.hlsl"

struct StructureMediumParameters
{
    float windMask;
    float windStrength;
    float windScale;
    float windSpeed;
};

float3 ApplyStructureMedium(float3 worldPos, float3 objectPos, StructureMediumParameters params)
{
    // Get wind components from Zephyr
    float3 wind, baseWind, gustWind, flutterWind, windzonesWind;
    GetWindComponentsAtPoint(worldPos, wind, baseWind, gustWind, flutterWind, windzonesWind);
    float windLength = length(wind);
    float baseWindLength = length(baseWind);
    float3 windDir = TransformWorldToObjectDir(normalize(wind + float3(0.0001, 0, 0)), true);

    float3 transformPosition = TransformObjectToWorld(float3(0, 0, 0));

    //Base Wind
    float baseWindSine = LayeredSineWave(transformPosition.x + transformPosition.y + transformPosition.z + _Time.y * params.windSpeed, params.windScale, baseWindLength, 1.0, 16.0);
    float baseWindRemap = lerp(0.2, 1.0, (baseWindSine + 1.0) * 0.5); // Remap from [ - 1, 1] to [0.2, 1]
    float baseWindStrength = baseWindRemap * params.windStrength * params.windMask * baseWindLength * 0.2;
    float3 baseWindCombined = windDir * baseWindStrength + abs(baseWindStrength) * float3(0.0, - 0.1, 0.0);

    return baseWindCombined;
}


void ApplyStructureMedium_float(float3 worldPos, float3 objectPos,
float windMask, float windStrength, float windScale, float windSpeed,
out float3 output
) {

    StructureMediumParameters params = (StructureMediumParameters)0;
    params.windMask = windMask;
    params.windStrength = windStrength;
    params.windScale = windScale;
    params.windSpeed = windSpeed;

    output = ApplyStructureMedium(worldPos, objectPos, params);

}

void ApplyStructureMedium_half(half3 worldPos, half3 objectPos,
half windMask, half windStrength, half windScale, half windSpeed,
out half3 output
) {
    StructureMediumParameters params = (StructureMediumParameters)0;
    params.windMask = windMask;
    params.windStrength = windStrength;
    params.windScale = windScale;
    params.windSpeed = windSpeed;

    output = ApplyStructureMedium(worldPos, objectPos, params);
}


#endif