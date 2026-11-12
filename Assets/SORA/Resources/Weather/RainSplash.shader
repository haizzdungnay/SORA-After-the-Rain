Shader "SORA/Weather/RainSplash"
{
    Properties { _Splash("Splash crown", Float) = 0 }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
                float _Splash;
            CBUFFER_END
            struct Attributes { float4 positionOS:POSITION; float2 uv:TEXCOORD0; half4 color:COLOR; };
            struct Varyings { float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0; half4 color:COLOR; };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS=TransformObjectToHClip(input.positionOS.xyz);
                output.uv=input.uv;
                output.color=input.color;
                return output;
            }
            half4 Frag(Varyings input):SV_Target
            {
                float2 uv=input.uv;
                float streak=pow(saturate(1-abs(uv.x*2-1)),2)*sin(uv.y*3.141593);
                float2 p=float2((uv.x-.5)*2,uv.y);
                // Saltline's analytic splash crown, independent of a sprite atlas.
                float dome=1-smoothstep(.25,1,length(p*float2(1.15,1.5)));
                float crown=dome*(.35+.65*smoothstep(.25,.75,length(p*float2(1,1.4))));
                float alpha=lerp(streak,crown,_Splash)*input.color.a;
                clip(alpha-.003);
                return half4(input.color.rgb,alpha);
            }
            ENDHLSL
        }
    }
}
