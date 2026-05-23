//Distant Lands | 2025

#ifndef ZEPHYR_MEDIUM_VEGETATION
#define ZEPHYR_MEDIUM_VEGETATION


#include "Packages/com.distantlands.zephyr/Runtime/Shaders/HLSL/ZephyrIncludes.hlsl"
#include "Packages/com.distantlands.zephyr/Runtime/Shaders/HLSL/ZephyrUtils.hlsl"

struct VegetationMediumParameters
{
    float windMask;
    float windStrength;
    float phaseOffset;
    float flutterMask;
    float flutterIntensity;
    float flutterScale;
    float flutterSpeed;
    float rotationMask;
    float rotationIntensity;
    float rotationScale;
    float rotationSpeed;
};

float3 ApplyVegetationMedium(float3 worldPos, float3 objectPos, VegetationMediumParameters params)
{
    // Get wind components from Zephyr
    float3 wind, baseWind, gustWind, flutterWind, windzonesWind;
    GetWindComponentsAtPoint(worldPos, wind, baseWind, gustWind, flutterWind, windzonesWind);
    float windLen = min(length(wind), 5);
    float windStrength = params.windStrength * params.windMask * windLen;
    float3 windDir = TransformWorldToObjectDir(normalize(wind + float3(0.0001, 0, 0)), true);

    // Add time offset to world position for flutter
    float3 flutterPos = worldPos + (_Time.x.xxx * params.flutterSpeed) + (params.phaseOffset.xxx);

    // Flutter (layered sine wave)
    float flutterSine = LayeredSineWave(flutterPos.x, params.flutterScale, windLen, 1.0, 16.0);
    float flutterRemap = lerp(- 0.5, 1.0, (flutterSine + 1.0) * 0.5); // Remap from [ - 1, 1] to [0.2, 1]
    float flutter = flutterRemap * params.flutterIntensity * 0.1 * params.windMask * windLen;


    // Combine wind and flutter
    float3 windVec = windDir * windStrength;
    float3 flutterVec = windDir * flutter;
    float3 result = windVec + flutterVec;

    // Rotation (layered sine wave with phase offset)
    float3 rotPos = TransformObjectToWorld(float3(0, 0, 0)) + (_Time.x.xxx * params.rotationSpeed) + (params.phaseOffset.xxx);
    float rotSine = LayeredSineWave(rotPos.x, params.rotationScale, windLen, 1.0, 16.0);
    float rotation = params.rotationIntensity * rotSine * params.rotationMask * windLen * windLen * 0.01;
    float3 axis = cross(windDir, float3(0, 1, 0));
    float3 rotated = RotateAboutAxis(objectPos, float3(0, 1, 0), rotation);

    float rotationBounceWindSine = LayeredSineWave(rotPos.x, params.rotationScale * 0.5, windLen, 1.0, 16.0);
    float3 rotationBounce = float3(0, 1, 0) * rotationBounceWindSine * windLen * params.rotationMask * params.rotationIntensity * .005;

    float3 rotatedWindCombined = rotationBounce + rotated;
    result += (rotatedWindCombined);

    return NormalizePosition(float3(0, 0, 0), objectPos, result) - objectPos;
}

void ApplyVegetationMedium_float(float3 worldPos, float3 objectPos,
float windMask, float windStrength, float phaseOffset,
float flutterMask, float flutterIntensity, float flutterScale, float flutterSpeed,
float rotationMask, float rotationIntensity, float rotationScale, float rotationSpeed,
out float3 windVector)
{
    VegetationMediumParameters params = (VegetationMediumParameters)0;
    params.windMask = windMask;
    params.windStrength = windStrength;
    params.phaseOffset = phaseOffset;
    params.rotationMask = rotationMask;
    params.rotationIntensity = rotationIntensity;
    params.rotationScale = rotationScale;
    params.rotationSpeed = rotationSpeed;
    params.flutterMask = flutterMask;
    params.flutterIntensity = flutterIntensity;
    params.flutterScale = flutterScale;
    params.flutterSpeed = flutterSpeed;

    windVector = ApplyVegetationMedium(worldPos, objectPos, params);


}
void ApplyVegetationMedium_half(float3 worldPos, float3 objectPos,
float windMask, float windStrength, float phaseOffset,
float flutterMask, float flutterIntensity, float flutterScale, float flutterSpeed,
float rotationMask, float rotationIntensity, float rotationScale, float rotationSpeed,
out float3 windVector)
{
    VegetationMediumParameters params = (VegetationMediumParameters)0;
    params.windMask = windMask;
    params.windStrength = windStrength;
    params.phaseOffset = phaseOffset;
    params.rotationMask = rotationMask;
    params.rotationIntensity = rotationIntensity;
    params.rotationScale = rotationScale;
    params.rotationSpeed = rotationSpeed;
    params.flutterMask = flutterMask;
    params.flutterIntensity = flutterIntensity;
    params.flutterScale = flutterScale;
    params.flutterSpeed = flutterSpeed;

    windVector = ApplyVegetationMedium(worldPos, objectPos, params);


}

#endif