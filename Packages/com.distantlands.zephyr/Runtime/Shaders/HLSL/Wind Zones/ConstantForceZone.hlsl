inline float3 ApplyConstantForceZone(float3 windVector, ZephyrWindZoneData zone, float3 position, float influenceSq)
{
    float2 variationUV = float2(
        position.x / zone.variationTime + zone.variationOffsetX,
        position.z / zone.variationTime + zone.variationOffsetY
    );

    float4 variationSample = ZEPHYR_WindNoiseTexture.SampleLevel(
        samplerZEPHYR_WindNoiseTexture, frac(variationUV), 0
    );

    float pulse = lerp(
        zone.strength,
        zone.strength * (1.0 - zone.variationMagnitude),
        variationSample.r
    );
    return windVector + pulse * influenceSq * zone.direction;
}