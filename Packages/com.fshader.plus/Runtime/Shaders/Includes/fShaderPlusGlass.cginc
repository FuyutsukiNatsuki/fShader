#ifndef FSHADER_PLUS_GLASS_INCLUDED
#define FSHADER_PLUS_GLASS_INCLUDED

sampler2D _CondensationMap;
float4 _CondensationMap_ST;
sampler2D _CondensationNormal;
float4 _CondensationNormal_ST;
half4 _TransmissionColor;
half4 _CondensationColor;
half4 _AbsorptionColor;
half _GlassThickness;
half _RefractionStrength;
half _CondensationAmount;
half _DropletStrength;
half _TrailStrength;
half _MicroFogStrength;
half _CondensationRoughness;
half _CondensationOpacity;
half _CondensationNormalScale;
half _CondensationFadeDistance;
half _AbsorptionStrength;
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

    half droplets = 0.0h;
    half trails = 0.0h;
    half microFog = 0.0h;
    half condensation = 0.0h;
    #if defined(FSHADER_GLASS_CONDENSATION)
        float2 condensationUV = input.uv * _CondensationMap_ST.xy + _CondensationMap_ST.zw;
        half4 packedA = tex2D(_CondensationMap, condensationUV + _Time.y * _DropletSpeed.xy);
        half4 packedB = tex2D(_CondensationMap, condensationUV * 2.17h + _Time.y * _DropletSpeed.zw + 0.31h);
        half distanceFade = saturate(1.0h - distance(FSWorldSpaceCameraPosition(), input.worldPosition) / max(_CondensationFadeDistance, 0.1h));
        droplets = saturate(packedA.r * _DropletStrength);
        trails = saturate((packedA.g + packedB.g * 0.35h) * _TrailStrength) * distanceFade;
        microFog = saturate((packedA.b * 0.72h + packedB.b * 0.28h) * _MicroFogStrength);
        condensation = saturate((droplets + trails + microFog) * _CondensationAmount * vertexCondensation);
        half localRoughness = saturate(droplets * 0.45h + trails * 0.7h + microFog);
        surface.roughness = lerp(surface.roughness, _CondensationRoughness, localRoughness);
        surface.alpha = saturate(surface.alpha + condensation * _CondensationOpacity);
        surface.baseColor = lerp(surface.baseColor, _CondensationColor.rgb, condensation * _CondensationColor.a);
    #endif

    #if defined(FSHADER_GLASS_DROPLET_NORMAL)
        float2 normalUV = input.uv * _CondensationNormal_ST.xy + _CondensationNormal_ST.zw;
        half3 dropletNormalTS = UnpackScaleNormal(
            tex2D(_CondensationNormal, normalUV + _Time.y * _DropletSpeed.xy),
            _CondensationNormalScale);
        half3 dropletNormalWS = FSTangentNormalToWorld(
            dropletNormalTS,
            normalize(input.worldNormal),
            normalize(input.worldTangent),
            normalize(input.worldBitangent));
        surface.normalWS = normalize(lerp(surface.normalWS, dropletNormalWS, saturate(droplets + trails)));
    #endif

    half thicknessSource = saturate(surface.height * vertexThickness);
    half thickness = saturate(_GlassThickness * lerp(0.5h, 1.5h, thicknessSource));
    half3 absorption = lerp(1.0h, _AbsorptionColor.rgb, saturate(thickness * _AbsorptionStrength));
    surface.baseColor *= _TransmissionColor.rgb * absorption;
    surface.height = thickness;
    surface.modeMask = saturate(droplets + trails);
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
    half3 transmission = _TransmissionColor.rgb * surface.baseColor;
    transmission *= lerp(1.0h, 0.68h, surface.modeDetail);
    half surfaceWeight = saturate(0.16h + fresnel + surface.modeMask * 0.38h);
    return lerp(transmission, litColor, surfaceWeight) + transmission * (1.0h - fresnel) * 0.07h;
}

#endif
