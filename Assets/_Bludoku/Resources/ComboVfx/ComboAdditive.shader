Shader "Bludoku/Combo Additive"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1, 1, 1, 1)
        _FireWhiten ("Fire yellow-white remap", Range(0, 1)) = 0
        _SrcBlend ("Source Blend", Float) = 5
        _DstBlend ("Destination Blend", Float) = 1
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest Always
        Blend [_SrcBlend] [_DstBlend]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            fixed _FireWhiten;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 sampleColor = tex2D(_MainTex, i.uv);
                fixed brightness = max(sampleColor.r, max(sampleColor.g, sampleColor.b));
                fixed hot = saturate((brightness - 0.35) * 2.0);
                fixed3 yellowWhite = fixed3(brightness,
                    brightness * (0.82 + 0.18 * hot),
                    brightness * (0.20 + 0.70 * hot));
                sampleColor.rgb = lerp(sampleColor.rgb, yellowWhite, _FireWhiten);
                return sampleColor * i.color;
            }
            ENDCG
        }
    }
}
