//  Distant Lands 2025
//  Applies all the wind zone functions

#include "Packages/com.distantlands.zephyr/Runtime/Shaders/HLSL/Wind Zones/AccelerationZone.hlsl"
#include "Packages/com.distantlands.zephyr/Runtime/Shaders/HLSL/Wind Zones/AttractionZone.hlsl"
#include "Packages/com.distantlands.zephyr/Runtime/Shaders/HLSL/Wind Zones/ConstantForceZone.hlsl"
#include "Packages/com.distantlands.zephyr/Runtime/Shaders/HLSL/Wind Zones/FunnelZone.hlsl"
#include "Packages/com.distantlands.zephyr/Runtime/Shaders/HLSL/Wind Zones/VortexZone.hlsl"
#include "Packages/com.distantlands.zephyr/Runtime/Shaders/HLSL/Wind Zones/AerodynamicZone.hlsl"



inline float3 ApplyWindZone(float3 windVector, ZephyrWindZoneData zone, float3 position, float3 toPos, float sqrDist)
{
    float radiusSq = zone.radius * zone.radius;
    float influence = 1.0 - saturate(sqrDist / radiusSq);
    float influenceSq = influence * influence;

    switch (zone.id)
    {
        case 1: return ApplyConstantForceZone(windVector, zone, position, influenceSq);
        case 2: return ApplyAttractionZone(windVector, zone, position, influenceSq);
        case 3: return ApplyAccelerationZone(windVector, zone, influenceSq);
        case 4: return ApplyFunnelZone(windVector, zone, influenceSq);
        case 5: return ApplyVortexZone(windVector, zone, position, toPos, influenceSq);
        case 6: return ApplyAerodynamicZone(windVector, zone, position, influenceSq);
        default: return windVector;
    }
}