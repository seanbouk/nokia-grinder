Shader "Custom/LitTransparent"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "RenderType"="Transparent"}
        LOD 200
        
        Cull Off   // Enables double-sided rendering
        ZWrite Off // Prevents depth issues with transparency
        Blend SrcAlpha OneMinusSrcAlpha // Proper transparency blending

        CGPROGRAM
        #pragma surface surf Standard alpha:fade doubleSided
        #include "UnityCG.cginc"

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
            if (c.r + c.g + c.b < 0.1)  // If the pixel is almost black
                c.a = 0;  // Make it fully transparent
            o.Albedo = c.rgb;
            o.Alpha = c.a;

        }
        ENDCG
    }
}
