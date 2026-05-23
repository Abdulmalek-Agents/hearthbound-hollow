inline float3 ApplyAccelerationZone(float3 windVector, ZephyrWindZoneData zone, float influenceSq)
{
    return windVector * lerp(1.0, zone.strength, influenceSq);
}