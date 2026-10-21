// PopGlow.shader
// A simple unlit UI shader for the soul energy popup with a glow effect.
// It multiplies the main texture color and adds an optional outline.
Shader "UI/PopGlow"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _GlowColor ("Glow Color", Color) = (0.2, 0.9, 1, 0.5)
        _GlowSize ("Glow Size", Range(0, 1)) = 0.05
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "CanUseSpriteAtlas"="True" }
        LOD 100

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _GlowColor;
            float _GlowSize;

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
            };

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;
                // Simple glow by adding a colored border based on distance to edge (alpha threshold)
                float alpha = col.a;
                // Approximate edge detection (works best with alpha cutout textures)
                float edge = saturate(1.0 - smoothstep(0.5 - _GlowSize, 0.5 + _GlowSize, alpha));
                col.rgb += _GlowColor.rgb * edge * _GlowColor.a;
                return col;
            }
            ENDCG
        }
    }
}
