//Distant Lands | 2025

#ifndef ZEPHYR_MEDIUM_TREE
#define ZEPHYR_MEDIUM_TREE


#include "Packages/com.distantlands.zephyr/Runtime/Shaders/HLSL/ZephyrIncludes.hlsl"
#include "Packages/com.distantlands.zephyr/Runtime/Shaders/HLSL/ZephyrUtils.hlsl"

struct TreeMediumParameters
{
    float windMask;
    float windStrength;
    float windScale;
    float windSpeed;
    float branchPhase;
    float branchRotationMask;
    float branchRotationIntensity;
    float branchRotationScale;
    float branchRotationSpeed;
    float branchBounceMask;
    float branchBounceIntensity;
    float branchBounceScale;
    float branchBounceSpeed;
    float leafNoiseMask;
    float leafNoiseIntensity;
    float leafNoiseScale;
    float leafNoiseSpeed;
};

float3 ApplyTreeMedium(float3 worldPos, float3 objectPos, TreeMediumParameters params)
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
    float baseWindStrength = baseWindRemap * params.windStrength * params.windMask * baseWindLength * 0.01;
    float3 baseWindCombined = windDir * baseWindStrength + abs(baseWindStrength) * float3(0.0, - 0.1, 0.0);

    //Branch Wind
    float branchRotationWindSine = LayeredSineWave(transformPosition.x + transformPosition.y + transformPosition.z + _Time.y * params.branchRotationSpeed + params.branchPhase, params.branchRotationScale, baseWindLength, 1.0, 16.0);
    float branchRotation = branchRotationWindSine * baseWindLength * params.branchRotationMask * params.branchRotationIntensity * 0.005;
    float3 rotatedBranch = RotateAboutAxis(objectPos, float3(0, 1, 0), branchRotation);

    float branchBounceWindSine = LayeredSineWave(transformPosition.x + transformPosition.z + _Time.y * params.branchBounceSpeed + params.branchPhase, params.branchBounceScale, baseWindLength, 1.0, 16.0);
    float3 branchBounce = float3(0,1,0) * branchBounceWindSine * baseWindLength * params.branchBounceMask * params.branchBounceIntensity * 0.01;

    float3 branchWindCombined = branchBounce + rotatedBranch;


    //Leaf Wind
    float2 UV = float2(
    (worldPos.x + _Time.y * params.leafNoiseSpeed) / params.leafNoiseScale,
    (worldPos.z + _Time.y * params.leafNoiseSpeed) / params.leafNoiseScale
    );

    float noise = (ZEPHYR_WindNoiseTexture.SampleLevel(
    samplerZEPHYR_WindNoiseTexture,
    frac(UV),
    0
    ).r - 0.5) * 2;
    float leafWindAmount = noise * params.leafNoiseMask * params.leafNoiseIntensity * windLength * 0.01;
    float3 leafWindCombined = float3(0, 1, 0) * leafWindAmount + windDir * leafWindAmount;


    return baseWindCombined + branchWindCombined + leafWindCombined - objectPos;
}


void ApplyTreeMedium_float(float3 worldPos, float3 objectPos,
float windMask, float windStrength, float windScale, float windSpeed,
float branchPhase,
float branchRotationMask, float branchRotationIntensity, float branchRotationScale, float branchRotationSpeed,
float branchBounceMask, float branchBounceIntensity, float branchBounceScale, float branchBounceSpeed,
float leafNoiseMask, float leafNoiseIntensity, float leafNoiseScale, float leafNoiseSpeed,
out float3 output
) {

    TreeMediumParameters params = (TreeMediumParameters)0;
    params.windMask = windMask;
    params.windStrength = windStrength;
    params.windScale = windScale;
    params.windSpeed = windSpeed;
    params.branchPhase = branchPhase;
    params.branchRotationMask = branchRotationMask;
    params.branchRotationIntensity = branchRotationIntensity;
    params.branchRotationScale = branchRotationScale;
    params.branchRotationSpeed = branchRotationSpeed;
    params.branchBounceMask = branchBounceMask;
    params.branchBounceIntensity = branchBounceIntensity;
    params.branchBounceScale = branchBounceScale;
    params.branchBounceSpeed = branchBounceSpeed;
    params.leafNoiseMask = leafNoiseMask;
    params.leafNoiseIntensity = leafNoiseIntensity;
    params.leafNoiseScale = leafNoiseScale;
    params.leafNoiseSpeed = leafNoiseSpeed;

    output = ApplyTreeMedium(worldPos, objectPos, params);

}

void ApplyTreeMedium_half(half3 worldPos, half3 objectPos,
half windMask, half windStrength, half windScale, half windSpeed,
half branchPhase,
half branchRotationMask, half branchRotationIntensity, half branchRotationScale, half branchRotationSpeed,
half branchBounceMask, half branchBounceIntensity, half branchBounceScale, half branchBounceSpeed,
half leafNoiseMask, half leafNoiseIntensity, half leafNoiseScale, half leafNoiseSpeed,
out half3 output
) {

    TreeMediumParameters params = (TreeMediumParameters)0;
    params.windMask = windMask;
    params.windStrength = windStrength;
    params.windScale = windScale;
    params.windSpeed = windSpeed;
    params.branchPhase = branchPhase;
    params.branchRotationMask = branchRotationMask;
    params.branchRotationIntensity = branchRotationIntensity;
    params.branchRotationScale = branchRotationScale;
    params.branchRotationSpeed = branchRotationSpeed;
    params.branchBounceMask = branchBounceMask;
    params.branchBounceIntensity = branchBounceIntensity;
    params.branchBounceScale = branchBounceScale;
    params.branchBounceSpeed = branchBounceSpeed;
    params.leafNoiseMask = leafNoiseMask;
    params.leafNoiseIntensity = leafNoiseIntensity;
    params.leafNoiseScale = leafNoiseScale;
    params.leafNoiseSpeed = leafNoiseSpeed;

    output = ApplyTreeMedium(worldPos, objectPos, params);

}


#endif