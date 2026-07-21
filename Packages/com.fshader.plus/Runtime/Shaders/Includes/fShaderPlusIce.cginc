#ifndef FSHADER_PLUS_ICE_INCLUDED
#define FSHADER_PLUS_ICE_INCLUDED

sampler2D _FrostMap;
float4 _FrostMap_ST;
sampler2D _CrackMap;
float4 _CrackMap_ST;
half4 _IceColor;
half4 _AbsorptionColor;
half4 _FrostColor;
half4 _CrackGlowColor;
half4 _BackLightColor;
half _IceThickness;
half _AbsorptionStrength;
half _FrostStrength;
half _FrostScaleA;
half _FrostScaleB;
half _FrostEdge;
half _CrackDepth;
half _CrackParallax;
half _CrackGlowStrength;
half _BackLightStrength;
half _BackLightThickness;
half _SparkleStrength;
half _SparkleDensity;
half _SparkleSize;
half _SparkleDistance;
half _RefractionStrength;

half FSPlusIceHash(float3 value)
{
    value = frac(value * 0.1031);
    value += dot(value, value.yzx + 33.33);
    return frac((value.x + value.y) * value.z);
}

void FSModeModifyVertex(inout float3 positionOS, half3 normalOS, half4 vertexColor)
{
}

void FSModeModifySurface(inout FSSurfaceData surface, FSVaryings input)
{
    half frost = 0.0h;
    #if defined(FSHADER_ICE_FROST)
        float2 frostUV = input.uv * _FrostMap_ST.xy + _FrostMap_ST.zw;
        half frostA = tex2D(_FrostMap, frostUV * _FrostScaleA).r;
        half frostB = tex2D(_FrostMap, frostUV.yx * _FrostScaleB + 0.37h).r;
        half edge = Pow5(1.0h - saturate(dot(normalize(input.worldNormal), FSWorldSpaceViewDirection(input.worldPosition))));
        frost = saturate((frostA * 0.68h + frostB * 0.32h + edge * _FrostEdge) * _FrostStrength);
        #if defined(FSHADER_VERTEX_COLOR)
            frost *= input.vertexColor.r;
        #endif
        surface.roughness = lerp(surface.roughness, 0.97h, frost);
        surface.baseColor = lerp(surface.baseColor, _FrostColor.rgb, frost * _FrostColor.a);
    #endif

    half cracks = 0.0h;
    #if defined(FSHADER_ICE_CRACKS)
        half3 viewDirection = FSWorldSpaceViewDirection(input.worldPosition);
        half3 viewTS = half3(
            dot(viewDirection, normalize(input.worldTangent)),
            dot(viewDirection, normalize(input.worldBitangent)),
            dot(viewDirection, normalize(input.worldNormal)));
        float2 crackUV = input.uv * _CrackMap_ST.xy + _CrackMap_ST.zw;
        crackUV += viewTS.xy / max(abs(viewTS.z), 0.2h) * _CrackParallax * (surface.height - 0.5h);
        cracks = saturate(tex2D(_CrackMap, crackUV).r * _CrackDepth);
        #if defined(FSHADER_VERTEX_COLOR)
            cracks *= input.vertexColor.g;
        #endif
        surface.baseColor *= lerp(1.0h, 0.48h, cracks);
    #endif

    half thickness = saturate(_IceThickness * lerp(0.55h, 1.0h, surface.height));
    #if defined(FSHADER_VERTEX_COLOR)
        thickness *= input.vertexColor.a;
    #endif
    half3 viewDirectionWS = FSWorldSpaceViewDirection(input.worldPosition);
    half viewThickness = thickness / max(abs(dot(normalize(input.worldNormal), viewDirectionWS)), 0.22h);
    half absorption = saturate(viewThickness * _AbsorptionStrength);
    surface.baseColor *= lerp(_IceColor.rgb, _AbsorptionColor.rgb, absorption);
    surface.modeMask = frost;
    surface.modeDetail = cracks;
    #if defined(FSHADER_ICE_TRANSPARENT)
        surface.alpha = saturate(surface.alpha + frost * _FrostColor.a * 0.42h + cracks * 0.18h + absorption * 0.12h);
    #endif
}

half3 FSModeReflectionNormal(FSVaryings input, FSSurfaceData surface)
{
    return surface.normalWS;
}

half3 FSModeModifyLighting(FSVaryings input, FSSurfaceData surface, half3 litColor)
{
    half3 viewDirection = FSWorldSpaceViewDirection(input.worldPosition);
    half3 lightDirection = FSMainLightDirection(input.worldPosition);

    #if defined(FSHADER_ICE_BACKLIGHT)
        half backFacing = saturate(dot(-surface.normalWS, lightDirection));
        half rim = Pow5(1.0h - saturate(dot(surface.normalWS, viewDirection)));
        half thickness = saturate(_BackLightThickness * (0.35h + surface.height));
        half scatter = saturate(backFacing * 0.65h + rim) * thickness * (1.0h - surface.modeMask * 0.55h);
        litColor += _BackLightColor.rgb * _BackLightStrength * scatter;
    #endif

    #if defined(FSHADER_ICE_CRACKS)
        half crackRim = saturate(0.25h + Pow5(1.0h - saturate(dot(surface.normalWS, viewDirection))));
        litColor += _CrackGlowColor.rgb * _CrackGlowStrength * surface.modeDetail * crackRim;
    #endif

    #if defined(FSHADER_ICE_SPARKLE)
        half distanceFade = saturate(1.0h - distance(FSWorldSpaceCameraPosition(), input.worldPosition) / max(_SparkleDistance, 0.1h));
        half randomValue = FSPlusIceHash(floor(input.worldPosition * max(_SparkleDensity, 1.0h)));
        half sparkleCell = step(1.0h - saturate(_SparkleStrength) * 0.04h, randomValue);
        half sparklePower = lerp(96.0h, 18.0h, saturate(_SparkleSize));
        half sparkleAngle = pow(saturate(dot(surface.normalWS, normalize(viewDirection + lightDirection))), sparklePower);
        half vertexMask = 1.0h;
        #if defined(FSHADER_VERTEX_COLOR)
            vertexMask = input.vertexColor.b;
        #endif
        litColor += sparkleCell * sparkleAngle * distanceFade * vertexMask * _LightColor0.rgb * 2.2h;
    #endif
    return litColor;
}

#endif
