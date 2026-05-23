// Upgrade NOTE: replaced tex2D unity_Lightmap with UNITY_SAMPLE_TEX2D

// Made with Amplify Shader Editor v1.9.3.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Orly Shader/LightMap Fusion/Built-in/Standard Transparent"
{
	Properties
	{
		[NoScaleOffset][SingleLineTexture]_MainTex("Albedo", 2D) = "white" {}
		_AlbedoColor("Albedo Color", Color) = (1,1,1,0)
		[NoScaleOffset][SingleLineTexture]_MetallicGlossMap("Metal Smoothness", 2D) = "white" {}
		[NoScaleOffset][Normal][SingleLineTexture]_BumpMap("Normal", 2D) = "bump" {}
		_Metal1("Metal", Range( 0 , 1)) = 0
		[NoScaleOffset][SingleLineTexture]_EmissionMap("Emission map", 2D) = "white" {}
		_Smoothness1("Smoothness", Range( 0 , 1)) = 0
		[HDR]_EmissionColor1("Emission Color", Color) = (0,0,0,0)
		_LightIntensity1("Light Intensity", Range( 0 , 10)) = 1
		_Tiling2("Tiling", Float) = 1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] _texcoord2( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
		[Header(Forward Rendering Options)]
		[ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0
		[ToggleOff] _GlossyReflections("Reflections", Float) = 1.0
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "ForceNoShadowCasting" = "True" "IsEmissive" = "true"  }
		Cull Back
		CGINCLUDE
		#include "UnityCG.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 4.5
		#pragma shader_feature _SPECULARHIGHLIGHTS_OFF
		#pragma shader_feature _GLOSSYREFLECTIONS_OFF
		#ifdef UNITY_PASS_SHADOWCASTER
			#undef INTERNAL_DATA
			#undef WorldReflectionVector
			#undef WorldNormalVector
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
		#endif
		struct Input
		{
			float2 uv_texcoord;
			float2 uv2_texcoord2;
			float3 viewDir;
			INTERNAL_DATA
		};

		uniform sampler2D _BumpMap;
		uniform float _Tiling2;
		uniform sampler2D _MainTex;
		uniform float4 _AlbedoColor;
		uniform float _Set3Intensity;
		uniform float _Set4Intensity;
		uniform float _BlendLightmapX;
		uniform float _Set2Intensity;
		uniform float _Set1Intensity;
		uniform float _BlendLightmapY;
		uniform float _Metal1;
		uniform sampler2D _MetallicGlossMap;
		uniform float _LightIntensity1;
		uniform float4 _EmissionColor1;
		uniform sampler2D _EmissionMap;
		uniform float _Smoothness1;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float4 appendResult234 = (float4(_Tiling2 , _Tiling2 , 0.0 , 0.0));
			float2 uv_TexCoord235 = i.uv_texcoord * appendResult234.xy;
			float2 uvs237 = uv_TexCoord235;
			float3 Normal_out273 = UnpackNormal( tex2D( _BumpMap, uvs237 ) );
			float3 temp_output_279_0 = Normal_out273;
			o.Normal = temp_output_279_0;
			float4 Albedo_out257 = ( tex2D( _MainTex, uvs237 ) * _AlbedoColor );
			float4 temp_output_276_0 = Albedo_out257;
			o.Albedo = temp_output_276_0.rgb;
			float2 temp_output_266_0_g1 = ( (i.uv2_texcoord2*(unity_LightmapST).xy + (unity_LightmapST).zw) * float2( 0.5,0.5 ) );
			float4 lerpResult274_g1 = lerp( ( UNITY_SAMPLE_TEX2D( unity_Lightmap, ( float2( 0,0.5 ) + temp_output_266_0_g1 ) ) * _Set3Intensity ) , ( UNITY_SAMPLE_TEX2D( unity_Lightmap, ( float2( 0.5,0.5 ) + temp_output_266_0_g1 ) ) * _Set4Intensity ) , _BlendLightmapX);
			float4 lerpResult276_g1 = lerp( ( UNITY_SAMPLE_TEX2D( unity_Lightmap, ( temp_output_266_0_g1 + float2( 0.5,0 ) ) ) * _Set2Intensity ) , ( UNITY_SAMPLE_TEX2D( unity_Lightmap, ( temp_output_266_0_g1 + float2( 0,0 ) ) ) * _Set1Intensity ) , _BlendLightmapX);
			float4 lerpResult278_g1 = lerp( lerpResult274_g1 , lerpResult276_g1 , _BlendLightmapY);
			float3 decodeLightMap290_g1 = DecodeLightmap(lerpResult278_g1);
			#ifdef DIRLIGHTMAP_COMBINED
				float3 staticSwitch226_g1 = float3( 0,0,0 );
			#else
				float3 staticSwitch226_g1 = decodeLightMap290_g1;
			#endif
			float4 tex2DNode240 = tex2D( _MetallicGlossMap, uvs237 );
			float4 Metallic_out246 = ( _Metal1 * tex2DNode240 );
			float4 temp_output_253_0 = ( float4( staticSwitch226_g1 , 0.0 ) - Metallic_out246 );
			float dotResult326 = dot( Normal_out273 , i.viewDir );
			float clampResult323 = clamp( dotResult326 , 0.3 , 0.7 );
			float4 Emission_out266 = ( _EmissionColor1 * tex2D( _EmissionMap, uvs237 ) );
			o.Emission = ( ( ( temp_output_253_0 * _LightIntensity1 ) * ( clampResult323 * Albedo_out257 ) ) + Emission_out266 ).rgb;
			o.Metallic = Metallic_out246.r;
			float Smoothness_out271 = ( tex2DNode240.a * _Smoothness1 );
			o.Smoothness = Smoothness_out271;
			float _Opacity332 = _AlbedoColor.a;
			o.Alpha = ( Albedo_out257.a * _Opacity332 );
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard alpha:fade keepalpha fullforwardshadows noambient nolightmap  nodynlightmap 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 4.5
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			sampler3D _DitherMaskLOD;
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float4 customPack1 : TEXCOORD1;
				float4 tSpace0 : TEXCOORD2;
				float4 tSpace1 : TEXCOORD3;
				float4 tSpace2 : TEXCOORD4;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				o.customPack1.zw = customInputData.uv2_texcoord2;
				o.customPack1.zw = v.texcoord1;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				surfIN.uv2_texcoord2 = IN.customPack1.zw;
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.viewDir = IN.tSpace0.xyz * worldViewDir.x + IN.tSpace1.xyz * worldViewDir.y + IN.tSpace2.xyz * worldViewDir.z;
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				half alphaRef = tex3D( _DitherMaskLOD, float3( vpos.xy * 0.25, o.Alpha * 0.9375 ) ).a;
				clip( alphaRef - 0.01 );
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Standard"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19302
Node;AmplifyShaderEditor.CommentaryNode;232;-4821.406,299.5188;Inherit;False;1313.824;239.9395;;4;237;235;234;233;UVs;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;233;-4771.406,364.3351;Inherit;False;Property;_Tiling2;Tiling;15;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;234;-4463.046,355.2585;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;235;-4137.683,377.2672;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;237;-3749.581,349.5187;Inherit;False;uvs;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;236;-3348.972,314.1188;Inherit;False;1242.48;716.9421;;8;271;265;260;246;242;240;239;238;Metal Smootness;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;256;-3326.705,-99.70174;Inherit;False;1359.33;371.4727;;3;273;267;262;Normal;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;238;-3306.521,574.4327;Inherit;False;237;uvs;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;241;-3368.881,-885.8096;Inherit;False;1335.455;613.678;;6;257;252;248;247;243;332;Albedo;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;262;-3276.704,-27.36699;Inherit;False;237;uvs;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;239;-3295.07,364.1188;Inherit;False;Property;_Metal1;Metal;10;0;Create;True;0;0;0;False;0;False;0;0.216;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;240;-3009.509,567.5875;Inherit;True;Property;_MetallicGlossMap;Metal Smoothness;8;2;[NoScaleOffset];[SingleLineTexture];Create;False;0;0;0;False;0;False;-1;None;2a812634529b1944cbe54724cb732d5b;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;243;-3318.881,-474.0516;Inherit;False;237;uvs;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;267;-2915.43,-20.30812;Inherit;True;Property;_BumpMap;Normal;9;3;[NoScaleOffset];[Normal];[SingleLineTexture];Create;False;0;0;0;False;0;False;-1;None;e108b8f2e9a780d49bd23ee3035e8105;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;242;-2610.569,486.6945;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;245;-3348.743,1295.118;Inherit;False;1404.481;512.5583;;5;266;261;255;254;250;Emissive;1,1,1,1;0;0
Node;AmplifyShaderEditor.ColorNode;247;-2936.018,-835.8096;Inherit;False;Property;_AlbedoColor;Albedo Color;7;0;Create;True;0;0;0;False;0;False;1,1,1,0;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;273;-2251.552,90.04048;Inherit;False;Normal_out;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;248;-2988.892,-499.1312;Inherit;True;Property;_MainTex;Albedo;6;2;[NoScaleOffset];[SingleLineTexture];Create;False;0;0;0;False;0;False;-1;None;609fe16e845d333478e823e4ad786650;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;246;-2390.43,471.4098;Inherit;False;Metallic_out;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;250;-3294.969,1580.456;Inherit;False;237;uvs;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;252;-2573.622,-477.3295;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;279;-1081.185,-234.7309;Inherit;False;273;Normal_out;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;325;-1105.774,-72.54588;Inherit;False;Tangent;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.GetLocalVarNode;251;-972.4704,349.4021;Inherit;False;246;Metallic_out;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;303;-1535.261,162.5563;Inherit;False;Sample Lightmap Atlas;0;;1;2285874493434b44fafddcf5f59663b9;1,209,0;0;3;COLOR;352;FLOAT3;0;FLOAT3;178
Node;AmplifyShaderEditor.RegisterLocalVarNode;257;-2275.421,-610.5335;Inherit;False;Albedo_out;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.DotProductOpNode;326;-703.2692,-94.66833;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;255;-2837.465,1345.118;Inherit;False;Property;_EmissionColor1;Emission Color;13;1;[HDR];Create;True;0;0;0;False;0;False;0,0,0,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;254;-2846.939,1580.677;Inherit;True;Property;_EmissionMap;Emission map;11;2;[NoScaleOffset];[SingleLineTexture];Create;False;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleSubtractOpNode;253;-629.994,200.234;Inherit;False;2;0;FLOAT3;0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;259;-508.1682,419.3266;Inherit;False;Property;_LightIntensity1;Light Intensity;14;0;Create;True;0;0;0;False;0;False;1;1;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;261;-2485.35,1493.778;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;260;-3029.364,918.2603;Inherit;False;Property;_Smoothness1;Smoothness;12;0;Create;True;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;264;-277.8922,142.7786;Inherit;False;257;Albedo_out;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;323;-478.8585,-82.55478;Inherit;False;3;0;FLOAT;0.5;False;1;FLOAT;0.3;False;2;FLOAT;0.7;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;263;-62.35153,312.6992;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;266;-2170.372,1491.485;Inherit;False;Emission_out;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;265;-2626.608,778.61;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;322;-71.49724,-3.645575;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;332;-2305.441,-752.2079;Inherit;False;_Opacity;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;276;300.3514,-232.8465;Inherit;False;257;Albedo_out;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;272;20.84939,419.7656;Inherit;False;266;Emission_out;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;271;-2374.19,782.3706;Inherit;False;Smoothness_out;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;268;145.8066,200.4485;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.BreakToComponentsNode;328;507.4555,-83.8178;Inherit;False;COLOR;1;0;COLOR;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.GetLocalVarNode;333;426.9275,93.17841;Inherit;False;332;_Opacity;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;258;-382.7159,250.2529;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;274;369.4963,227.8063;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;277;708.9374,196.4932;Inherit;False;246;Metallic_out;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;278;695.4174,265.816;Inherit;False;271;Smoothness_out;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;330;703.0784,95.05901;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;205;1024.303,3.396568;Float;False;True;-1;5;ASEMaterialInspector;0;0;Standard;Orly Shader/LightMap Fusion/Built-in/Standard Transparent;False;False;False;False;True;False;True;True;False;False;False;False;False;False;True;True;False;False;True;True;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Transparent;0.5;True;True;0;False;Transparent;;Transparent;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;2;5;False;;10;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;Standard;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;17;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;234;0;233;0
WireConnection;234;1;233;0
WireConnection;235;0;234;0
WireConnection;237;0;235;0
WireConnection;240;1;238;0
WireConnection;267;1;262;0
WireConnection;242;0;239;0
WireConnection;242;1;240;0
WireConnection;273;0;267;0
WireConnection;248;1;243;0
WireConnection;246;0;242;0
WireConnection;252;0;248;0
WireConnection;252;1;247;0
WireConnection;257;0;252;0
WireConnection;326;0;279;0
WireConnection;326;1;325;0
WireConnection;254;1;250;0
WireConnection;253;0;303;0
WireConnection;253;1;251;0
WireConnection;261;0;255;0
WireConnection;261;1;254;0
WireConnection;323;0;326;0
WireConnection;263;0;253;0
WireConnection;263;1;259;0
WireConnection;266;0;261;0
WireConnection;265;0;240;4
WireConnection;265;1;260;0
WireConnection;322;0;323;0
WireConnection;322;1;264;0
WireConnection;332;0;247;4
WireConnection;271;0;265;0
WireConnection;268;0;263;0
WireConnection;268;1;322;0
WireConnection;328;0;276;0
WireConnection;258;0;253;0
WireConnection;274;0;268;0
WireConnection;274;1;272;0
WireConnection;330;0;328;3
WireConnection;330;1;333;0
WireConnection;205;0;276;0
WireConnection;205;1;279;0
WireConnection;205;2;274;0
WireConnection;205;3;277;0
WireConnection;205;4;278;0
WireConnection;205;9;330;0
ASEEND*/
//CHKSM=433B660915CAE862E308FC6FB101EBFC3FE2A122