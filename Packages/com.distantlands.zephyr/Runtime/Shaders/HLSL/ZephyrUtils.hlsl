//Distant Lands | 2025

#ifndef ZEPHYR_UTILS
#define ZEPHYR_UTILS

#ifndef ZEPHYR_NOISETEX
#define ZEPHYR_NOISETEX
TEXTURE2D(ZEPHYR_WindNoiseTexture);
SAMPLER(samplerZEPHYR_WindNoiseTexture);
#endif

// Creates a scalable sine wave for changing wind speed at runtime without stuttering
float LayeredSineWave(float input, float scale, float windSpeed, float min, float max) {

    float waveOne = sin(input * scale * 1);
    float waveTwo = sin(input * scale * 2);
    float waveThree = sin(input * scale * 4);
    float waveFour = sin(input * scale * 8);
    float waveFive = sin(input * scale * 16);

    float remappedWind = 16.0 * (saturate((windSpeed - min) / (max - min)) + 0.000001);
    float waveOneWeight = saturate(1 + remappedWind * - 0.5);
    float waveTwoWeight = saturate(saturate(2 + remappedWind * - 0.5) - (waveOneWeight));
    float waveThreeWeight = saturate(saturate(2 + remappedWind * - 0.25) - (waveOneWeight + waveTwoWeight));
    float waveFourWeight = saturate(saturate(2 + remappedWind * - 0.125) - (waveOneWeight + waveTwoWeight + waveThreeWeight));
    float waveFiveWeight = saturate(1 - (waveOneWeight + waveTwoWeight + waveThreeWeight + waveFourWeight));

    float finalWaveStrength = waveOne * waveOneWeight + waveTwo * waveTwoWeight + waveThree * waveThreeWeight + waveFour * waveFourWeight + waveFive * waveFiveWeight;

    return finalWaveStrength;

}

inline float SampleNoiseTex(float2 uv) {
    return ZEPHYR_WindNoiseTexture.SampleLevel(
    samplerZEPHYR_WindNoiseTexture,
    frac(uv),
    0
    ).r;
}

float LayeredNoise(float2 input, float scale, float windSpeed, float min, float max) {

    float waveOne = SampleNoiseTex(input * scale * 1);
    float waveTwo = SampleNoiseTex(input * scale * 2);
    float waveThree = SampleNoiseTex(input * scale * 4);
    float waveFour = SampleNoiseTex(input * scale * 8);
    float waveFive = SampleNoiseTex(input * scale * 16);

    float remappedWind = 16.0 * (saturate((windSpeed - min) / (max - min)) + 0.000001);
    float waveOneWeight = saturate(1 + remappedWind * - 0.5);
    float waveTwoWeight = saturate(saturate(2 + remappedWind * - 0.5) - (waveOneWeight));
    float waveThreeWeight = saturate(saturate(2 + remappedWind * - 0.25) - (waveOneWeight + waveTwoWeight));
    float waveFourWeight = saturate(saturate(2 + remappedWind * - 0.125) - (waveOneWeight + waveTwoWeight + waveThreeWeight));
    float waveFiveWeight = saturate(1 - (waveOneWeight + waveTwoWeight + waveThreeWeight + waveFourWeight));

    float finalWaveStrength = waveOne * waveOneWeight + waveTwo * waveTwoWeight + waveThree * waveThreeWeight + waveFour * waveFourWeight + waveFive * waveFiveWeight;

    return finalWaveStrength;

}

void LayeredSineWave_float(float input, float scale, float windSpeed, float min, float max, out float output){
    output = LayeredSineWave(input, scale, windSpeed, min, max);
}

void LayeredNoise_float(float2 input, float scale, float windSpeed, float min, float max, out float output){
    output = LayeredNoise(input, scale, windSpeed, min, max);
}


float3 RotateAboutAxis(float3 vectorToRotate, float3 axis, float angleInRadians)
{
    float s = sin(angleInRadians);
    float c = cos(angleInRadians);
    float one_minus_c = 1.0 - c;

    axis = normalize(axis);

    float3x3 rot_mat =
    {
        one_minus_c * axis.x * axis.x + c, one_minus_c * axis.x * axis.y - axis.z * s, one_minus_c * axis.z * axis.x + axis.y * s,
        one_minus_c * axis.x * axis.y + axis.z * s, one_minus_c * axis.y * axis.y + c, one_minus_c * axis.y * axis.z - axis.x * s,
        one_minus_c * axis.z * axis.x - axis.y * s, one_minus_c * axis.y * axis.z + axis.x * s, one_minus_c * axis.z * axis.z + c
    };

    return mul(rot_mat, vectorToRotate);
}

// Normalizes the position of a vertex after applying an offset. This will keep meshes as intact as possible by maintaining scale and preventing stretching.
float3 NormalizePosition(float3 center, float3 originalPosition, float3 offset) {
    float originalLength = length(originalPosition - center);
    float3 offsetDirection = normalize((originalPosition + offset) - center);
    return center + offsetDirection * originalLength;
}

void NormalizePosition_float(float3 center, float3 originalPosition, float3 offset, out float3 outputPosition) {
    outputPosition = NormalizePosition(center, originalPosition, offset);
}

void NormalizePosition_half(half3 center, half3 originalPosition, half3 offset, out half3 outputPosition) {
    outputPosition = NormalizePosition(center, originalPosition, offset);
}

#endif