Shader "XRLab/GridFloor"
{
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 world : TEXCOORD0;
            };
            v2f vert(appdata_base v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.world = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }
            fixed4 frag(v2f i) : SV_Target
            {
                float2 g = abs(frac(i.world.xz) - 0.5);
                float line = 1.0 - smoothstep(0.02, 0.045, min(g.x, g.y));
                fixed3 color = lerp(fixed3(0.16, 0.13, 0.10), fixed3(0.90, 0.72, 0.40), line);
                return fixed4(color, 1.0);
            }
            ENDCG
        }
    }
}
