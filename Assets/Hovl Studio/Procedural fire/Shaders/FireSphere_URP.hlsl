#ifndef FIRESPHERE_URP_INCLUDE
#define FIRESPHERE_URP_INCLUDE

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

struct FireAttributes
{
    float4 positionOS : POSITION;
    float2 uv : TEXCOORD0;
    half4 color : COLOR;
};

struct FireVaryings
{
    float4 positionCS : SV_POSITION;
    float2 uv : TEXCOORD0;
    half4 color : COLOR;
    float4 screenPos : TEXCOORD1;
};

CBUFFER_START(UnityPerMaterial)
    half4 _Color;
    float _Emission;
    float _StartFrequency;
    float _Amplitude;
    float _Frequency;
    float _Usedepth;
    float _Depthpower;
    float _Useblack;
    float _Opacity;
    float4 _MainTex_ST;
CBUFFER_END

TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);

FireVaryings FireVert(FireAttributes input)
{
    FireVaryings output;
    output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
    output.uv = TRANSFORM_TEX(input.uv, _MainTex);
    output.color = input.color;
    output.screenPos = ComputeScreenPos(output.positionCS);
    return output;
}

half4 FireFrag(FireVaryings input) : SV_Target
{
    float3 uvTex3Coord = float3(input.uv, 0.0);

    float4 emissionBase = _Emission * _Color * input.color;

    float2 tempOutput8 = (((float2(0.2, 0.0) * _Time.y) + uvTex3Coord.xy + uvTex3Coord.z) * _StartFrequency);
    float2 break18 = floor(tempOutput8);
    float tempOutput21 = break18.x + (break18.y * 57.0);
    float2 tempOutput10 = frac(tempOutput8);
    float2 break17 = tempOutput10 * tempOutput10 * (float2(3.0, 3.0) - (tempOutput10 * 2.0));
    float lerpResult39 = lerp(frac(473.5 * sin(tempOutput21)), frac(473.5 * sin(1.0 + tempOutput21)), break17.x);
    float lerpResult38 = lerp(frac(473.5 * sin(57.0 + tempOutput21)), frac(473.5 * sin(58.0 + tempOutput21)), break17.x);
    float lerpResult40 = lerp(lerpResult39, lerpResult38, break17.y);

    float3 tempOutput51 = ((float3(float2(0.5, 0.5) * _Time.y, 0.0) + (uvTex3Coord * (lerpResult40 * _Amplitude)) + uvTex3Coord.z) * _Frequency);
    float3 break87 = floor(tempOutput51);
    float tempOutput90 = break87.x + (break87.y * 57.0);
    float3 tempOutput52 = frac(tempOutput51);
    float3 break110 = tempOutput52 * tempOutput52 * (float3(3.0, 3.0, 3.0) - (tempOutput52 * 2.0));
    float lerpResult109 = lerp(frac(473.5 * sin(tempOutput90)), frac(473.5 * sin(1.0 + tempOutput90)), break110.x);
    float lerpResult105 = lerp(frac(473.5 * sin(57.0 + tempOutput90)), frac(473.5 * sin(58.0 + tempOutput90)), break110.x);
    float lerpResult106 = lerp(lerpResult109, lerpResult105, break110.y);

    float2 sampleUv = uvTex3Coord.xy + (0.2 * (lerpResult106 * _Amplitude));
    half4 texSample = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, sampleUv);

    half3 rgb = lerp(emissionBase.rgb, emissionBase.rgb * texSample.rgb, _Useblack);
    half4 clampResult132 = saturate(input.color.a * texSample * _Opacity);

    float2 screenUv = input.screenPos.xy / input.screenPos.w;
    float sceneDepth = LinearEyeDepth(SampleSceneDepth(screenUv), _ZBufferParams);
    float particleDepth = LinearEyeDepth(input.screenPos.z / input.screenPos.w, _ZBufferParams);
    float distanceDepth = abs((sceneDepth - particleDepth) / _Depthpower);
    half depthFactor = saturate(distanceDepth);

    half alpha = lerp(clampResult132.r, clampResult132.r * depthFactor, _Usedepth);
    return half4(rgb, alpha);
}

#endif
