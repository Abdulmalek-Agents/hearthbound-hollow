inline float3 ApplyAerodynamicZone(float3 windVector, ZephyrWindZoneData zone, float3 position, float influenceSq)
{
    // Wind in the movement direction (passed from CPU as zone.direction)
    float3 dirWind = zone.direction * zone.strength;

    // Radial push/attraction using AuxOne as push strength
    float3 radialWind = -normalize(zone.position - position) * zone.auxOne * length(zone.direction);

    // Apply influence falloff
    return windVector + (dirWind + radialWind) * influenceSq;
}