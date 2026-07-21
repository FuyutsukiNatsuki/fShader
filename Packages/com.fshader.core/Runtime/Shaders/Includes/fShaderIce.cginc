#ifndef FSHADER_ICE_INCLUDED
#define FSHADER_ICE_INCLUDED

sampler2D _FrostMap;
sampler2D _CrackMap;
half4 _IceColor;
half _IceThickness;
half _FrostStrength;
half _CrackDepth;
half4 _ScatterColor;
half _ScatterStrength;
half _SparkleStrength;
half _SparkleDistance;

half FSHash13(float3 value)
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
    half cracks = 0.0h;
    #if defined(FSHADER_ICE_FROST)
        frost = tex2D(_FrostMap, input.uv).r * _FrostStrength;
        #if defined(FSHADER_VERTEX_COLOR)
            frost *= input.vertexColor.r;
        #endif
        frost = saturate(frost);
        surface.roughness = lerp(surface.roughness, 0.96h, frost);
    #endif
    #if defined(FSHADER_ICE_CRACKS)
        cracks = tex2D(_CrackMap, input.uv).r;
        #if defined(FSHADER_VERTEX_COLOR)
            cracks *= input.vertexColor.g;
        #endif
        cracks = saturate(cracks * _CrackDepth);
    #endif

    half thickness = saturate(_IceThickness);
    #if defined(FSHADER_VERTEX_COLOR)
        thickness *= input.vertexColor.a;
    #endif
    half3 iceTint = lerp(1.0h, _IceColor.rgb, thickness);
    surface.baseColor *= iceTint;
    surface.baseColor = lerp(surface.baseColor, half3(0.82h, 0.94h, 1.0h), frost * 0.55h);
    surface.baseColor *= lerp(1.0h, 0.45h, cracks);
    surface.modeMask = frost;
    surface.modeDetail = cracks;
    #if defined(FSHADER_ICE_TRANSPARENT)
        surface.alpha = saturate(surface.alpha + frost * 0.38h + cracks * 0.16h + thickness * 0.10h);
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
    #if defined(FSHADER_ICE_SCATTER)
        half wrappedLight = saturate((dot(surface.normalWS, lightDirection) + 0.35h) / 1.35h);
        half rim = Pow5(1.0h - saturate(dot(surface.normalWS, viewDirection)));
        half scatterMask = saturate(wrappedLight * 0.65h + rim);
        scatterMask *= saturate(1.0h - surface.modeMask * 0.5h);
        litColor += _ScatterColor.rgb * _ScatterStrength * scatterMask * (1.0h - surface.modeDetail * 0.4h);
    #endif

    #if defined(FSHADER_ICE_SPARKLE)
        half distanceFade = saturate(1.0h - distance(FSWorldSpaceCameraPosition(), input.worldPosition) /
                                     max(_SparkleDistance, 0.1h));
        half randomValue = FSHash13(floor(input.worldPosition * 38.0));
        half sparkleCell = step(1.0h - saturate(_SparkleStrength) * 0.035h, randomValue);
        half sparkleAngle = pow(saturate(dot(surface.normalWS, normalize(viewDirection + lightDirection))), 48.0h);
        half vertexMask = 1.0h;
        #if defined(FSHADER_VERTEX_COLOR)
            vertexMask = input.vertexColor.b;
        #endif
        litColor += sparkleCell * sparkleAngle * distanceFade * vertexMask * _LightColor0.rgb * 1.8h;
    #endif
    return litColor;
}

#endif
