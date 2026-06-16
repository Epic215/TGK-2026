#ifndef GAP_URP_PARTICLES_INCLUDE
#define GAP_URP_PARTICLES_INCLUDE

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

struct GAPAttributes
{
    float4 positionOS : POSITION;
    float2 uv : TEXCOORD0;
    half4 color : COLOR;
};

struct GAPVaryings
{
    float4 positionCS : SV_POSITION;
    float2 uv : TEXCOORD0;
    half4 color : COLOR;
    float4 screenPos : TEXCOORD1;
};

GAPVaryings GAPVert(GAPAttributes input)
{
    GAPVaryings output;
    output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
    output.uv = input.uv;
    output.color = input.color;
    output.screenPos = ComputeScreenPos(output.positionCS);
    return output;
}

float2 GAPTransformTex(float2 uv, float4 st)
{
    return uv * st.xy + st.zw;
}

float GAPSoftFactor(float4 screenPos, float edgeSoftness)
{
    if (edgeSoftness < 0.001)
        return 1.0;

    float2 uv = UnityStereoTransformScreenSpaceTex(screenPos.xy / screenPos.w);
    float rawDepth = SampleSceneDepth(uv);
    float sceneZ = LinearEyeDepth(rawDepth, _ZBufferParams);
    float particleZ = LinearEyeDepth(screenPos.z / screenPos.w, _ZBufferParams);
    float soft = saturate((sceneZ - particleZ) / edgeSoftness);

    // When URP depth texture is disabled, depth reads are unreliable — keep particles visible.
    if (soft <= 0.001 && particleZ > 0.001)
        return 1.0;

    return soft;
}

float GAPFaceClamp(float facing, float doubleSided)
{
    return clamp(facing >= 0 ? 1.0 : 0.0, doubleSided, 1.0);
}

half4 GAPSampleMobileScroll(
    GAPVaryings input,
    half4 tintColor,
    float4 mainTexTiling,
    float4 mainTexSpeed,
    float distortionAmount,
    float4 distortionTiling,
    float4 distortionSpeed,
    float dissolveAmount,
    float4 dissolveTiling,
    float4 dissolveSpeed,
    TEXTURE2D_PARAM(maskTex, maskSampler),
    TEXTURE2D_PARAM(mainTex, mainSampler),
    TEXTURE2D_PARAM(distortionTex, distortionSampler),
    TEXTURE2D_PARAM(dissolveTex, dissolveSampler))
{
    float2 uv = input.uv;
    float2 uvMain = uv * (mainTexTiling.xy - float2(1.0, 1.0));
    float2 pannerMain = _Time.y * mainTexSpeed.xy + uvMain;
    float2 pannerDistortion = _Time.y * distortionSpeed.xy + uv * distortionTiling.xy;
    half4 distortionSample = SAMPLE_TEXTURE2D(distortionTex, distortionSampler, pannerDistortion);
    half4 distortionLerp = lerp(half4(uv, 0.0, 0.0), distortionSample, distortionAmount);
    float2 pannerDissolve = _Time.y * dissolveSpeed.xy + uv * dissolveTiling.xy;
    float2 mainUv = float2(pannerMain.x, pannerMain.y) + distortionLerp.xy;
    float2 dissolveUv = distortionLerp.xy + pannerDissolve;
    half4 maskSample = SAMPLE_TEXTURE2D(maskTex, maskSampler, uv);
    half mainAlpha = SAMPLE_TEXTURE2D(mainTex, mainSampler, mainUv).a;
    half4 dissolveSample = SAMPLE_TEXTURE2D(dissolveTex, dissolveSampler, dissolveUv);
    half4 dissolvePow = pow(max(dissolveSample, half4(0.0001, 0.0001, 0.0001, 0.0001)), dissolveAmount.xxxx);
    return input.color * (tintColor * (maskSample * (mainAlpha * dissolvePow)));
}

half4 GAPDistortionScrollAlpha(
    GAPVaryings input,
    half4 tintColor,
    float colorMultiplier,
    float mainTexUSpeed,
    float mainTexVSpeed,
    float distortMainTexture,
    float gradientPower,
    float gradientUSpeed,
    float gradientVSpeed,
    float noiseAmount,
    float distortionUSpeed,
    float distortionVSpeed,
    float edgeSoftness,
    float doubleSided,
    float facing,
    float4 mainTexST,
    float4 gradientST,
    float4 distortionST,
    float4 mainMaskST,
    TEXTURE2D_PARAM(mainTex, mainSampler),
    TEXTURE2D_PARAM(gradientTex, gradientSampler),
    TEXTURE2D_PARAM(distortionTex, distortionSampler),
    TEXTURE2D_PARAM(mainMaskTex, mainMaskSampler))
{
    float2 uv = input.uv;
    float2 distortionUv = GAPTransformTex(uv + _Time.y * float2(distortionUSpeed, distortionVSpeed), distortionST);
    half4 distortionSample = SAMPLE_TEXTURE2D(distortionTex, distortionSampler, distortionUv);
    float2 noiseUv = lerp(uv, distortionSample.rr, noiseAmount);
    float2 mainUv = GAPTransformTex(_Time.y * float2(mainTexUSpeed, mainTexVSpeed) + lerp(uv, noiseUv, distortMainTexture), mainTexST);
    half4 mainSample = SAMPLE_TEXTURE2D(mainTex, mainSampler, mainUv);
    float2 gradientUv = GAPTransformTex(noiseUv + _Time.y * float2(gradientUSpeed, gradientVSpeed), gradientST);
    half4 gradientSample = SAMPLE_TEXTURE2D(gradientTex, gradientSampler, gradientUv);
    half4 maskSample = SAMPLE_TEXTURE2D(mainMaskTex, mainMaskSampler, GAPTransformTex(uv, mainMaskST));
    half3 gradientTerm = gradientSample.rgb * pow(max(gradientSample.rgb, half3(0.0001, 0.0001, 0.0001)), gradientPower);
    half face = GAPFaceClamp(facing, doubleSided);
    half3 rgbTerm = mainSample.rgb * input.color.rgb * (tintColor.rgb * colorMultiplier) * 2.0h
        * (mainSample.a * gradientTerm * maskSample.a * face);
    half soft = GAPSoftFactor(input.screenPos, edgeSoftness);
    half3 rgb = rgbTerm * soft + rgbTerm;
    half alpha = input.color.a * tintColor.a * mainSample.a * maskSample.a * soft;
    return half4(rgb, alpha);
}

half3 GAPDistortionScrollAdditive(
    GAPVaryings input,
    half4 tintColor,
    float colorMultiplier,
    float mainTexUSpeed,
    float mainTexVSpeed,
    float distortMainTexture,
    float gradientPower,
    float gradientUSpeed,
    float gradientVSpeed,
    float noiseAmount,
    float distortionUSpeed,
    float distortionVSpeed,
    float edgeSoftness,
    float doubleSided,
    float facing,
    float4 mainTexST,
    float4 gradientST,
    float4 distortionST,
    float4 maskST,
    float4 colorRampST,
    TEXTURE2D_PARAM(mainTex, mainSampler),
    TEXTURE2D_PARAM(gradientTex, gradientSampler),
    TEXTURE2D_PARAM(distortionTex, distortionSampler),
    TEXTURE2D_PARAM(maskTex, maskSampler),
    TEXTURE2D_PARAM(colorRampTex, colorRampSampler))
{
    float2 uv = input.uv;
    half4 maskCopy = SAMPLE_TEXTURE2D(maskTex, maskSampler, GAPTransformTex(uv, maskST));
    float2 distortionUv = GAPTransformTex(uv + _Time.y * float2(distortionUSpeed, distortionVSpeed), distortionST);
    half4 distortionSample = SAMPLE_TEXTURE2D(distortionTex, distortionSampler, distortionUv);
    half4 distortionMask = SAMPLE_TEXTURE2D(maskTex, maskSampler, distortionUv);
    float2 noiseUv = lerp(uv, lerp(uv, distortionSample.rr, noiseAmount), distortionMask.r);
    float2 mainUv = GAPTransformTex(_Time.y * float2(mainTexUSpeed, mainTexVSpeed) + lerp(uv, noiseUv, distortMainTexture), mainTexST);
    half4 mainSample = SAMPLE_TEXTURE2D(mainTex, mainSampler, mainUv);
    half4 colorRamp = SAMPLE_TEXTURE2D(colorRampTex, colorRampSampler, GAPTransformTex(uv, colorRampST));
    float2 gradientUv = GAPTransformTex(noiseUv + _Time.y * float2(gradientUSpeed, gradientVSpeed), gradientST);
    half4 gradientSample = SAMPLE_TEXTURE2D(gradientTex, gradientSampler, gradientUv);
    half face = GAPFaceClamp(facing, doubleSided);
    half3 rgb = (maskCopy.rgb * mainSample.rgb) * input.color.rgb
        * (colorMultiplier * tintColor.rgb * colorRamp.rgb) * 2.0h
        * (mainSample.a * pow(max(gradientSample.r, 0.0001h), gradientPower) * face);
    return rgb * GAPSoftFactor(input.screenPos, edgeSoftness);
}

#endif
