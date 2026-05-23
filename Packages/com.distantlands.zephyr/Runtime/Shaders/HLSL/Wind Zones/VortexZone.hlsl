inline float3 ApplyVortexZone(float3 windVector, ZephyrWindZoneData zone, float3 position, float3 toPos, float influenceSq)
{
    float3 swirlDir = normalize(cross(zone.direction, toPos));
    float angle = atan2(toPos.z, toPos.x);

    float wave = sin(angle + _Time.y * zone.variationTime * 6.283185 +
                     zone.variationOffsetX + length(zone.position - position) * zone.auxOne);
    float magnitudeMultiplier = -1 + wave * zone.variationMagnitude;

    windVector += swirlDir * zone.strength * influenceSq * magnitudeMultiplier;

    float3 dirToCenter = normalize(zone.position - position);
    return windVector + dirToCenter * wave * influenceSq * zone.auxOne;
}