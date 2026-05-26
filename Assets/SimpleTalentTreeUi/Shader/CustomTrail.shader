Shader "Custom/CustomTrail"
{
    Properties
    {
        _MainTex ("Shape Texture", 2D) = "white" {}
        [GradientDrawer] _TrailWidthGradient ("Trail Width Gradient", 2D) = "white" {}
        _MainTexScrollX ("Main Texture Scroll X", Range(-100, 100)) = 1
        _MainTexScrollY ("Main Texture Scroll Y", Range(-100, 100)) = 1
        _TrailWidthPower ("Trail Width Power", Range(0.1, 10)) = 1
        _MainTexOffset ("Main Texture Offset", Vector) = (0, 0, 0, 0)
        _Color ("Object Color", Color) = (1, 1, 1, 1)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Blend One One // Additive blending
            Zwrite Off
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
            };

            sampler2D _MainTex;
            sampler2D _NoiseTex;
            sampler2D _TrailWidthGradient;
            float2  _MainTexOffset;
            float _MainTexScrollX;
            float _MainTexScrollY;
            float _TrailWidthPower;
            fixed4 _Color;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {

                float width = pow(tex2D(_TrailWidthGradient, i.uv).r, _TrailWidthPower);
                i.uv.y = (i.uv.y * 2 - 1) / width * 0.5 + 0.5;
                clip(i.uv.y);
                clip(1 - i.uv.y);

                half4 shapeColor = tex2D(_MainTex, i.uv);

                float2 shapeUV = i.uv + float2(_MainTexScrollX, _MainTexScrollY) * _Time.xy;

                shapeUV += _MainTexOffset;

                shapeColor = tex2D(_MainTex, shapeUV) * i.color;

                return shapeColor;
            }
            ENDCG
        }
    }

    FallBack "Diffuse"
}
