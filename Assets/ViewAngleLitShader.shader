Shader "Custom/ViewAngleLit"
{
    Properties
    {
        _Color ("Base Color", Color) = (1,1,1,1)
        _DarkenAmount ("Darken Amount", Range(0,1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 worldNormal : TEXCOORD0;
                float3 viewDir : TEXCOORD1;
            };

            fixed4 _Color;
            float _DarkenAmount;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                
                // Calculate world normal
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                
                // Calculate view direction
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.viewDir = normalize(_WorldSpaceCameraPos - worldPos);
                
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Calculate dot product between normal and view direction
                float viewDot = dot(normalize(i.worldNormal), normalize(i.viewDir));
                
                // Remap dot product from [-1,1] to [0,1]
                viewDot = viewDot * 0.5 + 0.5;
                
                // Calculate darkening factor based on view angle
                float darkening = lerp(1.0 - _DarkenAmount, 1.0, viewDot);
                
                // Apply darkening to base color
                fixed4 col = _Color * darkening;
                return col;
            }
            ENDCG
        }
    }
}
