// Made with Amplify Shader Editor v1.9.3.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Orly Shader/LightMap Fusion/Skybox/Built-Iin/Blend Skybox 4"
{
	Properties
	{
		_Color1("Color 1", Color) = (0,0,0,0)
		_Color2("Color 2", Color) = (0,0,0,0)
		_Color3("Color 3", Color) = (1,0.2688679,0.2688679,0)
		_Color4("Color 4", Color) = (0.25,0.5985248,1,0)
		_blendSkyX("blendSkyX", Range( 0 , 1)) = 0
		_blendSkyY("blendSkyY", Range( 0 , 1)) = 0
		_Exposure("Exposure", Range( 0 , 8)) = 0
		_Rotation("Rotation", Range( 0 , 360)) = 0
		[HDR]_Skybox1("Skybox 1", CUBE) = "gray" {}
		[HDR]_Skybox2("Skybox 2", CUBE) = "gray" {}
		[HDR]_Skybox4("Skybox 4", CUBE) = "gray" {}
		[HDR]_Skybox3("Skybox 3", CUBE) = "gray" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Background"  "Queue" = "Background+0" "IgnoreProjector" = "True" "ForceNoShadowCasting" = "True" "IsEmissive" = "true"  }
		Cull Off
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Unlit keepalpha noshadow noambient novertexlights nolightmap  nodynlightmap nodirlightmap nofog nometa noforwardadd 
		struct Input
		{
			half3 worldRefl;
			INTERNAL_DATA
		};

		uniform half _Exposure;
		uniform samplerCUBE _Skybox3;
		uniform half _Rotation;
		uniform half4 _Color3;
		uniform samplerCUBE _Skybox4;
		uniform half4 _Color4;
		uniform half _blendSkyX;
		uniform samplerCUBE _Skybox1;
		uniform half4 _Color2;
		uniform samplerCUBE _Skybox2;
		uniform half4 _Color1;
		uniform half _blendSkyY;


		float3 ASESafeNormalize(float3 inVec)
		{
			float dp3 = max(1.175494351e-38, dot(inVec, inVec));
			return inVec* rsqrt(dp3);
		}


		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			o.Normal = float3(0,0,1);
			half temp_output_121_0 = ( ( UNITY_PI / 180.0 ) * _Rotation );
			half temp_output_123_0 = cos( temp_output_121_0 );
			half temp_output_124_0 = sin( temp_output_121_0 );
			half2 appendResult128 = (half2(temp_output_123_0 , ( 1.0 - temp_output_124_0 )));
			half2 appendResult130 = (half2(temp_output_124_0 , temp_output_123_0));
			half3 normalizeResult98 = ASESafeNormalize( mul( float3x3(half3( appendResult128 ,  0.0 ), float3( 0,0,0 ), half3( appendResult130 ,  0.0 )), half3(1,1,1) ) );
			half3 newWorldReflection49 = normalize( WorldReflectionVector( i , normalizeResult98 ) );
			half4 lerpResult60 = lerp( ( texCUBE( _Skybox3, newWorldReflection49 ) * _Color3 ) , ( texCUBE( _Skybox4, newWorldReflection49 ) * _Color4 ) , _blendSkyX);
			half4 lerpResult61 = lerp( ( texCUBE( _Skybox1, newWorldReflection49 ) * _Color2 ) , ( texCUBE( _Skybox2, newWorldReflection49 ) * _Color1 ) , _blendSkyX);
			half4 lerpResult69 = lerp( lerpResult60 , lerpResult61 , _blendSkyY);
			o.Emission = ( _Exposure * lerpResult69 ).rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19302
Node;AmplifyShaderEditor.CommentaryNode;133;-6284.631,519.4848;Inherit;False;2188.025;599.527;Comment;13;114;91;115;121;124;123;129;128;130;126;98;97;132;Rotation;1,1,1,1;0;0
Node;AmplifyShaderEditor.PiNode;114;-6234.631,695.8159;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;115;-5911.964,770.9833;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;180;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;91;-6067.322,920.4817;Inherit;False;Property;_Rotation;Rotation;8;0;Create;True;0;0;0;False;0;False;0;89;0;360;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;121;-5684.96,855.1115;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;124;-5412.96,974.1116;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CosOpNode;123;-5427.96,771.1115;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;129;-5274.96,870.1115;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;128;-5020.96,826.1115;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;130;-5034.061,982.0127;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector3Node;132;-4808.124,569.4848;Inherit;False;Constant;_Vector0;Vector 0;13;0;Create;True;0;0;0;False;0;False;1,1,1;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.MatrixFromVectors;126;-4801.96,881.1115;Inherit;False;FLOAT3x3;True;4;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;1;FLOAT3x3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;97;-4522.554,626.6531;Inherit;False;2;2;0;FLOAT3x3;0,0,0,1,1,1,1,0,1;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NormalizeNode;98;-4274.606,622.6581;Inherit;False;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WorldReflectionVector;49;-3679.73,598.4262;Inherit;False;True;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.TexturePropertyNode;108;-3058.904,-142.1195;Inherit;True;Property;_Skybox4;Skybox 4;11;1;[HDR];Create;False;0;0;0;False;0;False;None;None;False;gray;LockedToCube;Cube;-1;0;2;SAMPLERCUBE;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.TexturePropertyNode;105;-3043.018,539.0146;Inherit;True;Property;_Skybox1;Skybox 1;9;1;[HDR];Create;True;0;0;0;False;0;False;None;23938a9edc076644685225f3428832d5;False;gray;LockedToCube;Cube;-1;0;2;SAMPLERCUBE;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.TexturePropertyNode;106;-3036.868,1021.019;Inherit;True;Property;_Skybox2;Skybox 2;10;1;[HDR];Create;True;0;0;0;False;0;False;None;65d6655fc3bb8b945a0aaa71a34be846;False;gray;LockedToCube;Cube;-1;0;2;SAMPLERCUBE;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.TexturePropertyNode;140;-3083.255,-535.2929;Inherit;True;Property;_Skybox3;Skybox 3;12;1;[HDR];Create;False;0;0;0;False;0;False;None;27097f5f3472dd4459e68cd013429abc;False;gray;LockedToCube;Cube;-1;0;2;SAMPLERCUBE;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SamplerNode;67;-2460.965,838.7947;Inherit;True;Property;_Skybox4;Skybox 4;6;2;[NoScaleOffset];[SingleLineTexture];Create;True;0;0;0;False;0;False;-1;None;27097f5f3472dd4459e68cd013429abc;True;1;False;white;LockedToCube;False;Object;-1;Auto;Cube;8;0;SAMPLERCUBE;;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT;1;False;6;FLOAT;1;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;14;-2386.049,1149.07;Inherit;False;Property;_Color1;Color 1;1;0;Create;True;0;0;0;False;0;False;0,0,0,0;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;15;-2385.581,610.4843;Inherit;False;Property;_Color2;Color 2;2;0;Create;True;0;0;0;False;0;False;0,0,0,0;0.7058823,0.7058823,0.7058823,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;18;-2412.438,150.9809;Inherit;False;Property;_Color4;Color 4;4;0;Create;True;0;0;0;False;0;False;0.25,0.5985248,1,0;0.7058823,0.7058823,0.7058823,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;17;-2419.895,-318.261;Inherit;False;Property;_Color3;Color 3;3;0;Create;True;0;0;0;False;0;False;1,0.2688679,0.2688679,0;0.7058823,0.7058823,0.7058823,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;110;-2378.48,-87.20045;Inherit;True;Property;_TextureSample1;Texture Sample 1;12;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Cube;8;0;SAMPLERCUBE;;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;109;-2445.018,-647.6795;Inherit;True;Property;_TextureSample0;Texture Sample 0;12;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Cube;8;0;SAMPLERCUBE;;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;139;-2372.031,374.3475;Inherit;True;Property;_TextureSample2;Texture Sample 2;14;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Cube;8;0;SAMPLERCUBE;;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;72;-1749.839,101.3173;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;71;-1783.935,-238.9751;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;73;-1742.745,435.4747;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;70;-1735.038,865.8171;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;20;-1407.996,329.3197;Inherit;False;Property;_blendSkyX;blendSkyX;5;0;Create;True;0;0;0;False;0;False;0;0.472;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;60;-874.864,-93.27958;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;61;-898.5544,460.8889;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;22;-1081.507,965.1956;Inherit;False;Property;_blendSkyY;blendSkyY;6;0;Create;True;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;69;-382.8176,309.6216;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;76;-566.1371,-7.755997;Inherit;False;Property;_Exposure;Exposure;7;0;Create;True;0;0;0;False;0;False;0;2.04;0;8;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;78;26.86292,331.244;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;13;583,216;Half;False;True;-1;2;ASEMaterialInspector;0;0;Unlit;Orly Shader/LightMap Fusion/Skybox/Built-Iin/Blend Skybox 4;False;False;False;False;True;True;True;True;True;True;True;True;False;False;True;True;False;False;False;False;False;Off;0;False;;0;False;;False;0;False;;0;False;;False;0;Custom;0.5;True;False;0;True;Background;;Background;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;False;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;0;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;115;0;114;0
WireConnection;121;0;115;0
WireConnection;121;1;91;0
WireConnection;124;0;121;0
WireConnection;123;0;121;0
WireConnection;129;0;124;0
WireConnection;128;0;123;0
WireConnection;128;1;129;0
WireConnection;130;0;124;0
WireConnection;130;1;123;0
WireConnection;126;0;128;0
WireConnection;126;2;130;0
WireConnection;97;0;126;0
WireConnection;97;1;132;0
WireConnection;98;0;97;0
WireConnection;49;0;98;0
WireConnection;67;0;106;0
WireConnection;67;1;49;0
WireConnection;110;0;108;0
WireConnection;110;1;49;0
WireConnection;109;0;140;0
WireConnection;109;1;49;0
WireConnection;139;0;105;0
WireConnection;139;1;49;0
WireConnection;72;0;110;0
WireConnection;72;1;18;0
WireConnection;71;0;109;0
WireConnection;71;1;17;0
WireConnection;73;0;139;0
WireConnection;73;1;15;0
WireConnection;70;0;67;0
WireConnection;70;1;14;0
WireConnection;60;0;71;0
WireConnection;60;1;72;0
WireConnection;60;2;20;0
WireConnection;61;0;73;0
WireConnection;61;1;70;0
WireConnection;61;2;20;0
WireConnection;69;0;60;0
WireConnection;69;1;61;0
WireConnection;69;2;22;0
WireConnection;78;0;76;0
WireConnection;78;1;69;0
WireConnection;13;2;78;0
ASEEND*/
//CHKSM=F1E3EA959FBEC56F92DA0120059441DB9D2D21A9