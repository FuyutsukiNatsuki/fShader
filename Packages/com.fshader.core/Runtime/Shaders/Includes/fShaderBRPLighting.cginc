#ifndef FSHADER_BRP_LIGHTING_INCLUDED
#define FSHADER_BRP_LIGHTING_INCLUDED

#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityStandardUtils.cginc"

half3 FSSampleBakedDiffuse(float2 lightmapUV, half3 normalWS, half3 vertexLight)
{
    half3 indirectDiffuse;
    #if defined(LIGHTMAP_ON)
        half4 encodedLightmap = UNITY_SAMPLE_TEX2D(unity_Lightmap, lightmapUV);
        indirectDiffuse = DecodeLightmap(encodedLightmap);
    #else
        indirectDiffuse = max((half3)0.0h, ShadeSH9(half4(normalWS, 1.0h)));
    #endif
    return indirectDiffuse + vertexLight;
}

half3 FSSampleReflectionDirection(half3 reflectionDirection, half roughness)
{
    half mip = saturate(roughness) * UNITY_SPECCUBE_LOD_STEPS;
    half4 encoded = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, reflectionDirection, mip);
    return DecodeHDR(encoded, unity_SpecCube0_HDR);
}

half3 FSBoxProjectedReflectionDirection(half3 reflectionDirection, float3 worldPosition)
{
    return BoxProjectedCubemapDirection(
        reflectionDirection,
        worldPosition,
        unity_SpecCube0_ProbePosition,
        unity_SpecCube0_BoxMin,
        unity_SpecCube0_BoxMax);
}

half3 FSSampleReflectionProbe(half3 normalWS, half3 viewDirection, half roughness)
{
    return FSSampleReflectionDirection(reflect(-viewDirection, normalWS), roughness);
}

half3 FSMainLightDirection(float3 worldPosition)
{
    return normalize(UnityWorldSpaceLightDir(worldPosition));
}

#endif
