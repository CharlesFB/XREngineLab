Shader "XRLab/Reprojection"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Lag ("Lag", Float) = 0.35
        _Shift ("Shift", Float) = 0.18
        _Warp ("Warp", Float) = 1
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
            float _Lag;
            float _Shift;
            float _Warp;

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
                bool left = i.uv.x < 0.5;
                float2 p = left ? float2(i.uv.x * 2.0, i.uv.y) : float2((i.uv.x - 0.5) * 2.0, i.uv.y);
                if (!left && _Warp > 0.5)
                {
                    float amount = _Lag * _Shift * (1.1 + (1.0 - p.y) * 1.6);
                    p.x -= amount;
                }
                fixed3 color = tex2D(_MainTex, p).rgb;
                if (p.x < 0.0 || p.x > 1.0 || p.y < 0.0 || p.y > 1.0)
                    color = fixed3(0.02, 0.018, 0.016);
                if (abs(i.uv.x - 0.5) < 0.002)
                    color = fixed3(0.95, 0.9, 0.8);
                return fixed4(color, 1.0);
            }
            ENDCG
        }
    }
}
