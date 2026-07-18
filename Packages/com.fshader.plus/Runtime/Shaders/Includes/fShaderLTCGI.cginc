#ifndef FSHADER_LTCGI_INCLUDED
#define FSHADER_LTCGI_INCLUDED

// Keep LTCGI as an external dependency so its controller and shader API stay aligned.
#include "Packages/at.pimaker.ltcgi/Shaders/LTCGI_structs.cginc"

half _LTCGIDiffuseStrength;
half _LTCGISpecularStrength;
half _LTCGIMaxBrightness;
#if defined(FSHADER_PLUS_GLASS)
half _LTCGICondensationDiffuse;
#endif

struct FSLTCGIAccumulator
{
    float3 diffuse;
    float3 specular;
};

void FSLTCGIAccumulateDiffuse(inout FSLTCGIAccumulator accumulator, in ltcgi_output output);
void FSLTCGIAccumulateSpecular(inout FSLTCGIAccumulator accumulator, in ltcgi_output output);

#define LTCGI_V2_CUSTOM_INPUT FSLTCGIAccumulator
#define LTCGI_V2_DIFFUSE_CALLBACK FSLTCGIAccumulateDiffuse
#define LTCGI_V2_SPECULAR_CALLBACK FSLTCGIAccumulateSpecular
#include "Packages/at.pimaker.ltcgi/Shaders/LTCGI.cginc"

void FSLTCGIAccumulateDiffuse(inout FSLTCGIAccumulator accumulator, in ltcgi_output output)
{
    accumulator.diffuse += output.intensity * output.color;
}

void FSLTCGIAccumulateSpecular(inout FSLTCGIAccumulator accumulator, in ltcgi_output output)
{
    accumulator.specular += output.intensity * output.color;
}

half3 FSEvaluateLTCGI(FSVaryings input, FSSurfaceData surface)
{
    FSLTCGIAccumulator accumulator = (FSLTCGIAccumulator)0;
    half3 viewDirection = FSWorldSpaceViewDirection(input.worldPosition);
    LTCGI_Contribution(
        accumulator,
        input.worldPosition,
        surface.normalWS,
        viewDirection,
        surface.roughness,
        input.lightmapUV);

    half diffuseStrength = _LTCGIDiffuseStrength;
    #if defined(FSHADER_PLUS_GLASS)
        diffuseStrength *= 1.0h + saturate(surface.modeDetail) * _LTCGICondensationDiffuse;
    #endif

    half3 diffuse = accumulator.diffuse * surface.baseColor *
                    (1.0h - surface.metallic) * surface.ao * diffuseStrength;
    half3 specular = accumulator.specular * _LTCGISpecularStrength;
    half3 contribution = max(diffuse + specular, 0.0h);

    // Preserve hue while bounding transparent area-light energy.
    half peak = max(max(contribution.r, contribution.g), contribution.b);
    half maximum = max(_LTCGIMaxBrightness, 0.01h);
    contribution *= min(1.0h, maximum / max(peak, 0.0001h));
    return contribution;
}

#endif
