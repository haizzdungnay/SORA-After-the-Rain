Shader "SORA/Weather/Puddle"
{
    Properties
    {
        _DetailMap("Saltline surface noise", 2D) = "gray" {}
        [Normal] _DetailNormal("Saltline surface normal", 2D) = "bump" {}
        _WaterColor("Water tint", Color) = (.065,.085,.105,1)
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent-50" }
        Pass
        {
            Name "WetGround"
            Tags { "LightMode"="UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Offset -1, -1
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #pragma multi_compile _ _REFLECTION_PROBE_BLENDING
            #pragma multi_compile _ _REFLECTION_PROBE_BOX_PROJECTION
            #pragma multi_compile_fog
            #define _SURFACE_TYPE_TRANSPARENT 1
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            TEXTURE2D(_DetailMap); SAMPLER(sampler_DetailMap);
            TEXTURE2D(_DetailNormal); SAMPLER(sampler_DetailNormal);
            CBUFFER_START(UnityPerMaterial)
                half4 _WaterColor;
            CBUFFER_END
            float _SoraRain, _SoraWetness, _SoraPuddles;
            float4 _SoraWetCenter;
            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings { float4 positionCS : SV_POSITION; float3 positionWS : TEXCOORD0; half fog : TEXCOORD1; };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                output.fog = ComputeFogFactor(output.positionCS.z);
                return output;
            }
            float Hash(float2 p) { return frac(sin(dot(p, float2(127.1,311.7))) * 43758.5453); }
            float2 Ripple(float2 p)
            {
                float2 grid = floor(p), result = 0;
                [unroll] for (int y = -1; y <= 1; y++)
                [unroll] for (int x = -1; x <= 1; x++)
                {
                    float2 cell = grid + float2(x,y);
                    float seed = Hash(cell);
                    float cycle = _Time.y * lerp(.7, 1.2, seed) + seed * 5;
                    float life = frac(cycle);
                    float eventSeed = Hash(cell + floor(cycle) * .37);
                    float2 delta = p - cell - float2(eventSeed * .8 + .1, Hash(cell + 19 + floor(cycle)) * .8 + .1);
                    float distance = length(delta);
                    float ring = distance - life * .85;
                    float wave = sin(ring * 70) * exp(-abs(ring) * 32) * (1-life) * smoothstep(0,.12,life);
                    result += delta / max(distance,.001) * wave * step(seed, _SoraRain);
                }
                return result;
            }
            half4 Frag(Varyings input) : SV_Target
            {
                float2 world = input.positionWS.xz;
                float noise = SAMPLE_TEXTURE2D(_DetailMap, sampler_DetailMap, world * .065).r * .7;
                noise += SAMPLE_TEXTURE2D(_DetailMap, sampler_DetailMap, world * .19 + .37).r * .3;
                // Keep the .67 midpoint aligned with the footstep query; soften only the visual boundary.
                float pool = smoothstep(.45 - _SoraPuddles * .18, .89 - _SoraPuddles * .18, noise);
                pool *= smoothstep(.02,.16,_SoraPuddles);
                float edge = 1-smoothstep(_SoraWetCenter.z - 3, _SoraWetCenter.z, length(world - _SoraWetCenter.xy));
                float alpha = (.24 * _SoraWetness + .16 * pool) * edge;
                clip(alpha-.002);
                half3 normalTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_DetailNormal, sampler_DetailNormal, world * .65), .035 * (1-pool));
                half3 flow = UnpackNormalScale(SAMPLE_TEXTURE2D(_DetailNormal, sampler_DetailNormal,
                    world * float2(.45, 2.4) + float2(0, _Time.y * .08)), .012 * _SoraRain);
                half3 normalWS = normalize(half3(normalTS.x, normalTS.z, normalTS.y));
                normalWS.xz += flow.xy + Ripple(world * 3) * (.022 * pool * _SoraRain);
                normalWS = normalize(normalWS);
                InputData lighting = (InputData)0;
                lighting.positionWS = input.positionWS;
                lighting.normalWS = normalWS;
                lighting.viewDirectionWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                lighting.shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                lighting.bakedGI = SampleSH(normalWS);
                lighting.vertexLighting = VertexLighting(input.positionWS, normalWS);
                lighting.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
                lighting.shadowMask = half4(1,1,1,1);
                SurfaceData surface = (SurfaceData)0;
                surface.albedo = _WaterColor.rgb;
                surface.metallic = 0;
                surface.smoothness = lerp(.86,.97,pool);
                surface.normalTS = half3(0,0,1);
                surface.occlusion = 1;
                surface.alpha = 1;
                half4 color = UniversalFragmentPBR(lighting,surface);
                color.rgb = MixFog(color.rgb,input.fog);
                color.a = alpha;
                return color;
            }
            ENDHLSL
        }
    }
}
