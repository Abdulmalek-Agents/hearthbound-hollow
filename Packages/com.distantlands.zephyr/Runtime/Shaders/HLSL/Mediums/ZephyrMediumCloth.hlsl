#ifndef ZEPHYR_MEDIUM_CLOTH
#define ZEPHYR_MEDIUM_CLOTH

#include "Packages/com.distantlands.zephyr/Runtime/Shaders/HLSL/ZephyrIncludes.hlsl"
#include "Packages/com.distantlands.zephyr/Runtime/Shaders/HLSL/ZephyrUtils.hlsl"

struct ClothMediumParameters
{
    float windMask;
    float windStrength;
    float rippleMask;
    float rippleIntensity;
    float rippleScale;
    float rippleSpeed;
    float flutterMask;
    float flutterIntensity;
    float flutterScale;
    float flutterSpeed;
};

float3 ApplyClothMedium(float3 worldPos, float3 objectPos, float3 worldNormal, ClothMediumParameters params)
{

    // Get wind components from Zephyr
    float3 wind, baseWind, gustWind, flutterWind, windzonesWind;
    GetWindComponentsAtPoint(worldPos, wind, baseWind, gustWind, flutterWind, windzonesWind);
    float windLen = length(wind);
    float3 windDir = TransformWorldToObjectDir(normalize(wind + float3(0.0001, 0, 0)), true);

    // Wind magnitude
    float windStrength = params.windStrength * params.windMask * windLen;
    float3 windVec = windDir * windStrength;

    // -- - Ripple -- -
    float ripplePhase = worldPos.y + _Time.y * params.rippleSpeed;
    float rippleWave = LayeredSineWave(ripplePhase, params.rippleScale, windLen, 1.0, 16.0);
    float ripple = rippleWave * params.rippleIntensity * params.rippleMask * windLen;
    float3 rippleVec = windDir * ripple;


    // -- - Flutter -- -
    float flutterPhase = (worldPos.x + worldPos.y + worldPos.z) + _Time.y * params.flutterSpeed;
    float flutterWave = LayeredSineWave(flutterPhase, params.flutterScale, windLen, 1.0, 16.0);
    float flutter = flutterWave * params.flutterIntensity * params.flutterMask * windLen;
    float3 flutterVec = worldNormal * flutter;

    // Combine all effects
    float3 result = windVec + flutterVec + rippleVec;
    return NormalizePosition(float3(0, 0, 0), objectPos, result);
}

void ApplyClothMedium_float(float3 worldPos, float3 objectPos, float3 worldNormal,
                            float windMask, float windStrength,
                            float rippleMask, float rippleIntensity, float rippleScale, float rippleSpeed,
                            float flutterMask, float flutterIntensity, float flutterScale, float flutterSpeed,
                            out float3 windVector)
{
    ClothMediumParameters params = (ClothMediumParameters)0;
    params.windMask = windMask;
    params.windStrength = windStrength;
    params.rippleMask = rippleMask;
    params.rippleIntensity = rippleIntensity;
    params.rippleScale = rippleScale;
    params.rippleSpeed = rippleSpeed;
    params.flutterMask = flutterMask;
    params.flutterIntensity = flutterIntensity;
    params.flutterScale = flutterScale;
    params.flutterSpeed = flutterSpeed;

    windVector = ApplyClothMedium(worldPos, objectPos, worldNormal, params) - objectPos;
}
void ApplyClothMedium_half(float3 worldPos, float3 objectPos, float3 worldNormal,
float windMask, float windStrength, 
float rippleMask, float rippleIntensity, float rippleScale, float rippleSpeed,
float flutterMask, float flutterIntensity, float flutterScale, float flutterSpeed,
out float3 windVector)
{
    ClothMediumParameters params = (ClothMediumParameters)0;
    params.windMask = windMask;
    params.windStrength = windStrength;
    params.rippleMask = rippleMask;
    params.rippleIntensity = rippleIntensity;
    params.rippleScale = rippleScale;
    params.rippleSpeed = rippleSpeed;
    params.flutterMask = flutterMask;
    params.flutterIntensity = flutterIntensity;
    params.flutterScale = flutterScale;
    params.flutterSpeed = flutterSpeed;

    windVector = ApplyClothMedium(worldPos, objectPos, worldNormal, params);
}

#endif