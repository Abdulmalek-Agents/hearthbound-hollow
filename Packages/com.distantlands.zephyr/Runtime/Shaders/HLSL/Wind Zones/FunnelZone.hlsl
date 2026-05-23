inline float3 ApplyFunnelZone(float3 windVector, ZephyrWindZoneData zone, float influenceSq)
{
    float3 newDir = normalize(lerp(normalize(windVector), zone.direction, influenceSq));
    windVector = newDir * length(windVector);
    return windVector * lerp(1.0, zone.strength, influenceSq);
}