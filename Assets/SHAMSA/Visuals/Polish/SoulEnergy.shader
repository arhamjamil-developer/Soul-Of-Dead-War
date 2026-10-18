Shader "SoulCrystal/AtmosphericEnergy"
{
 Properties { _MainTex ("Sprite", 2D) = "white" {} _Color ("Tint", Color) = (0.15,0.8,1,1) _Mode ("0 Ring / 1 Mist", Float) = 0 _Speed ("Speed", Float) = 0.35 }
 SubShader
 {
  Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "CanUseSpriteAtlas"="True" }
  Cull Off Lighting Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
  Pass
  {
   Tags { "LightMode"="Universal2D" }
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "UnityCG.cginc"
   struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; float4 color:COLOR; };
   struct v2f { float4 vertex:SV_POSITION; float2 uv:TEXCOORD0; float4 color:COLOR; };
   float4 _Color; float _Mode, _Speed;
   v2f vert(appdata v) { v2f o; o.vertex=UnityObjectToClipPos(v.vertex); o.uv=v.uv; o.color=v.color*_Color; return o; }
   fixed4 frag(v2f i):SV_Target
   {
    float2 p=i.uv*2-1; float r=length(p); float a=atan2(p.y,p.x); float t=_Time.y*_Speed;
    float ring=exp(-abs(r-.66)*32);
    float segments=.6+.4*sin(a*12+t*3);
    float orbit=exp(-abs(r-(.80+.025*sin(a*5-t*2)))*65)*(.5+.5*sin(a*8-t*4));
    float core=exp(-r*r*7)*.35;
    float energy=(ring*segments+orbit*.6+core)*(1-smoothstep(.86,1,r));
    float haze=(sin(p.x*4+p.y*3+t)*sin(p.y*7-p.x*2-t*.7)*.5+.5);
    haze=haze*haze*(1-smoothstep(.15,1,r))*.25;
    float alpha=lerp(energy,haze,saturate(_Mode));
    return fixed4(i.color.rgb,i.color.a*alpha);
   }
   ENDCG
  }
 }
}
