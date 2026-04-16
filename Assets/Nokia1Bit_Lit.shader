Shader "Custom/Nokia1Bit_Lit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Threshold ("Threshold", Range(0,1)) = 0.5
        _Color1 ("Color 1", Color) = (1, 1, 1, 1) // White
        _Color2 ("Color 2", Color) = (0, 0, 0, 1)   // Black
        _GridSizeX ("Grid Size X", Float) = 8
        _GridSizeY ("Grid Size Y", Float) = 4
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows

        struct Input
        {
            float2 uv_MainTex;
        };

        sampler2D _MainTex;
        float4 _Threshold;
        float _GridSizeX;
        float _GridSizeY;
        fixed4 _Color1;
        fixed4 _Color2;

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Calculate mosaic grid cell
            float2 cellSize = float2(1.0 / _GridSizeX, 1.0 / _GridSizeY);
            float2 cell = floor(IN.uv_MainTex / cellSize);
            float2 cellUV = (cell + 0.5) * cellSize; // Center of the cell

            // Sample texture at cell center and get brightness
            // Force all pixels in the cell to use the same color by snapping to cell center
            fixed4 texColor = tex2D(_MainTex, floor(IN.uv_MainTex / cellSize) * cellSize + cellSize * 0.5);
            float brightness = dot(texColor.rgb, float3(0.299, 0.587, 0.114));

            // Apply 1-bit dithering threshold
            float threshold = _Threshold.r;
            fixed4 finalColor = brightness > threshold ? _Color1 : _Color2;

            // Set shader output
            o.Albedo = finalColor.rgb;
            o.Alpha = finalColor.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
