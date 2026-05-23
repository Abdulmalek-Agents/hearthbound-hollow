// Upgrade NOTE: replaced tex2D unity_Lightmap with UNITY_SAMPLE_TEX2D

// Made with Amplify Shader Editor v1.9.3.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Orly Shader/LightMap Fusion/Built-in/Unlit Cutout"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		[HDR]_EmisionColor_1("EmisionColor_1", Color) = (0,0,0,0)
		[HDR]_EmisionColor_2("EmisionColor_2", Color) = (0,0,0,0)
		[HDR]_EmisionColor_3("EmisionColor_3", Color) = (0,0,0,0)
		[HDR]_EmisionColor_4("EmisionColor_4", Color) = (0,0,0,0)
		_Color0("Color 0", Color) = (1,1,1,0)
		[NoScaleOffset][SingleLineTexture]_MainTex("Diffuse", 2D) = "white" {}
		_Tiling("Tiling", Float) = 1
		_LightIntensity("Light Intensity", Range( 0 , 3)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] _texcoord2( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "AlphaTest+0" "IgnoreProjector" = "True" "ForceNoShadowCasting" = "True" "IsEmissive" = "true"  }
		Cull Off
		CGPROGRAM
		#include "UnityCG.cginc"
		#pragma target 4.5
		#pragma surface surf Unlit keepalpha addshadow fullforwardshadows novertexlights nodynlightmap nometa noforwardadd 
		struct Input
		{
			float2 uv_texcoord;
			float2 uv2_texcoord2;
		};

		uniform float _Set3Intensity;
		uniform float4 _EmisionColor_3;
		uniform float _Set4Intensity;
		uniform float4 _EmisionColor_4;
		uniform float _BlendLightmapX;
		uniform float _Set2Intensity;
		uniform float4 _EmisionColor_2;
		uniform float _Set1Intensity;
		uniform float4 _EmisionColor_1;
		uniform float _BlendLightmapY;
		uniform sampler2D _MainTex;
		uniform float _Tiling;
		uniform float4 _Color0;
		uniform float _LightIntensity;
		uniform float _Cutoff = 0.5;

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float4 lerpResult349_g1 = lerp( ( _Set3Intensity * _EmisionColor_3 ) , ( _Set4Intensity * _EmisionColor_4 ) , _BlendLightmapX);
			float4 lerpResult350_g1 = lerp( ( _Set2Intensity * _EmisionColor_2 ) , ( _Set1Intensity * _EmisionColor_1 ) , _BlendLightmapX);
			float4 lerpResult351_g1 = lerp( lerpResult349_g1 , lerpResult350_g1 , _BlendLightmapY);
			float4 appendResult10 = (float4(_Tiling , _Tiling , 0.0 , 0.0));
			float2 uv_TexCoord9 = i.uv_texcoord * appendResult10.xy;
			float2 uvs8 = uv_TexCoord9;
			float4 tex2DNode2 = tex2D( _MainTex, uvs8 );
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
			o.Emission = ( lerpResult351_g1 + ( tex2DNode2 * _Color0 * float4( ( staticSwitch226_g1 * _LightIntensity ) , 0.0 ) ) ).rgb;
			o.Alpha = 1;
			clip( tex2DNode2.a - _Cutoff );
		}

		ENDCG
	}
	Fallback "Standard"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19302
Node;AmplifyShaderEditor.RangedFloatNode;11;-2549.737,-112.0786;Inherit;False;Property;_Tiling;Tiling;9;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;10;-2241.379,-121.1557;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;9;-1916.015,-99.14651;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;8;-1571.884,-94.43292;Inherit;False;uvs;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;5;-6.570557,429.6035;Inherit;False;8;uvs;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;6;154.4079,130.504;Inherit;False;Property;_LightIntensity;Light Intensity;10;0;Create;True;0;0;0;False;0;False;0;1.06;0;3;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;19;328.2451,-198.9031;Inherit;False;Sample Lightmap Atlas;1;;1;2285874493434b44fafddcf5f59663b9;1,209,0;0;3;COLOR;352;FLOAT3;0;FLOAT3;178
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;12;789.7897,84.37749;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;2;357.8453,249.8048;Inherit;True;Property;_MainTex;Diffuse;8;2;[NoScaleOffset];[SingleLineTexture];Create;False;0;0;0;False;0;False;-1;None;6c39fd4a3533c33498c196a3bb1cb67b;True;0;False;white;LockedToTexture2D;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;4;838.2159,544.6459;Inherit;False;Property;_Color0;Color 0;7;0;Create;True;0;0;0;False;0;False;1,1,1,0;0.1037736,0.1037736,0.1037736,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;3;1226.194,194.9081;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;20;1475.368,186.8336;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1696.368,222.4345;Float;False;True;-1;5;ASEMaterialInspector;0;0;Unlit;Orly Shader/LightMap Fusion/Built-in/Unlit Cutout;False;False;False;False;False;True;False;True;False;False;True;True;False;False;True;True;False;False;False;False;False;Off;0;False;;0;False;;False;0;False;;0;False;;False;0;Masked;0.5;True;True;0;False;TransparentCutout;;AlphaTest;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;5;False;;10;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;Standard;0;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;10;0;11;0
WireConnection;10;1;11;0
WireConnection;9;0;10;0
WireConnection;8;0;9;0
WireConnection;12;0;19;0
WireConnection;12;1;6;0
WireConnection;2;1;5;0
WireConnection;3;0;2;0
WireConnection;3;1;4;0
WireConnection;3;2;12;0
WireConnection;20;0;19;352
WireConnection;20;1;3;0
WireConnection;0;2;20;0
WireConnection;0;10;2;4
ASEEND*/
//CHKSM=EB5FF6C395456508ACDE30326D77CFB06D43268A