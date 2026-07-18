#ifndef FSHADER_PBR_INCLUDED
#define FSHADER_PBR_INCLUDED

#include "UnityCG.cginc"
#include "UnityStandardUtils.cginc"

#define FS_PI 3.14159265h
#define FS_MIN_ROUGHNESS 0.04h

struct FSSurfaceData
{
    half3 baseColor;
    half3 normalWS;
    half ao;
    half roughness;
    half metallic;
    half height;
    half alpha;
    half modeMask;
    half modeDetail;
};

half3 FSFresnelSchlick(half cosTheta, half3 f0)
{
    half factor = Pow5(1.0h - saturate(cosTheta));
    return f0 + (1.0h - f0) * factor;
}

half3 FSFresnelSchlickRoughness(half cosTheta, half3 f0, half roughness)
{
    half3 grazing = max((half3)(1.0h - roughness), f0);
    return f0 + (grazing - f0) * Pow5(1.0h - saturate(cosTheta));
}

half FSGGXDistribution(half ndoth, half roughness)
{
    half alpha = max(roughness * roughness, 0.002h);
    half alpha2 = alpha * alpha;
    half denom = ndoth * ndoth * (alpha2 - 1.0h) + 1.0h;
    return alpha2 / max(FS_PI * denom * denom, 0.0001h);
}

half FSSchlickGeometry(half ndotx, half roughness)
{
    half k = roughness + 1.0h;
    k = (k * k) * 0.125h;
    return ndotx / max(ndotx * (1.0h - k) + k, 0.0001h);
}

half3 FSEvaluateDirectBRDF(
    FSSurfaceData surface,
    half3 viewDirection,
    half3 lightDirection,
    half3 lightColor)
{
    half3 halfDirection = normalize(viewDirection + lightDirection);
    half ndotl = saturate(dot(surface.normalWS, lightDirection));
    half ndotv = saturate(dot(surface.normalWS, viewDirection));
    half ndoth = saturate(dot(surface.normalWS, halfDirection));
    half vdoth = saturate(dot(viewDirection, halfDirection));

    half dielectricF0 = 0.04h;
    half3 f0 = lerp((half3)dielectricF0, surface.baseColor, surface.metallic);
    half3 fresnel = FSFresnelSchlick(vdoth, f0);
    half distribution = FSGGXDistribution(ndoth, surface.roughness);
    half geometry = FSSchlickGeometry(ndotv, surface.roughness) *
                    FSSchlickGeometry(ndotl, surface.roughness);
    half3 specular = distribution * geometry * fresnel /
                     max(4.0h * ndotv * ndotl, 0.0001h);

    half3 diffuse = surface.baseColor * (1.0h - surface.metallic) / FS_PI;
    diffuse *= (1.0h - fresnel);
    return (diffuse + specular) * lightColor * ndotl;
}

half3 FSTangentNormalToWorld(
    half3 normalTS,
    half3 normalWS,
    half3 tangentWS,
    half3 bitangentWS)
{
    half3x3 tangentToWorld = half3x3(tangentWS, bitangentWS, normalWS);
    return normalize(mul(normalTS, tangentToWorld));
}

#endif
