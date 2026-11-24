Shader "XRLab/Present"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Steps ("Steps", Float) = 4
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
            float _Steps;

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
                fixed4 color = tex2D(_MainTex, i.uv);
                float acc = 0.0;
                int steps = (int)clamp(_Steps, 1.0, 64.0);
                for (int n = 0; n < 64; n++)
                {
                    if (n >= steps)
                        break;
                    float2 offset = float2(sin(n * 12.9898), cos(n * 78.233)) * 0.0015;
                    acc += tex2D(_MainTex, i.uv + offset).r;
                }
                color.rgb *= 1.0 + (acc / steps - color.r) * 0.02;
                return color;
            }
            ENDCG
        }
    }
}
