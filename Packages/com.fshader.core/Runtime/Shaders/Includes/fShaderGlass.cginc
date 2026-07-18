#ifndef FSHADER_GLASS_INCLUDED
#define FSHADER_GLASS_INCLUDED

sampler2D _CondensationMap;
sampler2D _CondensationNormal;
half4 _TransmissionColor;
half _GlassThickness;
half _RefractionStrength;
half _CondensationAmount;
float4 _DropletSpeed;

void FSModeModifyVertex(inout float3 positionOS, half3 normalOS, half4 vertexColor)
{
}

void FSModeModifySurface(inout FSSurfaceData surface, FSVaryings input)
{
    half vertexCondensation = 1.0h;
    half vertexThickness = 1.0h;
    #if defined(FSHADER_VERTEX_COLOR)
        vertexCondensation = input.vertexColor.r;
        vertexThickness = input.vertexColor.g;
        surface.alpha *= input.vertexColor.a;
    #endif

    half condensation = 0.0h;
    half microFog = 0.0h;
    #if defined(FSHADER_GLASS_CONDENSATION)
        float2 condensationUV = input.uv + _Time.y * _DropletSpeed.xy;
        half4 packedCondensation = tex2D(_CondensationMap, condensationUV);
        half dropletsAndTrails = saturate(packedCondensation.r + packedCondensation.g * 0.65h);
        condensation = saturate(dropletsAndTrails * _CondensationAmount * vertexCondensation);
        microFog = saturate(packedCondensation.b * _CondensationAmount);
        surface.roughness = lerp(surface.roughness, 0.88h, saturate(condensation + microFog * 0.7h));
        surface.alpha = saturate(surface.alpha + microFog * 0.22h + condensation * 0.08h);
    #endif

    #if defined(FSHADER_GLASS_DROPLET_NORMAL)
        half3 dropletNormalTS = UnpackScaleNormal(
            tex2D(_CondensationNormal, input.uv + _Time.y * _DropletSpeed.zw),
            _CondensationAmount);
        half3 dropletNormalWS = FSTangentNormalToWorld(
            dropletNormalTS,
            normalize(input.worldNormal),
            normalize(input.worldTangent),
            normalize(input.worldBitangent));
        surface.normalWS = normalize(lerp(surface.normalWS, dropletNormalWS, condensation));
    #endif

    half thickness = saturate(_GlassThickness * vertexThickness);
    surface.baseColor *= lerp(1.0h, _TransmissionColor.rgb, thickness);
    surface.modeMask = condensation;
    surface.modeDetail = microFog;
}

half3 FSModeReflectionNormal(FSVaryings input, FSSurfaceData surface)
{
    half3 geometricNormal = normalize(input.worldNormal);
    half bend = (1.0h - rcp(max(_IOR, 1.001h))) * _RefractionStrength * (0.35h + _GlassThickness);
    return normalize(lerp(geometricNormal, surface.normalWS, saturate(0.3h + bend * 4.0h)));
}

half3 FSModeModifyLighting(FSVaryings input, FSSurfaceData surface, half3 litColor)
{
    half3 viewDirection = FSWorldSpaceViewDirection(input.worldPosition);
    half fresnel = Pow5(1.0h - saturate(dot(surface.normalWS, viewDirection)));
    half transmissionWeight = saturate(1.0h - fresnel * _ReflectionStrength);
    half3 transmission = _TransmissionColor.rgb * surface.baseColor;
    transmission *= lerp(1.0h, 0.72h, surface.modeDetail);
    return lerp(transmission, litColor, saturate(0.18h + fresnel + surface.modeMask * 0.32h)) +
           transmission * transmissionWeight * 0.08h;
}

#endif
