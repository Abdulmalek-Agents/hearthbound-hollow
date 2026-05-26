// Upgrade NOTE: replaced tex2D unity_Lightmap with UNITY_SAMPLE_TEX2D

// Made with Amplify Shader Editor v1.9.3.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Orly Shader/LightMap Fusion/Built-in/Unlit"
{
	Properties
	{
		[HDR]_EmisionColor_1("EmisionColor_1", Color) = (0,0,0,0)
		[HDR]_EmisionColor_2("EmisionColor_2", Color) = (0,0,0,0)
		[HDR]_EmisionColor_3("EmisionColor_3", Color) = (0,0,0,0)
		[HDR]_EmisionColor_4("EmisionColor_4", Color) = (0,0,0,0)
		[NoScaleOffset][SingleLineTexture]_MainTex("Texture", 2D) = "white" {}
		_Color0("Color 0", Color) = (1,1,1,0)
		_Tiling("Tiling", Float) = 1
		_LightIntensity("Light Intensity", Range( 0 , 10)) = 1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] _texcoord2( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#include "UnityCG.cginc"
		#pragma target 3.0
		#pragma surface surf Unlit keepalpha addshadow fullforwardshadows noambient novertexlights nolightmap  
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

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float4 lerpResult349_g109 = lerp( ( _Set3Intensity * _EmisionColor_3 ) , ( _Set4Intensity * _EmisionColor_4 ) , _BlendLightmapX);
			float4 lerpResult350_g109 = lerp( ( _Set2Intensity * _EmisionColor_2 ) , ( _Set1Intensity * _EmisionColor_1 ) , _BlendLightmapX);
			float4 lerpResult351_g109 = lerp( lerpResult349_g109 , lerpResult350_g109 , _BlendLightmapY);
			float4 appendResult10 = (float4(_Tiling , _Tiling , 0.0 , 0.0));
			float2 uv_TexCoord9 = i.uv_texcoord * appendResult10.xy;
			float2 uvs8 = uv_TexCoord9;
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
			o.Emission = ( lerpResult351_g109 + ( tex2D( _MainTex, uvs8 ) * _Color0 * float4( ( staticSwitch226_g109 * _LightIntensity ) , 0.0 ) ) ).rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Standard"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19302
Node;AmplifyShaderEditor.RangedFloatNode;11;-2549.737,-112.0786;Inherit;False;Property;_Tiling;Tiling;8;0;Create;True;0;0;0;False;0;False;1;5.03;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;10;-2241.379,-121.1557;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;9;-1916.015,-99.14651;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;8;-1571.884,-94.43292;Inherit;False;uvs;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;5;-6.570557,429.6035;Inherit;False;8;uvs;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;35;426.9776,124.8381;Inherit;False;Property;_LightIntensity;Light Intensity;9;0;Create;True;0;0;0;False;0;False;1;1;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;37;328.2451,-198.9031;Inherit;False;Sample Lightmap Atlas;0;;109;2285874493434b44fafddcf5f59663b9;1,209,0;0;3;COLOR;352;FLOAT3;0;FLOAT3;178
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;36;843.5244,120.2098;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;2;357.8453,249.8048;Inherit;True;Property;_MainTex;Texture;6;2;[NoScaleOffset];[SingleLineTexture];Create;False;0;0;0;False;0;False;-1;None;609fe16e845d333478e823e4ad786650;True;0;False;white;LockedToTexture2D;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;4;910.3461,391.5275;Inherit;False;Property;_Color0;Color 0;7;0;Create;True;0;0;0;False;0;False;1,1,1,0;1,0.6273585,0.6273585,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;3;1226.194,194.9081;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;38;1476.355,152.5074;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1670.777,141.9771;Float;False;True;-1;2;ASEMaterialInspector;0;0;Unlit;Orly Shader/LightMap Fusion/Built-in/Unlit;False;False;False;False;True;True;True;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;Standard;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;10;0;11;0
WireConnection;10;1;11;0
WireConnection;9;0;10;0
WireConnection;8;0;9;0
WireConnection;36;0;37;0
WireConnection;36;1;35;0
WireConnection;2;1;5;0
WireConnection;3;0;2;0
WireConnection;3;1;4;0
WireConnection;3;2;36;0
WireConnection;38;0;37;352
WireConnection;38;1;3;0
WireConnection;0;2;38;0
ASEEND*/
//CHKSM=6CA47C3ADB37C2934BE6A7F94CFF0B499239DFD5