Shader "XRLab/Distortion"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _K1 ("K1", Float) = 0.25
        _Chroma ("Chroma", Float) = 0.03
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
            float _K1;
            float _Chroma;

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

            float2 Distort(float2 uv, float k)
            {
                float2 p = uv * 2.0 - 1.0;
                float aspect = _ScreenParams.x / max(_ScreenParams.y, 1.0);
                p.x *= aspect;
                p *= 1.0 + k * dot(p, p);
                p.x /= aspect;
                return p * 0.5 + 0.5;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 r = Distort(i.uv, _K1 + _Chroma);
                float2 g = Distort(i.uv, _K1);
                float2 b = Distort(i.uv, _K1 - _Chroma);
                fixed3 color = fixed3(tex2D(_MainTex, r).r, tex2D(_MainTex, g).g, tex2D(_MainTex, b).b);
                if (g.x < 0.0 || g.x > 1.0 || g.y < 0.0 || g.y > 1.0)
                    color = fixed3(0.02, 0.018, 0.016);
                return fixed4(color, 1.0);
            }
            ENDCG
        }
    }
}
