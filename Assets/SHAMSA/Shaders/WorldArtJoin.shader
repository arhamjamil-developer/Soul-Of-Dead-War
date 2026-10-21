Shader "SoulCrystalDefense/WorldArtJoin"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _JoinAtTop ("Fade top edge instead of bottom", Float) = 0
        _BlendWidth ("UV overlap", Range(0.001,0.25)) = 0.08
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "CanUseSpriteAtlas"="True" }
        Cull Off ZWrite Off Lighting Off
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            fixed4 _Color;
            float _JoinAtTop, _BlendWidth;
            struct Input { float4 vertex:POSITION; float2 uv:TEXCOORD0; fixed4 color:COLOR; };
            struct Output { float4 vertex:SV_POSITION; float2 uv:TEXCOORD0; fixed4 color:COLOR; };
            Output vert(Input v)
            {
                Output o;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.color=v.color*_Color;return o;
            }
            fixed4 frag(Output i):SV_Target
            {
                fixed4 color=tex2D(_MainTex,i.uv)*i.color;
                float edge=lerp(i.uv.y,1-i.uv.y,saturate(_JoinAtTop));
                color.a*=smoothstep(0,_BlendWidth,edge);
                return color;
            }
            ENDCG
        }
    }
}
