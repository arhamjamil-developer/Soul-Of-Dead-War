Shader "Custom/2D/SpriteOutlineFlash"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        
        [Header(Outline Settings)]
        [Toggle] _OutlineEnabled ("Outline Enabled", Float) = 0
        [HDR] _OutlineColor ("Outline Color", Color) = (1,0.2,0.2,1)
        _OutlineThickness ("Outline Thickness", Range(0, 10)) = 2.0
        
        [Header(Hit Flash Settings)]
        [HDR] _FlashColor ("Flash Color", Color) = (1,1,1,1)
        _FlashAmount ("Flash Amount", Range(0, 1)) = 0.0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
            "RenderPipeline"="UniversalPipeline"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "SpriteOutlinePass"
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma multi_compile _ PIXELSNAP_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_TexelSize;

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _OutlineColor;
                float4 _FlashColor;
                float _OutlineThickness;
                float _OutlineEnabled;
                float _FlashAmount;
            CBUFFER_END

            CBUFFER_START(PerDrawSprite)
                float4 _RendererColor;
                float4 _Flip;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.color = input.color * _Color * _RendererColor;

                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                float4 baseCol = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * input.color;

                // Hit flash blending
                float3 flashRgb = lerp(baseCol.rgb, _FlashColor.rgb, _FlashAmount);
                float4 finalColor = float4(flashRgb, baseCol.a);

                if (_OutlineEnabled > 0.5)
                {
                    // 8-directional alpha sampling
                    float2 texel = _MainTex_TexelSize.xy * _OutlineThickness;
                    float outlineAlpha = 0.0;

                    outlineAlpha += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv + float2(texel.x, 0)).a;
                    outlineAlpha += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv + float2(-texel.x, 0)).a;
                    outlineAlpha += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv + float2(0, texel.y)).a;
                    outlineAlpha += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv + float2(0, -texel.y)).a;
                    outlineAlpha += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv + float2(texel.x, texel.y)).a;
                    outlineAlpha += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv + float2(-texel.x, texel.y)).a;
                    outlineAlpha += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv + float2(texel.x, -texel.y)).a;
                    outlineAlpha += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv + float2(-texel.x, -texel.y)).a;

                    float isEdge = step(0.01, outlineAlpha) * (1.0 - step(0.5, baseCol.a));
                    finalColor = lerp(finalColor, _OutlineColor, isEdge);
                    finalColor.a = max(finalColor.a, isEdge * _OutlineColor.a);
                }

                return finalColor;
            }
            ENDHLSL
        }
    }
    FallBack "Sprites/Default"
}
