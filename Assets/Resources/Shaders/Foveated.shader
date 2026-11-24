Shader "XRLab/Foveated"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Gaze ("Gaze", Vector) = (0.5, 0.5, 0, 0)
        _Radius ("Radius", Float) = 0.28
        _Block ("Block", Float) = 16
    }
    SubShader
    {
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            float4 _Gaze;
            float _Radius;
            float _Block;

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata_img v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float d = distance(uv, _Gaze.xy);
                float2 sampleUv = uv;
                if (d > _Radius)
                {
                    float2 block = max(_Block, 1.0) / _ScreenParams.xy;
                    sampleUv = (floor(uv / block) + 0.5) * block;
                }
                fixed3 color = tex2D(_MainTex, sampleUv).rgb;
                if (abs(d - _Radius) < 0.004)
                    color = fixed3(0.95, 0.86, 0.55);
                return fixed4(color, 1.0);
            }
            ENDCG
        }
    }
}
