Shader "XRLab/StereoInstanced"
{
    Properties
    {
        _Ipd ("IPD", Float) = 0.064
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"

            float _Ipd;

            struct appdata
            {
                float4 vertex : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float hue : TEXCOORD0;
            };

            float3 Hue(float h)
            {
                float3 rgb = saturate(float3(
                    abs(h * 6.0 - 3.0) - 1.0,
                    2.0 - abs(h * 6.0 - 2.0),
                    2.0 - abs(h * 6.0 - 4.0)));
                return lerp(float3(1, 1, 1), rgb, 0.55) * 0.9;
            }

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                #ifdef INSTANCING_ON
                float id = unity_InstanceID;
                #else
                float id = 0;
                #endif
                float eye = fmod(id, 2.0) < 0.5 ? 0.0 : 1.0;
                float4 world = mul(unity_ObjectToWorld, v.vertex);
                float4 view = mul(UNITY_MATRIX_V, world);
                view.x += (eye == 0.0 ? 0.5 : -0.5) * _Ipd;
                float4 clip = mul(UNITY_MATRIX_P, view);
                clip.x = clip.x * 0.5 + (eye == 0.0 ? -0.5 : 0.5) * clip.w;
                o.pos = clip;
                o.hue = frac(floor(id * 0.5) * 0.17);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return fixed4(Hue(i.hue), 1.0);
            }
            ENDCG
        }
    }
}
