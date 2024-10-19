Shader "Unlit/CloudNoise"
{
    Properties
    {
        _Scale("Scale", Range(1, 10)) = 5
        _Speed("Speed", Range(0.1, 2.0)) = 0.5
        _Intensity("Intensity", Range(0, 1)) = 0.5
        _TransparencyThreshold("Transparency Threshold", Range(0, 1)) = 0.5
        _Density("Density", Range(0, 1)) = 1
        _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        _EffectColor("Effect Color", Color) = (1, 1, 1, 1)
    }
        SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        Pass
        {
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float _Scale;
            float _Speed;
            float _Intensity;
            float _TransparencyThreshold;
            float _Density;
            float4 _BaseColor;
            float4 _EffectColor;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);

                // Calculate UVs based on world position
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.uv = worldPos.xy / _Scale;

                return o;
            }

            float noise(float2 p)
            {
                return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
            }

            float perlinNoise(float2 uv)
            {
                float2 i = floor(uv);
                float2 f = frac(uv);

                float a = noise(i);
                float b = noise(i + float2(1.0, 0.0));
                float c = noise(i + float2(0.0, 1.0));
                float d = noise(i + float2(1.0, 1.0));

                float2 u = f * f * (3.0 - 2.0 * f);

                return lerp(a, b, u.x) + (c - a) * u.y * (1.0 - u.x) + (d - b) * u.x * u.y;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv + _Speed * _Time.y * float2(1.0, 0.5);
                float cloud = perlinNoise(uv);
                float alpha = smoothstep(_TransparencyThreshold - 0.1, _TransparencyThreshold + 0.1, cloud) * _Intensity * _Density;
                float4 color = lerp(_BaseColor, _EffectColor, alpha);
                return float4(color.rgb, alpha);
            }
            ENDCG
        }
    }
        FallBack "Transparent/Diffuse"
}