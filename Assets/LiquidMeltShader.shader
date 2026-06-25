



Shader "Hidden/LiquidMeltShader"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            TEXTURE2D_X(_CameraDepthTexture);
            SAMPLER(sampler_CameraDepthTexture);

            float4 frag(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;
                float2 texelSize = _BlitTexture_TexelSize.xy;

                // ??????? ???? ??????
                float4 col = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv);
                float4 blur = col;
                blur += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2(texelSize.x * 2.5, texelSize.y * 2.5));
                blur += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv - float2(texelSize.x * 2.5, texelSize.y * 2.5));
                blur += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2(texelSize.x * 2.5, -texelSize.y * 2.5));
                blur += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv - float2(texelSize.x * 2.5, -texelSize.y * 2.5));
                blur /= 5.0;

                // ??? ????????
                float depth = SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_CameraDepthTexture, uv).r;
                float isGeometry = step(0.0001, depth); 
                float isLiquid = step(0.15, blur.b) * step(blur.r, blur.b - 0.1) * step(blur.g, blur.b) * isGeometry;

                // ???? ?????
                if (isLiquid > 0.5)
                {
                    float xDiff = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2(texelSize.x * 2.0, 0)).b - SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv - float2(texelSize.x * 2.0, 0)).b;
                    float yDiff = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2(0, texelSize.y * 2.0)).b - SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv - float2(0, texelSize.y * 2.0)).b;
                    
                    float3 normal = normalize(float3(xDiff * 40.0, yDiff * 40.0, 1.0));
                    
                    float3 lightDir = normalize(float3(0.5, 1.0, 0.5));
                    float spec = pow(max(0, dot(normal, lightDir)), 64.0) * 1.5;

                    float2 refractUV = uv + normal.xy * 0.03;
                    float4 bg = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, refractUV);

                    float3 waterColor = float3(0.1, 0.5, 0.9) * 0.6;
                    return float4(bg.rgb * 0.5 + waterColor + spec.xxx, 1.0);
                }

                return col; 
            }
            ENDHLSL
        }
    }
}
