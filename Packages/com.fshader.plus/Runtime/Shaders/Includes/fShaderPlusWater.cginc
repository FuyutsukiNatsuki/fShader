#ifndef FSHADER_PLUS_WATER_INCLUDED
#define FSHADER_PLUS_WATER_INCLUDED

sampler2D _WaveNormalMap;
float4 _WaveNormalMap_ST;
sampler2D _WaveNormalMap2;
float4 _WaveNormalMap2_ST;
sampler2D _FoamMap;
float4 _FoamMap_ST;
sampler2D _CausticsMap;
float4 _CausticsMap_ST;

half4 _ShallowColor;
half4 _DeepColor;
half4 _AbsorptionColor;
half4 _FoamColor;
half4 _CausticsColor;
half _WaveNormalScale;
half _WaveNormalScale2;
float4 _WaveSpeedA;
float4 _WaveSpeedB;
half _WaveScaleA;
half _WaveScaleB;
half _WaveAmplitude;
half _WaveLength;
float4 _WaveDirection;
half _WaveCount;
half _WaveTimeScale;
half _WaterThickness;
half _DepthStrength;
half _DepthBias;
half _AbsorptionStrength;
half _FresnelStrength;
half _FoamStrength;
half _FoamDetailScale;
half _FoamCrestStrength;
half _RefractionStrength;
half _CausticsStrength;

float2 FSPlusWaterDirection(float2 direction)
{
    return normalize(direction + float2(0.0001, 0.0001));
}

float FSPlusWaterPhase(float2 worldXZ, float2 direction, float lengthScale, float speedScale)
{
    float waveNumber = 6.2831853 / max(_WaveLength * lengthScale, 0.05h);
    return dot(worldXZ, FSPlusWaterDirection(direction)) * waveNumber +
           _Time.y * _WaveTimeScale * speedScale;
}

float FSPlusWaterWave(float2 worldXZ, float2 direction, float lengthScale, float speedScale, float amplitudeScale)
{
    return sin(FSPlusWaterPhase(worldXZ, direction, lengthScale, speedScale)) *
           _WaveAmplitude * amplitudeScale;
}

half FSPlusWaterCrest(float2 worldXZ)
{
    float2 directionA = FSPlusWaterDirection(_WaveDirection.xy);
    float2 directionB = float2(-directionA.y, directionA.x);
    half crest = sin(FSPlusWaterPhase(worldXZ, directionA, 1.0, 1.0)) * 0.5h + 0.5h;
    crest += (sin(FSPlusWaterPhase(worldXZ, directionB, 0.72, 0.78)) * 0.5h + 0.5h) * 0.5h;
    return saturate((crest - 1.02h) * 3.5h);
}

void FSModeModifyVertex(inout float3 positionOS, half3 normalOS, half4 vertexColor)
{
    #if defined(FSHADER_WATER_VERTEX_WAVES)
        float3 worldPosition = mul(unity_ObjectToWorld, float4(positionOS, 1.0)).xyz;
        float2 directionA = FSPlusWaterDirection(_WaveDirection.xy);
        float2 directionB = float2(-directionA.y, directionA.x);
        float2 directionC = FSPlusWaterDirection(directionA + directionB * 0.55);
        float2 directionD = FSPlusWaterDirection(-directionA + directionB * 0.35);
        half vertexWeight = 1.0h;
        #if defined(FSHADER_VERTEX_COLOR)
            vertexWeight = vertexColor.g;
        #endif

        float displacement = FSPlusWaterWave(worldPosition.xz, directionA, 1.0, 1.0, 0.48);
        if (_WaveCount > 1.5h) displacement += FSPlusWaterWave(worldPosition.xz, directionB, 0.72, 0.78, 0.26);
        if (_WaveCount > 2.5h) displacement += FSPlusWaterWave(worldPosition.xz, directionC, 1.45, 1.31, 0.16);
        if (_WaveCount > 3.5h) displacement += FSPlusWaterWave(worldPosition.xz, directionD, 0.48, 1.63, 0.10);
        worldPosition.y += displacement * vertexWeight;
        positionOS = mul(unity_WorldToObject, float4(worldPosition, 1.0)).xyz;
    #endif
}

void FSModeModifySurface(inout FSSurfaceData surface, FSVaryings input)
{
    half depthSource = surface.height;
    #if defined(FSHADER_VERTEX_COLOR)
        depthSource = saturate(depthSource * 0.5h + input.vertexColor.b * 0.5h);
        surface.alpha *= input.vertexColor.a;
    #endif
    half depth = saturate(_DepthBias + depthSource * _DepthStrength);
    half3 waterTint = lerp(_ShallowColor.rgb, _DeepColor.rgb, depth);
    half3 absorption = lerp(1.0h, _AbsorptionColor.rgb, saturate(depth * _AbsorptionStrength));
    surface.baseColor *= waterTint * absorption;
    surface.height = depth;

    #if defined(FSHADER_WATER_WAVE_NORMAL)
        float2 uvA = input.worldPosition.xz * _WaveNormalMap_ST.xy * _WaveScaleA + _WaveNormalMap_ST.zw;
        float2 uvB = input.worldPosition.xz * _WaveNormalMap2_ST.xy * _WaveScaleB + _WaveNormalMap2_ST.zw;
        half3 normalA = UnpackScaleNormal(tex2D(_WaveNormalMap, uvA + _Time.y * _WaveSpeedA.xy), _WaveNormalScale);
        half3 normalB = UnpackScaleNormal(tex2D(_WaveNormalMap2, uvB + _Time.y * _WaveSpeedB.xy), _WaveNormalScale2);
        half3 waveNormalTS = BlendNormals(normalA, normalB);
        half3 waveNormalWS = FSTangentNormalToWorld(
            waveNormalTS,
            normalize(input.worldNormal),
            normalize(input.worldTangent),
            normalize(input.worldBitangent));
        surface.normalWS = normalize(surface.normalWS + waveNormalWS - normalize(input.worldNormal));
    #endif

    half foam = 0.0h;
    #if defined(FSHADER_WATER_FOAM)
        float2 foamUV = input.worldPosition.xz * _FoamMap_ST.xy + _FoamMap_ST.zw;
        half foamLarge = tex2D(_FoamMap, foamUV + _Time.y * _WaveSpeedA.xy * 0.18h).r;
        half foamFine = tex2D(_FoamMap, foamUV * _FoamDetailScale - _Time.y * _WaveSpeedB.xy * 0.27h).r;
        half crest = FSPlusWaterCrest(input.worldPosition.xz) * _FoamCrestStrength;
        foam = saturate((foamLarge * foamFine * 1.7h + crest) * _FoamStrength);
        #if defined(FSHADER_VERTEX_COLOR)
            foam *= input.vertexColor.r;
        #endif
        surface.roughness = lerp(surface.roughness, 0.94h, foam);
        surface.baseColor = lerp(surface.baseColor, _FoamColor.rgb, foam * _FoamColor.a);
        surface.alpha = saturate(surface.alpha + foam * 0.15h);
    #endif

    #if defined(FSHADER_WATER_CAUSTICS)
        float2 causticsUV = input.worldPosition.xz * _CausticsMap_ST.xy + _CausticsMap_ST.zw;
        half caustics = tex2D(_CausticsMap, causticsUV + _Time.y * _WaveSpeedA.xy * 0.11h).r;
        surface.baseColor += _CausticsColor.rgb * caustics * _CausticsStrength * (1.0h - foam);
    #endif
    surface.modeMask = foam;
}

half3 FSModeReflectionNormal(FSVaryings input, FSSurfaceData surface)
{
    half3 geometricNormal = normalize(input.worldNormal);
    half bend = (1.0h - rcp(max(_IOR, 1.001h))) * _RefractionStrength;
    return normalize(lerp(geometricNormal, surface.normalWS, saturate(0.38h + bend * 3.5h)));
}

half3 FSModeModifyLighting(FSVaryings input, FSSurfaceData surface, half3 litColor)
{
    half3 viewDirection = FSWorldSpaceViewDirection(input.worldPosition);
    half fresnel = Pow5(1.0h - saturate(dot(surface.normalWS, viewDirection)));
    litColor += fresnel * _FresnelStrength * _ReflectionStrength * _ShallowColor.rgb * 0.2h;
    litColor += surface.modeMask * _FoamColor.rgb * 0.12h;
    return litColor;
}

#endif
