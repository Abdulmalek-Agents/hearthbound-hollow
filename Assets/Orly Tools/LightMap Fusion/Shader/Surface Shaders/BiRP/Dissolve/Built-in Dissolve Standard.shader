// Upgrade NOTE: replaced tex2D unity_Lightmap with UNITY_SAMPLE_TEX2D

// Made with Amplify Shader Editor v1.9.3.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Orly Shader/LightMap Fusion/Built-in/Dissolve/Standard"
{
	Properties
	{
		[HDR]_EmisionColor_1("EmisionColor_1", Color) = (0,0,0,0)
		[HDR]_EmisionColor_2("EmisionColor_2", Color) = (0,0,0,0)
		[HDR]_EmisionColor_3("EmisionColor_3", Color) = (0,0,0,0)
		[HDR]_EmisionColor_4("EmisionColor_4", Color) = (0,0,0,0)
		[NoScaleOffset][SingleLineTexture]_MainTex("Albedo", 2D) = "white" {}
		_AlbedoColor("Albedo Color", Color) = (1,1,1,0)
		[NoScaleOffset][Normal][SingleLineTexture]_BumpMap("Normal", 2D) = "bump" {}
		[NoScaleOffset][SingleLineTexture]_MetallicGlossMap("Metal Smoothness", 2D) = "white" {}
		_Metal1("Metal", Range( 0 , 1)) = 0
		[NoScaleOffset][SingleLineTexture]_EmissionMap("Emission Map", 2D) = "black" {}
		_Smoothness1("Smoothness", Range( 0 , 1)) = 0
		_LightIntensity1("Light Intensity", Range( 0 , 10)) = 1
		_Tiling2("Tiling", Float) = 1
		[KeywordEnum(Disabled,Enabled)] _EnableDissolve("Enable Dissolve", Float) = 1
		[KeywordEnum(Plane,Sphere,Box)] _MaskSelector("Mask Selector ", Float) = 0
		[Toggle]_Invert("Invert", Float) = 0
		_EdgeThickness("Edge Thickness", Range( 0 , 1)) = 0.04917255
		_GizmoCenter("Gizmo Center", Vector) = (0,0,0,0)
		_vID("vID", Int) = 0
		_NoiseScrollSpeed("Noise Scroll Speed", Vector) = (1,1,1,0)
		_NoiseScale("NoiseScale", Float) = 0
		_BoxSize("Box Size", Vector) = (1,1,1,0)
		_SphereRadius("Sphere Radius", Float) = 1
		_PlaneDirection("Plane Direction", Vector) = (0,1,0,0)
		_Dissolve("Dissolve", Range( 0 , 1)) = 1
		[HideInInspector] _texcoord2( "", 2D ) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
		[Header(Forward Rendering Options)]
		[ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0
		[ToggleOff] _GlossyReflections("Reflections", Float) = 1.0
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "AlphaTest+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		CGINCLUDE
		#include "UnityStandardUtils.cginc"
		#include "UnityCG.cginc"
		#include "UnityShaderVariables.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#pragma shader_feature _SPECULARHIGHLIGHTS_OFF
		#pragma shader_feature _GLOSSYREFLECTIONS_OFF
		#pragma shader_feature_local _ENABLEDISSOLVE_DISABLED _ENABLEDISSOLVE_ENABLED
		#pragma shader_feature_local _MASKSELECTOR_PLANE _MASKSELECTOR_SPHERE _MASKSELECTOR_BOX
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
			float3 viewDir;
			INTERNAL_DATA
			float2 uv2_texcoord2;
			float3 worldPos;
		};

		uniform int _vID;
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
		uniform float4 _EmisionColor_3;
		uniform float4 _EmisionColor_4;
		uniform float4 _EmisionColor_2;
		uniform float4 _EmisionColor_1;
		uniform sampler2D _EmissionMap;
		uniform float4 _EdgeColor;
		uniform float3 _NoiseScrollSpeed;
		uniform float _NoiseScale;
		uniform float _Dissolve;
		uniform float _Invert;
		uniform float3 _GizmoCenter;
		uniform float3 _PlaneDirection;
		uniform float _SphereRadius;
		uniform float3 _BoxSize;
		uniform float _EdgeThickness;
		uniform float _Smoothness1;


		float3 mod3D289( float3 x ) { return x - floor( x / 289.0 ) * 289.0; }

		float4 mod3D289( float4 x ) { return x - floor( x / 289.0 ) * 289.0; }

		float4 permute( float4 x ) { return mod3D289( ( x * 34.0 + 1.0 ) * x ); }

		float4 taylorInvSqrt( float4 r ) { return 1.79284291400159 - r * 0.85373472095314; }

		float snoise( float3 v )
		{
			const float2 C = float2( 1.0 / 6.0, 1.0 / 3.0 );
			float3 i = floor( v + dot( v, C.yyy ) );
			float3 x0 = v - i + dot( i, C.xxx );
			float3 g = step( x0.yzx, x0.xyz );
			float3 l = 1.0 - g;
			float3 i1 = min( g.xyz, l.zxy );
			float3 i2 = max( g.xyz, l.zxy );
			float3 x1 = x0 - i1 + C.xxx;
			float3 x2 = x0 - i2 + C.yyy;
			float3 x3 = x0 - 0.5;
			i = mod3D289( i);
			float4 p = permute( permute( permute( i.z + float4( 0.0, i1.z, i2.z, 1.0 ) ) + i.y + float4( 0.0, i1.y, i2.y, 1.0 ) ) + i.x + float4( 0.0, i1.x, i2.x, 1.0 ) );
			float4 j = p - 49.0 * floor( p / 49.0 );  // mod(p,7*7)
			float4 x_ = floor( j / 7.0 );
			float4 y_ = floor( j - 7.0 * x_ );  // mod(j,N)
			float4 x = ( x_ * 2.0 + 0.5 ) / 7.0 - 1.0;
			float4 y = ( y_ * 2.0 + 0.5 ) / 7.0 - 1.0;
			float4 h = 1.0 - abs( x ) - abs( y );
			float4 b0 = float4( x.xy, y.xy );
			float4 b1 = float4( x.zw, y.zw );
			float4 s0 = floor( b0 ) * 2.0 + 1.0;
			float4 s1 = floor( b1 ) * 2.0 + 1.0;
			float4 sh = -step( h, 0.0 );
			float4 a0 = b0.xzyw + s0.xzyw * sh.xxyy;
			float4 a1 = b1.xzyw + s1.xzyw * sh.zzww;
			float3 g0 = float3( a0.xy, h.x );
			float3 g1 = float3( a0.zw, h.y );
			float3 g2 = float3( a1.xy, h.z );
			float3 g3 = float3( a1.zw, h.w );
			float4 norm = taylorInvSqrt( float4( dot( g0, g0 ), dot( g1, g1 ), dot( g2, g2 ), dot( g3, g3 ) ) );
			g0 *= norm.x;
			g1 *= norm.y;
			g2 *= norm.z;
			g3 *= norm.w;
			float4 m = max( 0.6 - float4( dot( x0, x0 ), dot( x1, x1 ), dot( x2, x2 ), dot( x3, x3 ) ), 0.0 );
			m = m* m;
			m = m* m;
			float4 px = float4( dot( x0, g0 ), dot( x1, g1 ), dot( x2, g2 ), dot( x3, g3 ) );
			return 42.0 * dot( m, px);
		}


		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float4 appendResult234 = (float4(_Tiling2 , _Tiling2 , 0.0 , 0.0));
			float2 uv_TexCoord235 = i.uv_texcoord * appendResult234.xy;
			float2 uvs237 = uv_TexCoord235;
			float3 Normal_out273 = UnpackScaleNormal( tex2D( _BumpMap, uvs237 ), 2.0 );
			float3 temp_output_279_0 = Normal_out273;
			o.Normal = temp_output_279_0;
			float4 Albedo_out257 = ( tex2D( _MainTex, uvs237 ) * _AlbedoColor );
			o.Albedo = Albedo_out257.rgb;
			float dotResult451 = dot( Normal_out273 , i.viewDir );
			float clampResult452 = clamp( dotResult451 , 0.3 , 1.0 );
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
			float4 lerpResult349_g1 = lerp( ( _Set3Intensity * _EmisionColor_3 ) , ( _Set4Intensity * _EmisionColor_4 ) , _BlendLightmapX);
			float4 lerpResult350_g1 = lerp( ( _Set2Intensity * _EmisionColor_2 ) , ( _Set1Intensity * _EmisionColor_1 ) , _BlendLightmapX);
			float4 lerpResult351_g1 = lerp( lerpResult349_g1 , lerpResult350_g1 , _BlendLightmapY);
			float4 Emission_out266 = ( lerpResult351_g1 * tex2D( _EmissionMap, uvs237 ) );
			#if defined(_ENABLEDISSOLVE_DISABLED)
				float staticSwitch94_g112 = (float)0;
			#elif defined(_ENABLEDISSOLVE_ENABLED)
				float staticSwitch94_g112 = (float)1;
			#else
				float staticSwitch94_g112 = (float)1;
			#endif
			float3 ase_worldPos = i.worldPos;
			float2 appendResult29_g112 = (float2(_NoiseScale , _NoiseScale));
			float simplePerlin3D31_g112 = snoise( ( ase_worldPos + ( _NoiseScrollSpeed * _Time.y ) )*appendResult29_g112.x );
			simplePerlin3D31_g112 = simplePerlin3D31_g112*0.5 + 0.5;
			float temp_output_43_0_g112 = _Dissolve;
			float temp_output_37_0_g112 = (0.0 + (temp_output_43_0_g112 - 0.1) * (1.0 - 0.0) / (1.0 - 0.1));
			float Noise33_g112 = ( simplePerlin3D31_g112 - temp_output_37_0_g112 );
			float clampResult64_g112 = clamp( Noise33_g112 , 0.1 , 1.0 );
			float3 temp_output_132_0_g112 = ( ase_worldPos - _GizmoCenter );
			float dotResult25_g112 = dot( temp_output_132_0_g112 , _PlaneDirection );
			float temp_output_142_0_g112 = ( dotResult25_g112 + 0.0 );
			float3 temp_output_5_0_g113 = ( ( ase_worldPos - _GizmoCenter ) / _SphereRadius );
			float dotResult8_g113 = dot( temp_output_5_0_g113 , temp_output_5_0_g113 );
			#if defined(_MASKSELECTOR_PLANE)
				float staticSwitch50_g112 = temp_output_142_0_g112;
			#elif defined(_MASKSELECTOR_SPHERE)
				float staticSwitch50_g112 = pow( saturate( dotResult8_g113 ) , 1.0 );
			#elif defined(_MASKSELECTOR_BOX)
				float staticSwitch50_g112 = ( distance( max( ( abs( ( ase_worldPos - _GizmoCenter ) ) - ( _BoxSize * float3( 0.5,0.5,0.5 ) ) ) , float3( 0,0,0 ) ) , float3( 0,0,0 ) ) / 1.0 );
			#else
				float staticSwitch50_g112 = temp_output_142_0_g112;
			#endif
			float Gradient32_g112 = (( _Invert )?( ( 1.0 - staticSwitch50_g112 ) ):( staticSwitch50_g112 ));
			float temp_output_74_0_g112 = ( ( ( clampResult64_g112 * ( 1.0 - Gradient32_g112 ) ) - Gradient32_g112 ) - temp_output_37_0_g112 );
			float4 lerpResult430 = lerp( ( ( ( clampResult452 * Albedo_out257 ) * ( temp_output_253_0 * _LightIntensity1 ) ) + Emission_out266 ) , _EdgeColor , ( staticSwitch94_g112 != 0.0 ? step( temp_output_74_0_g112 , ( temp_output_37_0_g112 + _EdgeThickness ) ) : 0.0 ));
			o.Emission = lerpResult430.rgb;
			o.Metallic = Metallic_out246.r;
			float Smoothness_out271 = ( tex2DNode240.a * _Smoothness1 );
			o.Smoothness = Smoothness_out271;
			o.Alpha = 1;
			int clampResult294 = clamp( 0 , 0 , 0 );
			clip( ( ( staticSwitch94_g112 != 0.0 ? temp_output_74_0_g112 : 1.0 ) + clampResult294 ) - _Dissolve );
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard keepalpha fullforwardshadows noambient nolightmap  nodynlightmap 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
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
				surfIN.worldPos = worldPos;
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Standard"
	CustomEditor "LightMapFusion.CustomShaderGUI"
}
/*ASEBEGIN
Version=19302
Node;AmplifyShaderEditor.CommentaryNode;232;-4821.406,299.5188;Inherit;False;1313.824;239.9395;;4;237;235;234;233;UVs;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;233;-4771.406,364.3351;Inherit;False;Property;_Tiling2;Tiling;15;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;234;-4463.046,355.2585;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;235;-4137.683,377.2672;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;236;-3348.972,314.1188;Inherit;False;1242.48;716.9421;;8;271;265;260;246;242;240;239;238;Metal Smootness;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;237;-3749.581,349.5187;Inherit;False;uvs;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;256;-3326.705,-99.70174;Inherit;False;822.3063;277;;3;273;267;262;Normal;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;238;-3306.521,574.4327;Inherit;False;237;uvs;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;241;-3368.881,-885.8096;Inherit;False;1335.455;613.678;;5;257;252;248;247;243;Albedo;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;262;-3276.704,-27.36699;Inherit;False;237;uvs;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;239;-3295.07,364.1188;Inherit;False;Property;_Metal1;Metal;10;0;Create;True;0;0;0;False;0;False;0;0.794;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;240;-3009.509,567.5875;Inherit;True;Property;_MetallicGlossMap;Metal Smoothness;9;2;[NoScaleOffset];[SingleLineTexture];Create;False;0;0;0;False;0;False;-1;None;2a812634529b1944cbe54724cb732d5b;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;243;-3318.881,-474.0516;Inherit;False;237;uvs;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;267;-3049.338,-49.70091;Inherit;True;Property;_BumpMap;Normal;8;3;[NoScaleOffset];[Normal];[SingleLineTexture];Create;False;0;0;0;False;0;False;-1;None;e108b8f2e9a780d49bd23ee3035e8105;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;2;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;242;-2610.569,486.6945;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;245;-3348.743,1295.118;Inherit;False;1404.481;512.5583;;5;266;261;255;254;250;Emissive;1,1,1,1;0;0
Node;AmplifyShaderEditor.ColorNode;247;-2936.018,-835.8096;Inherit;False;Property;_AlbedoColor;Albedo Color;7;0;Create;True;0;0;0;False;0;False;1,1,1,0;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;273;-2713.397,-36.88502;Inherit;False;Normal_out;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;248;-2988.892,-499.1312;Inherit;True;Property;_MainTex;Albedo;6;2;[NoScaleOffset];[SingleLineTexture];Create;False;0;0;0;False;0;False;-1;None;609fe16e845d333478e823e4ad786650;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;246;-2390.43,471.4098;Inherit;False;Metallic_out;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;250;-3294.969,1580.456;Inherit;False;237;uvs;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;252;-2573.622,-477.3295;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;450;-894.6644,-143.0356;Inherit;False;Tangent;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.GetLocalVarNode;279;-1028.919,-309.965;Inherit;False;273;Normal_out;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;251;-972.4704,349.4021;Inherit;False;246;Metallic_out;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;447;-1200.261,90.5563;Inherit;False;Sample Lightmap Atlas;0;;1;2285874493434b44fafddcf5f59663b9;1,209,0;0;3;COLOR;352;FLOAT3;0;FLOAT3;178
Node;AmplifyShaderEditor.RegisterLocalVarNode;257;-2275.421,-610.5335;Inherit;False;Albedo_out;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.DotProductOpNode;451;-492.1595,-165.158;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;254;-2846.939,1580.677;Inherit;True;Property;_EmissionMap;Emission Map;11;2;[NoScaleOffset];[SingleLineTexture];Create;False;0;0;0;False;0;False;-1;None;None;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleSubtractOpNode;253;-629.994,200.234;Inherit;False;2;0;FLOAT3;0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;259;-508.1682,419.3266;Inherit;False;Property;_LightIntensity1;Light Intensity;14;0;Create;True;0;0;0;False;0;False;1;1;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;261;-2485.35,1493.778;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;264;-571.8495,69.8996;Inherit;False;257;Albedo_out;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;452;-267.7488,-153.0445;Inherit;False;3;0;FLOAT;0.5;False;1;FLOAT;0.3;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;260;-3029.364,918.2603;Inherit;False;Property;_Smoothness1;Smoothness;12;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;263;-62.35153,312.6992;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;266;-2170.372,1491.485;Inherit;False;Emission_out;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;453;-75.17822,-129.9732;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;281;-384,864;Inherit;False;Property;_Dissolve;Dissolve;28;0;Create;True;0;0;0;False;0;False;1;0.438;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;265;-2626.608,778.61;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;268;177.9068,215.7485;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;272;20.84939,419.7656;Inherit;False;266;Emission_out;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;454;80,768;Inherit;False;Dissolve Orly;16;;112;98efd218901405c4290b92d5f67c74d3;0;1;43;FLOAT;1;False;3;COLOR;146;FLOAT;42;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;271;-2347.19,915.3706;Inherit;False;Smoothness_out;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;274;416,288;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;294;585.5491,896.5869;Inherit;False;3;0;INT;0;False;1;INT;0;False;2;INT;0;False;1;INT;0
Node;AmplifyShaderEditor.ColorNode;255;-2837.465,1345.118;Inherit;False;Property;_EmissionColor1;Emission Color;13;1;[HDR];Create;True;0;0;0;False;0;False;0,0,0,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;258;-382.7159,250.2529;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;277;709.9374,209.4932;Inherit;False;246;Metallic_out;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;278;695.1049,289.0341;Inherit;False;271;Smoothness_out;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;292;829.7893,746.1211;Inherit;False;2;2;0;FLOAT;0;False;1;INT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;276;256,-176;Inherit;False;257;Albedo_out;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.ConditionalIfNode;435;128,-800;Inherit;False;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;430;639.9384,557.4294;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;205;1126.837,83.69457;Float;False;True;-1;2;LightMapFusion.CustomShaderGUI;0;0;Standard;Orly Shader/LightMap Fusion/Built-in/Dissolve/Standard;False;False;False;False;True;False;True;True;False;False;False;False;False;False;True;False;False;False;True;True;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Masked;0.5;True;True;0;False;TransparentCutout;;AlphaTest;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;Standard;-1;-1;-1;-1;0;False;0;0;False;;-1;0;True;_Dissolve;0;0;0;False;0.1;False;;0;False;;False;17;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
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
WireConnection;451;0;279;0
WireConnection;451;1;450;0
WireConnection;254;1;250;0
WireConnection;253;0;447;0
WireConnection;253;1;251;0
WireConnection;261;0;447;352
WireConnection;261;1;254;0
WireConnection;452;0;451;0
WireConnection;263;0;253;0
WireConnection;263;1;259;0
WireConnection;266;0;261;0
WireConnection;453;0;452;0
WireConnection;453;1;264;0
WireConnection;265;0;240;4
WireConnection;265;1;260;0
WireConnection;268;0;453;0
WireConnection;268;1;263;0
WireConnection;454;43;281;0
WireConnection;271;0;265;0
WireConnection;274;0;268;0
WireConnection;274;1;272;0
WireConnection;258;0;253;0
WireConnection;292;0;454;42
WireConnection;292;1;294;0
WireConnection;430;0;274;0
WireConnection;430;1;454;146
WireConnection;430;2;454;0
WireConnection;205;0;276;0
WireConnection;205;1;279;0
WireConnection;205;2;430;0
WireConnection;205;3;277;0
WireConnection;205;4;278;0
WireConnection;205;10;292;0
ASEEND*/
//CHKSM=71675C416F94AFC9E6825260A3DEBEC41E9E1BFF