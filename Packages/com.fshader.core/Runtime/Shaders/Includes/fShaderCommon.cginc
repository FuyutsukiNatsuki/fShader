#ifndef FSHADER_COMMON_INCLUDED
#define FSHADER_COMMON_INCLUDED

#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "AutoLight.cginc"
#include "Packages/com.fshader.core/Runtime/Shaders/Includes/fShaderPBR.cginc"
#include "Packages/com.fshader.core/Runtime/Shaders/Includes/fShaderBRPLighting.cginc"

sampler2D _BaseMap;
float4 _BaseMap_ST;
sampler2D _ARMHMap;
sampler2D _NormalMap;

half _AOStrength;
half _Roughness;
half _Metallic;
half _NormalScale;
half _Opacity;
half _ReflectionStrength;
half _IOR;
half _FSDebugView;

// VRChat overrides these globals while rendering mirrors. In normal cameras
// (desktop, VR eyes, photo camera) _VRChatMirrorMode is zero and Unity's
// per-camera/per-eye position remains authoritative.
float _VRChatMirrorMode;
float3 _VRChatMirrorCameraPos;

float3 FSWorldSpaceCameraPosition()
{
    return _VRChatMirrorMode > 0.5 ? _VRChatMirrorCameraPos : _WorldSpaceCameraPos;
}

half3 FSWorldSpaceViewDirection(float3 worldPosition)
{
    return normalize(FSWorldSpaceCameraPosition() - worldPosition);
}

struct FSAppData
{
    float4 vertex : POSITION;
    half3 normal : NORMAL;
    half4 tangent : TANGENT;
    float2 uv : TEXCOORD0;
    float2 uv1 : TEXCOORD1;
    half4 color : COLOR;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct FSVaryings
{
    float4 pos : SV_POSITION;
    float2 uv : TEXCOORD0;
    float3 worldPosition : TEXCOORD1;
    half3 worldNormal : TEXCOORD2;
    half3 worldTangent : TEXCOORD3;
    half3 worldBitangent : TEXCOORD4;
    float2 lightmapUV : TEXCOORD5;
    half3 vertexLight : TEXCOORD6;
    UNITY_FOG_COORDS(7)
    UNITY_SHADOW_COORDS(8)
    half4 vertexColor : COLOR0;
    #if defined(FSHADER_SCREEN_REFRACTION)
        float4 grabPosition : TEXCOORD9;
    #endif
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

#if defined(FSHADER_PLUS_WATER)
    #include "Packages/com.fshader.plus/Runtime/Shaders/Includes/fShaderPlusWater.cginc"
#elif defined(FSHADER_PLUS_ICE)
    #include "Packages/com.fshader.plus/Runtime/Shaders/Includes/fShaderPlusIce.cginc"
#elif defined(FSHADER_PLUS_GLASS)
    #include "Packages/com.fshader.plus/Runtime/Shaders/Includes/fShaderPlusGlass.cginc"
#elif defined(FSHADER_MODE_WATER)
    #include "Packages/com.fshader.core/Runtime/Shaders/Includes/fShaderWater.cginc"
#elif defined(FSHADER_MODE_ICE)
    #include "Packages/com.fshader.core/Runtime/Shaders/Includes/fShaderIce.cginc"
#elif defined(FSHADER_MODE_GLASS)
    #include "Packages/com.fshader.core/Runtime/Shaders/Includes/fShaderGlass.cginc"
#else
    void FSModeModifyVertex(inout float3 positionOS, half3 normalOS, half4 vertexColor)
    {
    }
    void FSModeModifySurface(inout FSSurfaceData surface, FSVaryings input)
    {
    }
    half3 FSModeModifyLighting(FSVaryings input, FSSurfaceData surface, half3 litColor)
    {
        return litColor;
    }
    half3 FSModeReflectionNormal(FSVaryings input, FSSurfaceData surface)
    {
        return surface.normalWS;
    }
#endif

#if defined(FSHADER_LTCGI)
    #include "Packages/com.fshader.plus/Runtime/Shaders/Includes/fShaderLTCGI.cginc"
#endif

UNITY_INSTANCING_BUFFER_START(FShaderPerInstance)
    UNITY_DEFINE_INSTANCED_PROP(float4, _BaseColor)
UNITY_INSTANCING_BUFFER_END(FShaderPerInstance)

FSVaryings FSVert(FSAppData input)
{
    FSVaryings output;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_INITIALIZE_OUTPUT(FSVaryings, output);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    float3 positionOS = input.vertex.xyz;
    FSModeModifyVertex(positionOS, input.normal, input.color);
    float4 modifiedVertex = float4(positionOS, 1.0);
    output.pos = UnityObjectToClipPos(modifiedVertex);
    output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
    output.worldPosition = mul(unity_ObjectToWorld, modifiedVertex).xyz;
    output.worldNormal = UnityObjectToWorldNormal(input.normal);
    output.worldTangent = UnityObjectToWorldDir(input.tangent.xyz);
    half tangentSign = input.tangent.w * unity_WorldTransformParams.w;
    output.worldBitangent = cross(output.worldNormal, output.worldTangent) * tangentSign;
    output.vertexColor = input.color;

    #if defined(LIGHTMAP_ON)
        output.lightmapUV = input.uv1 * unity_LightmapST.xy + unity_LightmapST.zw;
    #else
        output.lightmapUV = 0.0;
    #endif

    #if defined(VERTEXLIGHT_ON)
        output.vertexLight = Shade4PointLights(
            unity_4LightPosX0,
            unity_4LightPosY0,
            unity_4LightPosZ0,
            unity_LightColor[0].rgb,
            unity_LightColor[1].rgb,
            unity_LightColor[2].rgb,
            unity_LightColor[3].rgb,
            unity_4LightAtten0,
            output.worldPosition,
            normalize(output.worldNormal));
    #else
        output.vertexLight = 0.0h;
    #endif

    UNITY_TRANSFER_FOG(output, output.pos);
    UNITY_TRANSFER_SHADOW(output, input.uv1);
    #if defined(FSHADER_SCREEN_REFRACTION)
        output.grabPosition = ComputeGrabScreenPos(output.pos);
    #endif
    return output;
}

half4 FSGetBaseColor()
{
    return UNITY_ACCESS_INSTANCED_PROP(FShaderPerInstance, _BaseColor);
}

FSSurfaceData FSSampleSurface(FSVaryings input)
{
    FSSurfaceData surface;
    half4 baseSample = 1.0h;
    #if defined(FSHADER_BASEMAP)
        baseSample = tex2D(_BaseMap, input.uv);
    #endif

    half4 baseColor = FSGetBaseColor() * baseSample;
    surface.baseColor = baseColor.rgb;
    surface.alpha = saturate(baseColor.a * _Opacity);
    surface.ao = 1.0h;
    surface.roughness = saturate(_Roughness);
    surface.metallic = saturate(_Metallic);
    surface.height = 0.5h;
    surface.modeMask = 0.0h;
    surface.modeDetail = 0.0h;

    #if defined(FSHADER_MASKMAP)
        half4 armh = tex2D(_ARMHMap, input.uv);
        surface.ao = lerp(1.0h, armh.r, saturate(_AOStrength));
        surface.roughness = saturate(armh.g);
        surface.metallic = saturate(armh.b);
        surface.height = armh.a;
    #endif

    surface.roughness = max(surface.roughness, FS_MIN_ROUGHNESS);
    half3 normalWS = normalize(input.worldNormal);
    #if defined(FSHADER_NORMALMAP)
        half3 normalTS = UnpackScaleNormal(tex2D(_NormalMap, input.uv), _NormalScale);
        normalWS = FSTangentNormalToWorld(
            normalTS,
            normalWS,
            normalize(input.worldTangent),
            normalize(input.worldBitangent));
    #endif
    surface.normalWS = normalWS;
    FSModeModifySurface(surface, input);
    surface.roughness = max(saturate(surface.roughness), FS_MIN_ROUGHNESS);
    return surface;
}

half3 FSShadeSurface(FSVaryings input, FSSurfaceData surface)
{
    half3 viewDirection = FSWorldSpaceViewDirection(input.worldPosition);
    half3 lightDirection = FSMainLightDirection(input.worldPosition);
    UNITY_LIGHT_ATTENUATION(attenuation, input, input.worldPosition);

    half3 direct = FSEvaluateDirectBRDF(
        surface,
        viewDirection,
        lightDirection,
        _LightColor0.rgb) * attenuation;

    half3 bakedDiffuse = FSSampleBakedDiffuse(
        input.lightmapUV,
        surface.normalWS,
        input.vertexLight);
    half3 diffuse = bakedDiffuse * surface.baseColor *
                    (1.0h - surface.metallic) * surface.ao;

    half dielectricF0 = 0.04h;
    half3 f0 = lerp((half3)dielectricF0, surface.baseColor, surface.metallic);
    half ndotv = saturate(dot(surface.normalWS, viewDirection));
    half3 environmentFresnel = FSFresnelSchlickRoughness(
        ndotv,
        f0,
        surface.roughness);
    half specularOcclusion = lerp(1.0h, surface.ao, saturate(1.0h - surface.roughness));
    half3 reflectionNormal = FSModeReflectionNormal(input, surface);
    half3 reflectionDirection = reflect(-viewDirection, reflectionNormal);
    #if defined(FSHADER_BOX_PROJECTION)
        reflectionDirection = FSBoxProjectedReflectionDirection(reflectionDirection, input.worldPosition);
    #endif
    half3 reflection = FSSampleReflectionDirection(reflectionDirection, surface.roughness);
    reflection *= environmentFresnel * specularOcclusion * _ReflectionStrength;

    half3 litColor = FSModeModifyLighting(input, surface, direct + diffuse + reflection);
    #if defined(FSHADER_LTCGI)
        litColor += FSEvaluateLTCGI(input, surface);
    #endif
    return litColor;
}

half4 FSResolveDebug(FSSurfaceData surface, FSVaryings input)
{
    if (_FSDebugView < 1.5h) return half4(surface.baseColor, 1.0h);
    if (_FSDebugView < 2.5h) return half4(surface.ao.xxx, 1.0h);
    if (_FSDebugView < 3.5h) return half4(surface.roughness.xxx, 1.0h);
    if (_FSDebugView < 4.5h) return half4(surface.metallic.xxx, 1.0h);
    if (_FSDebugView < 5.5h) return half4(surface.height.xxx, 1.0h);
    if (_FSDebugView < 6.5h) return half4(surface.normalWS * 0.5h + 0.5h, 1.0h);
    if (_FSDebugView < 7.5h) return half4(input.vertexColor.rrr, 1.0h);
    if (_FSDebugView < 8.5h) return half4(input.vertexColor.ggg, 1.0h);
    if (_FSDebugView < 9.5h) return half4(input.vertexColor.bbb, 1.0h);
    return half4(input.vertexColor.aaa, 1.0h);
}

half4 FSFragOpaque(FSVaryings input) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
    FSSurfaceData surface = FSSampleSurface(input);

    #if defined(FSHADER_DEBUG)
        half4 debugColor = FSResolveDebug(surface, input);
        UNITY_APPLY_FOG(input.fogCoord, debugColor);
        return debugColor;
    #endif

    half4 color = half4(FSShadeSurface(input, surface), 1.0h);
    UNITY_APPLY_FOG(input.fogCoord, color);
    return color;
}

half4 FSFragTransparent(FSVaryings input) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
    FSSurfaceData surface = FSSampleSurface(input);

    #if defined(FSHADER_DEBUG)
        half4 debugColor = FSResolveDebug(surface, input);
        UNITY_APPLY_FOG(input.fogCoord, debugColor);
        return debugColor;
    #endif

    half4 color = half4(FSShadeSurface(input, surface), surface.alpha);
    color.rgb *= color.a;
    half4 premultipliedFog = half4(unity_FogColor.rgb * color.a, color.a);
    UNITY_APPLY_FOG_COLOR(input.fogCoord, color, premultipliedFog);
    return color;
}

#if defined(FSHADER_SCREEN_REFRACTION)
UNITY_DECLARE_SCREENSPACE_TEXTURE(_fShaderSharedGrab);

float2 FSRefractedScreenUV(FSVaryings input, FSSurfaceData surface, half strength, half thickness)
{
    float2 screenUV = input.grabPosition.xy / max(input.grabPosition.w, 0.0001);
    half3 normalVS = mul((half3x3)UNITY_MATRIX_V, surface.normalWS);
    float2 distortion = normalVS.xy * (strength * 0.018h) * (0.35h + thickness * 0.65h);
    #if defined(UNITY_SINGLE_PASS_STEREO)
        // ComputeGrabScreenPos already transforms into the packed eye region.
        // Scale only the post-transform distortion so it cannot cross eyes.
        distortion *= unity_StereoScaleOffset[unity_StereoEyeIndex].xy;
        screenUV += distortion;
        return UnityStereoClamp(screenUV, unity_StereoScaleOffset[unity_StereoEyeIndex]);
    #else
        screenUV += distortion;
        return saturate(screenUV);
    #endif
}

#if defined(FSHADER_SCREEN_GLASS)
half4 FSFragGlassScreen(FSVaryings input) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
    FSSurfaceData surface = FSSampleSurface(input);

    #if defined(FSHADER_DEBUG)
        half4 debugColor = FSResolveDebug(surface, input);
        UNITY_APPLY_FOG(input.fogCoord, debugColor);
        return debugColor;
    #endif

    float2 screenUV = FSRefractedScreenUV(input, surface, _RefractionStrength, _GlassThickness);
    half3 background = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_fShaderSharedGrab, screenUV).rgb * _TransmissionColor.rgb;
    half3 surfaceLighting = FSShadeSurface(input, surface);
    half surfaceWeight = saturate(surface.alpha + surface.modeMask * 0.2h);
    half4 color = half4(lerp(background, surfaceLighting, surfaceWeight), 1.0h);
    UNITY_APPLY_FOG(input.fogCoord, color);
    return color;
}
#endif

#if defined(FSHADER_SCREEN_WATER)
half4 FSFragWaterScreen(FSVaryings input) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
    FSSurfaceData surface = FSSampleSurface(input);

    #if defined(FSHADER_DEBUG)
        half4 debugColor = FSResolveDebug(surface, input);
        UNITY_APPLY_FOG(input.fogCoord, debugColor);
        return debugColor;
    #endif

    half depth = saturate(surface.height * _WaterThickness);
    float2 screenUV = FSRefractedScreenUV(input, surface, _RefractionStrength, depth);
    half3 absorptionTint = lerp(_ShallowColor.rgb, _DeepColor.rgb, depth);
    half3 background = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_fShaderSharedGrab, screenUV).rgb;
    background *= lerp(1.0h, absorptionTint, saturate(_AbsorptionStrength * depth));
    half3 surfaceLighting = FSShadeSurface(input, surface);
    half3 viewDirection = FSWorldSpaceViewDirection(input.worldPosition);
    half fresnel = Pow5(1.0h - saturate(dot(surface.normalWS, viewDirection)));
    half surfaceWeight = saturate(surface.alpha * 0.35h + fresnel * _FresnelStrength + surface.modeMask * 0.2h);
    half4 color = half4(lerp(background, surfaceLighting, surfaceWeight), 1.0h);
    UNITY_APPLY_FOG(input.fogCoord, color);
    return color;
}
#endif
#endif

#endif
