#ifndef FSHADER_WATER_INCLUDED
#define FSHADER_WATER_INCLUDED

sampler2D _WaveNormalMap;
float4 _WaveNormalMap_ST;
sampler2D _FoamMap;
float4 _FoamMap_ST;
half4 _ShallowColor;
half4 _DeepColor;
half _WaveNormalScale;
float4 _WaveSpeedA;
float4 _WaveSpeedB;
half _WaveAmplitude;
half _WaveLength;
float4 _WaveDirection;
half _FresnelStrength;
half _FoamStrength;
half _RefractionStrength;

void FSModeModifyVertex(inout float3 positionOS, half3 normalOS, half4 vertexColor)
{
    #if defined(FSHADER_WATER_VERTEX_WAVES)
        float3 worldPosition = mul(unity_ObjectToWorld, float4(positionOS, 1.0)).xyz;
        float2 directionA = normalize(_WaveDirection.xy + float2(0.0001, 0.0));
        float2 directionB = float2(-directionA.y, directionA.x);
        float waveNumber = 6.2831853 / max(_WaveLength, 0.05h);
        float phaseA = dot(worldPosition.xz, directionA) * waveNumber + _Time.y * 1.15;
        float phaseB = dot(worldPosition.xz, directionB) * waveNumber * 1.37 + _Time.y * 0.82;
        half weight = vertexColor.g;
        float displacement = (sin(phaseA) + sin(phaseB) * 0.5) * _WaveAmplitude * weight;
        worldPosition.y += displacement;
        worldPosition.xz += (cos(phaseA) * directionA + cos(phaseB) * directionB * 0.35) *
                            (_WaveAmplitude * 0.12 * weight);
        positionOS = mul(unity_WorldToObject, float4(worldPosition, 1.0)).xyz;
    #endif
}

void FSModeModifySurface(inout FSSurfaceData surface, FSVaryings input)
{
    half depthWeight = surface.height;
    #if defined(FSHADER_VERTEX_COLOR)
        depthWeight = input.vertexColor.b;
        surface.alpha *= input.vertexColor.a;
    #endif
    half3 waterTint = lerp(_ShallowColor.rgb, _DeepColor.rgb, saturate(depthWeight));
    surface.baseColor *= waterTint;

    #if defined(FSHADER_WATER_WAVE_NORMAL)
        float2 worldUV = input.worldPosition.xz * _WaveNormalMap_ST.xy + _WaveNormalMap_ST.zw;
        half3 normalA = UnpackScaleNormal(
            tex2D(_WaveNormalMap, worldUV + _Time.y * _WaveSpeedA.xy),
            _WaveNormalScale);
        half3 normalB = UnpackScaleNormal(
            tex2D(_WaveNormalMap, worldUV.yx + _Time.y * _WaveSpeedB.xy),
            _WaveNormalScale);
        half3 waveNormalTS = BlendNormals(normalA, normalB);
        surface.normalWS = FSTangentNormalToWorld(
            waveNormalTS,
            normalize(input.worldNormal),
            normalize(input.worldTangent),
            normalize(input.worldBitangent));
    #endif

    half foam = 0.0h;
    #if defined(FSHADER_WATER_FOAM)
        float2 foamUV = input.uv * _FoamMap_ST.xy + _FoamMap_ST.zw;
        foam = tex2D(_FoamMap, foamUV + _Time.y * _WaveSpeedA.xy * 0.25).r;
        #if defined(FSHADER_VERTEX_COLOR)
            foam *= input.vertexColor.r;
        #endif
        foam = saturate(foam * _FoamStrength);
        surface.roughness = lerp(surface.roughness, 0.92h, foam);
        surface.baseColor = lerp(surface.baseColor, half3(0.86h, 0.95h, 1.0h), foam * 0.7h);
    #endif
    surface.modeMask = foam;
}

half3 FSModeReflectionNormal(FSVaryings input, FSSurfaceData surface)
{
    half3 geometricNormal = normalize(input.worldNormal);
    half bend = (1.0h - rcp(max(_IOR, 1.001h))) * _RefractionStrength;
    return normalize(lerp(geometricNormal, surface.normalWS, saturate(0.35h + bend * 4.0h)));
}

half3 FSModeModifyLighting(FSVaryings input, FSSurfaceData surface, half3 litColor)
{
    half3 viewDirection = FSWorldSpaceViewDirection(input.worldPosition);
    half fresnel = Pow5(1.0h - saturate(dot(surface.normalWS, viewDirection)));
    litColor += fresnel * _FresnelStrength * _ReflectionStrength * _ShallowColor.rgb * 0.18h;
    litColor += surface.modeMask * half3(0.1h, 0.13h, 0.15h);
    return litColor;
}

#endif
