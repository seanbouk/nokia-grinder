Shader "Custom/Nokia1BitMosaicShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Threshold ("Threshold", Range(0,1)) = 0.9
        _Color1 ("Color 1", Color) = (1, 1, 1, 1)
        _Color2 ("Color 2", Color) = (0, 0, 0, 1)
        _MosaicSizeX ("Mosaic Size X", Range(1,320)) = 16
        _MosaicSizeY ("Mosaic Size Y", Range(1,240)) = 16
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
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
            };
            
            sampler2D _MainTex;
            float _Threshold;
            fixed4 _Color1;
            fixed4 _Color2;
            float _MosaicSizeX;
            float _MosaicSizeY;
            
            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Create a mosaic effect by snapping UVs independently along each axis.
                float2 mosaicFactor = float2(_MosaicSizeX, _MosaicSizeY);
                float2 mosaicUV = floor(i.uv * mosaicFactor) / mosaicFactor;
                float grayscale = tex2D(_MainTex, mosaicUV).r;
                return grayscale > _Threshold ? _Color1 : _Color2;
            }
            
            ENDCG
        }
    }
}
