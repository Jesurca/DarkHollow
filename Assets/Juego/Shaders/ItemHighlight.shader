Shader "DarkHollow/ItemHighlight"
{
    Properties
    {
        _Color ("Aura Color", Color) = (0.25, 0.85, 1, 0.5)
        _RimPower ("Rim Width", Range(0.5, 8)) = 2.2
        _AuraStrength ("Aura Strength", Range(0, 2)) = 1.25
        _EmissionIntensity ("Emission Intensity", Range(0, 6)) = 2.5
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "ItemSurfaceAura"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha One
            ZWrite Off
            ZTest LEqual
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half _RimPower;
                half _AuraStrength;
                half _EmissionIntensity;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float3 viewDirWS : TEXCOORD1;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);

                output.positionHCS = positionInputs.positionCS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.viewDirWS = GetWorldSpaceViewDir(positionInputs.positionWS);

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half3 normalWS = normalize(input.normalWS);
                half3 viewDirWS = normalize(input.viewDirWS);
                half rim = pow(1.0h - saturate(dot(normalWS, viewDirWS)), _RimPower);
                half aura = saturate((rim + 0.08h) * _AuraStrength);
                half3 emission = _Color.rgb * _EmissionIntensity;

                return half4(emission, _Color.a * aura);
            }
            ENDHLSL
        }
    }
}
