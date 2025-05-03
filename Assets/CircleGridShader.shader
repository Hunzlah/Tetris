Shader "Custom/CircleGridShader"
{
    Properties
    {
        _GridSize ("Grid Size", Float) = 20.0
        _MainTex ("Texture", 2D) = "white" {}
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

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _GridSize;
            float4 _ScreenParams; // Needed for screen resolution

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv * 2.0 - 1.0; // convert to [-1, 1]
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;

                if (length(uv) > 1.0) discard;

                float r = length(uv);
                float theta = atan2(uv.y, uv.x);
                float2 polarUV = float2(theta / (2 * UNITY_PI) + 0.5, r);

                float2 grid = frac(polarUV * _GridSize);

                float line = step(0.98, grid.x) + step(0.98, grid.y);
                return float4(line.xxx, 1.0);
            }
            ENDCG
        }
    }
}
