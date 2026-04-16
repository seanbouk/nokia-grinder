Shader "Custom/UnlitBlackTransparent" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _Rotation ("UV Rotation (Degrees)", Range(0,360)) = 0
    }
    SubShader {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Pass {
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Rotation; // rotation angle in degrees

            v2f vert (appdata_t v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                // Rotate the UVs about the center (0.5,0.5)
                float rad = _Rotation * UNITY_PI / 180.0;
                float cosA = cos(rad);
                float sinA = sin(rad);
                float2 center = float2(0.5, 0.5);
                float2 uv = i.uv - center;
                float2 rotatedUV;
                rotatedUV.x = uv.x * cosA - uv.y * sinA;
                rotatedUV.y = uv.x * sinA + uv.y * cosA;
                rotatedUV += center;
                
                fixed4 col = tex2D(_MainTex, rotatedUV);
                if (col.r < 0.1 && col.g < 0.1 && col.b < 0.1) {
                    col.a = 0; // Make black pixels transparent
                }
                return col;
            }
            ENDCG
        }
    }
}
