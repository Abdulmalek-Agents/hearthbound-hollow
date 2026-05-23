//Distant Lands | 2025
#pragma target 4.5
#include "Packages/com.distantlands.zephyr/Runtime/Shaders/HLSL/ZephyrIncludes.hlsl"

//ShaderGraph stub
void GetWindAtPoint_float(float3 position, out float3 wind) {
    wind = GetWindAtPoint(position);
}

void GetWindComponentsAtPoint_float(float3 position, out float3 windVector, out float3 baseWind, out float3 gust, out float3 flutter, out float3 windzones) {
    GetWindComponentsAtPoint(position, windVector, baseWind, gust, flutter, windzones);
}