// Shader "Custom/LiquidParticle"
// {
//     Properties
//     {
//         _MainColor ("Liquid Color", Color) = (0.2, 0.6, 1.0, 1)
//     }
//     SubShader
//     {
//         Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
//         LOD 100

//         Pass
//         {
//             Name "ForwardLit"
//             Tags { "LightMode" = "UniversalForward" }

//             HLSLPROGRAM
//             #pragma vertex vert
//             #pragma fragment frag
//             #pragma multi_compile_instancing

//             #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

//             struct Attributes
//             {
//                 float4 positionOS : POSITION;
//                 float3 normalOS   : NORMAL;
//                 UNITY_VERTEX_INPUT_INSTANCE_ID
//             };

//             struct Varyings
//             {
//                 float4 positionCS : SV_POSITION;
//                 float3 normalWS   : TEXCOORD0;
//                 UNITY_VERTEX_INPUT_INSTANCE_ID
//             };

//             CBUFFER_START(UnityPerMaterial)
//                 half4 _MainColor;
//             CBUFFER_END

//             Varyings vert(Attributes input)
//             {
//                 Varyings output = (Varyings)0;
//                 UNITY_SETUP_INSTANCE_ID(input);
//                 UNITY_TRANSFER_INSTANCE_ID(input, output);

//                 // Calculate world position for instanced meshes
//                 float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
//                 output.positionCS = TransformWorldToHClip(positionWS);
                
//                 // Pass world normals to the fragment shader
//                 output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                
//                 return output;
//             }

//             half4 frag(Varyings input) : SV_Target
//             {
//                 UNITY_SETUP_INSTANCE_ID(input);
                
//                 // Normalize the normal to ensure smooth shading across the sphere
//                 float3 normal = normalize(input.normalWS);
                
//                 // Basic directional lighting for the placeholder phase
//                 float3 lightDir = normalize(float3(0.5, 1.0, 0.5));
//                 float NdotL = saturate(dot(normal, lightDir));
                
//                 half3 finalColor = _MainColor.rgb * (NdotL * 0.8 + 0.2); 
//                 return half4(finalColor, 1.0);
//             }
//             ENDHLSL
//         }
//     }
// }





Shader "Custom/LiquidParticle"
{
    Properties
    {
        _MainColor ("Liquid Color", Color) = (0.2, 0.6, 1.0, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 100

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS   : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _MainColor;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(positionWS);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                
                float3 normal = normalize(input.normalWS);
                float3 lightDir = normalize(float3(0.5, 1.0, 0.5));
                float NdotL = saturate(dot(normal, lightDir));
                
                half3 finalColor = _MainColor.rgb * (NdotL * 0.8 + 0.2); 
                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
}