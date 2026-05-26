// Upgrade NOTE: replaced tex2D unity_Lightmap with UNITY_SAMPLE_TEX2D

// Made with Amplify Shader Editor v1.9.3.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Orly Shader/LightMap Fusion/Built-in/Dissolve/Unlit"
{
	Properties
	{
		_MainTex("Diffuse", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
		_LightIntensity("Light Intensity", Range( 0 , 10)) = 1
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
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "AlphaTest+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Off
		CGPROGRAM
		#include "UnityCG.cginc"
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma shader_feature_local _ENABLEDISSOLVE_DISABLED _ENABLEDISSOLVE_ENABLED
		#pragma shader_feature_local _MASKSELECTOR_PLANE _MASKSELECTOR_SPHERE _MASKSELECTOR_BOX
		#pragma surface surf Unlit keepalpha addshadow fullforwardshadows nolightmap  nodynlightmap 
		struct Input
		{
			float2 uv_texcoord;
			float2 uv2_texcoord2;
			float3 worldPos;
		};

		uniform int _vID;
		uniform float4 _Color;
		uniform sampler2D _MainTex;
		uniform float4 _MainTex_ST;
		uniform float _Set3Intensity;
		uniform float _Set4Intensity;
		uniform float _BlendLightmapX;
		uniform float _Set2Intensity;
		uniform float _Set1Intensity;
		uniform float _BlendLightmapY;
		uniform float _LightIntensity;
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


		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float2 uv_MainTex = i.uv_texcoord * _MainTex_ST.xy + _MainTex_ST.zw;
			float2 temp_output_266_0_g109 = ( (i.uv2_texcoord2*(unity_LightmapST).xy + (unity_LightmapST).zw) * float2( 0.5,0.5 ) );
			float4 lerpResult274_g109 = lerp( ( UNITY_SAMPLE_TEX2D( unity_Lightmap, ( float2( 0,0.5 ) + temp_output_266_0_g109 ) ) * _Set3Intensity ) , ( UNITY_SAMPLE_TEX2D( unity_Lightmap, ( float2( 0.5,0.5 ) + temp_output_266_0_g109 ) ) * _Set4Intensity ) , _BlendLightmapX);
			float4 lerpResult276_g109 = lerp( ( UNITY_SAMPLE_TEX2D( unity_Lightmap, ( temp_output_266_0_g109 + float2( 0.5,0 ) ) ) * _Set2Intensity ) , ( UNITY_SAMPLE_TEX2D( unity_Lightmap, ( temp_output_266_0_g109 + float2( 0,0 ) ) ) * _Set1Intensity ) , _BlendLightmapX);
			float4 lerpResult278_g109 = lerp( lerpResult274_g109 , lerpResult276_g109 , _BlendLightmapY);
			float3 decodeLightMap290_g109 = DecodeLightmap(lerpResult278_g109);
			#ifdef DIRLIGHTMAP_COMBINED
				float3 staticSwitch226_g109 = float3( 0,0,0 );
			#else
				float3 staticSwitch226_g109 = decodeLightMap290_g109;
			#endif
			#if defined(_ENABLEDISSOLVE_DISABLED)
				float staticSwitch94_g113 = (float)0;
			#elif defined(_ENABLEDISSOLVE_ENABLED)
				float staticSwitch94_g113 = (float)1;
			#else
				float staticSwitch94_g113 = (float)1;
			#endif
			float3 ase_worldPos = i.worldPos;
			float2 appendResult29_g113 = (float2(_NoiseScale , _NoiseScale));
			float simplePerlin3D31_g113 = snoise( ( ase_worldPos + ( _NoiseScrollSpeed * _Time.y ) )*appendResult29_g113.x );
			simplePerlin3D31_g113 = simplePerlin3D31_g113*0.5 + 0.5;
			float temp_output_43_0_g113 = _Dissolve;
			float temp_output_37_0_g113 = (0.0 + (temp_output_43_0_g113 - 0.1) * (1.0 - 0.0) / (1.0 - 0.1));
			float Noise33_g113 = ( simplePerlin3D31_g113 - temp_output_37_0_g113 );
			float clampResult64_g113 = clamp( Noise33_g113 , 0.1 , 1.0 );
			float3 temp_output_132_0_g113 = ( ase_worldPos - _GizmoCenter );
			float dotResult25_g113 = dot( temp_output_132_0_g113 , _PlaneDirection );
			float temp_output_142_0_g113 = ( dotResult25_g113 + 0.0 );
			float3 temp_output_5_0_g114 = ( ( ase_worldPos - _GizmoCenter ) / _SphereRadius );
			float dotResult8_g114 = dot( temp_output_5_0_g114 , temp_output_5_0_g114 );
			#if defined(_MASKSELECTOR_PLANE)
				float staticSwitch50_g113 = temp_output_142_0_g113;
			#elif defined(_MASKSELECTOR_SPHERE)
				float staticSwitch50_g113 = pow( saturate( dotResult8_g114 ) , 1.0 );
			#elif defined(_MASKSELECTOR_BOX)
				float staticSwitch50_g113 = ( distance( max( ( abs( ( ase_worldPos - _GizmoCenter ) ) - ( _BoxSize * float3( 0.5,0.5,0.5 ) ) ) , float3( 0,0,0 ) ) , float3( 0,0,0 ) ) / 1.0 );
			#else
				float staticSwitch50_g113 = temp_output_142_0_g113;
			#endif
			float Gradient32_g113 = (( _Invert )?( ( 1.0 - staticSwitch50_g113 ) ):( staticSwitch50_g113 ));
			float temp_output_74_0_g113 = ( ( ( clampResult64_g113 * ( 1.0 - Gradient32_g113 ) ) - Gradient32_g113 ) - temp_output_37_0_g113 );
			float4 lerpResult414 = lerp( ( _Color * tex2D( _MainTex, uv_MainTex ) * float4( ( staticSwitch226_g109 * _LightIntensity ) , 0.0 ) ) , _EdgeColor , ( staticSwitch94_g113 != 0.0 ? step( temp_output_74_0_g113 , ( temp_output_37_0_g113 + _EdgeThickness ) ) : 0.0 ));
			o.Emission = lerpResult414.rgb;
			o.Alpha = 1;
			int clampResult323 = clamp( 0 , 0 , 0 );
			clip( ( ( staticSwitch94_g113 != 0.0 ? temp_output_74_0_g113 : 1.0 ) + clampResult323 ) - _Dissolve );
		}

		ENDCG
	}
	Fallback "Unlit/Texture"
	CustomEditor "LightMapFusion.CustomShaderGUI"
}
/*ASEBEGIN
Version=19302
Node;AmplifyShaderEditor.RangedFloatNode;309;1369.395,-871.247;Inherit;False;Property;_LightIntensity;Light Intensity;8;0;Create;True;0;0;0;False;0;False;1;1;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;425;1048.209,-1018.632;Inherit;False;Sample Lightmap Atlas;0;;109;2285874493434b44fafddcf5f59663b9;1,209,0;0;3;COLOR;352;FLOAT3;0;FLOAT3;178
Node;AmplifyShaderEditor.RangedFloatNode;98;1154.708,-52.42949;Inherit;False;Property;_Dissolve;Dissolve;21;0;Create;True;0;0;0;False;0;False;1;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;308;1761.491,-918.5214;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ColorNode;3;1169.734,-714.0243;Inherit;False;Property;_Color;Color;7;0;Create;True;0;0;0;False;0;False;1,1,1,1;1,0.9575471,0.9575471,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;2;1258.499,-488.2957;Inherit;True;Property;_MainTex;Diffuse;6;0;Create;False;1;Textures;0;0;False;0;False;-1;None;609fe16e845d333478e823e4ad786650;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;428;1648.716,-67.28065;Inherit;False;Dissolve Orly;9;;113;98efd218901405c4290b92d5f67c74d3;0;1;43;FLOAT;1;False;3;COLOR;146;FLOAT;42;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;5;1814.057,-585.1757;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;323;2070.541,27.07965;Inherit;False;3;0;INT;0;False;1;INT;0;False;2;INT;0;False;1;INT;0
Node;AmplifyShaderEditor.LerpOp;414;2096,-416;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;319;2330.541,-159.9203;Inherit;False;2;2;0;FLOAT;0;False;1;INT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;2778.535,-417.3909;Float;False;True;-1;2;LightMapFusion.CustomShaderGUI;0;0;Unlit;Orly Shader/LightMap Fusion/Built-in/Dissolve/Unlit;False;False;False;False;False;False;True;True;False;False;False;False;False;False;True;False;False;False;False;False;False;Off;0;False;;0;False;;False;0;False;;0;False;;False;0;Masked;0.5;True;True;0;False;TransparentCutout;;AlphaTest;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;Unlit/Texture;-1;-1;-1;-1;0;False;0;0;False;;-1;0;True;_Dissolve;0;0;0;False;0.1;False;;0;False;;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;308;0;425;0
WireConnection;308;1;309;0
WireConnection;428;43;98;0
WireConnection;5;0;3;0
WireConnection;5;1;2;0
WireConnection;5;2;308;0
WireConnection;414;0;5;0
WireConnection;414;1;428;146
WireConnection;414;2;428;0
WireConnection;319;0;428;42
WireConnection;319;1;323;0
WireConnection;0;2;414;0
WireConnection;0;10;319;0
ASEEND*/
//CHKSM=F4FD755AEA4467AFB9910479032F21659C89FEF5