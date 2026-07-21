Shader "Hidden/fShader/Plus/WaterScreenRefraction"
{
    Properties
    {
        [MainTexture] _BaseMap ("Base Map", 2D) = "white" {}
        [MainColor] _BaseColor ("Base Color", Color) = (0.12, 0.58, 0.72, 0.72)
        [NoScaleOffset] _ARMHMap ("ARMH (AO/Roughness/Metallic/Height)", 2D) = "white" {}
        _AOStrength ("AO Strength", Range(0, 1)) = 1
        _Roughness ("Roughness", Range(0.02, 1)) = 0.12
        _Metallic ("Metallic", Range(0, 1)) = 0
        [Normal] [NoScaleOffset] _NormalMap ("Normal Map", 2D) = "bump" {}
        _NormalScale ("Normal Scale", Range(0, 2)) = 0.5
        _HeightScale ("Height Scale", Range(0, 0.1)) = 0
        _Opacity ("Opacity", Range(0, 1)) = 1
        _ShallowColor ("Shallow Color", Color) = (0.35, 0.88, 0.92, 1)
        _DeepColor ("Deep Color", Color) = (0.012, 0.10, 0.25, 1)
        _AbsorptionColor ("Absorption Color", Color) = (0.18, 0.72, 0.78, 1)
        _AbsorptionStrength ("Absorption Strength", Range(0, 2)) = 0.8
        _WaterThickness ("Water Thickness", Range(0, 2)) = 1
        _DepthStrength ("Height/Vertex Depth Strength", Range(0, 2)) = 1
        _DepthBias ("Depth Bias", Range(-1, 1)) = 0
        [Toggle] _FSWaterWaveNormal ("Dual Wave Normal", Float) = 1
        [Normal] _WaveNormalMap ("Wave Normal A", 2D) = "bump" {}
        [Normal] _WaveNormalMap2 ("Wave Normal B", 2D) = "bump" {}
        _WaveNormalScale ("Wave Normal Scale A", Range(0, 2)) = 0.7
        _WaveNormalScale2 ("Wave Normal Scale B", Range(0, 2)) = 0.45
        _WaveScaleA ("Wave Scale A", Range(0.05, 8)) = 1
        _WaveScaleB ("Wave Scale B", Range(0.05, 8)) = 1.8
        _WaveSpeedA ("Wave Speed A", Vector) = (0.035, 0.02, 0, 0)
        _WaveSpeedB ("Wave Speed B", Vector) = (-0.018, 0.03, 0, 0)
        [Toggle] _FSWaterVertexWaves ("World Vertex Waves", Float) = 0
        _WaveCount ("Wave Count", Range(1, 4)) = 2
        _WaveAmplitude ("Wave Amplitude", Range(0, 0.75)) = 0.08
        _WaveLength ("Wave Length", Range(0.05, 30)) = 3.5
        _WaveDirection ("Wave Direction", Vector) = (1, 0.35, 0, 0)
        _WaveTimeScale ("Wave Time Scale", Range(0, 3)) = 1
        [Toggle] _FSWaterFoam ("Multi-scale Foam", Float) = 0
        _FoamMap ("Foam Map", 2D) = "black" {}
        _FoamColor ("Foam Color", Color) = (0.86, 0.96, 1, 0.85)
        _FoamStrength ("Foam Strength", Range(0, 2)) = 0.75
        _FoamDetailScale ("Foam Detail Scale", Range(1, 8)) = 3
        _FoamCrestStrength ("Wave Crest Foam", Range(0, 2)) = 0.55
        [Toggle] _FSWaterCaustics ("Surface Caustics", Float) = 0
        _CausticsMap ("Caustics Map", 2D) = "black" {}
        _CausticsColor ("Caustics Color", Color) = (0.6, 0.95, 1, 1)
        _CausticsStrength ("Caustics Strength", Range(0, 2)) = 0.35
        _FresnelStrength ("Fresnel Strength", Range(0, 2)) = 0.9
        _RefractionStrength ("Probe/Screen Distortion", Range(0, 1)) = 0.14
        _ReflectionStrength ("Reflection Strength", Range(0, 2)) = 0.9
        _IOR ("Index of Refraction", Range(1, 2.5)) = 1.333
        [Toggle] _FSBoxProjection ("Box Projected Probe", Float) = 1
        [Toggle] _FSScreenRefraction ("Screen Refraction (Heavy)", Float) = 1
        [Toggle] _FSVertexColor ("Use Vertex Color", Float) = 0
        [Toggle] _LTCGI ("LTCGI", Float) = 0
        _LTCGIDiffuseStrength ("LTCGI Diffuse Strength", Range(0, 2)) = 0.65
        _LTCGISpecularStrength ("LTCGI Specular Strength", Range(0, 2)) = 0.8
        _LTCGIMaxBrightness ("LTCGI Max Brightness", Range(0.1, 10)) = 2
        [HideInInspector] _FSDebugView ("Debug View", Float) = 0
        [HideInInspector] _FSQueueOverride ("Queue Override", Float) = 0
        [HideInInspector] _FSTransparentZWrite ("Transparent ZWrite", Float) = 0
        [HideInInspector] _FSVersion ("fShader Version", Float) = 0.5
        [HideInInspector] _FSEdition ("fShader Edition", Float) = 1
        [HideInInspector] _FSMode ("fShader Mode", Float) = 0
        [HideInInspector] _FSFeatureFlags ("fShader Feature Flags", Float) = 1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "PreviewType"="Plane" "LTCGI"="_LTCGI" }
        GrabPass { "_fShaderSharedGrab" }
        Pass
        {
            Name "FORWARD"
            Tags { "LightMode"="ForwardBase" }
            Cull Back ZWrite [_FSTransparentZWrite] ZTest LEqual Blend Off
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex FSVert
            #pragma fragment FSFragWaterScreen
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #pragma shader_feature_local _ FSHADER_BASEMAP
            #pragma shader_feature_local _ FSHADER_MASKMAP
            #pragma shader_feature_local _ FSHADER_NORMALMAP
            #pragma shader_feature_local _ FSHADER_DEBUG
            #pragma shader_feature_local _ FSHADER_WATER_WAVE_NORMAL
            #pragma shader_feature_local _ FSHADER_WATER_VERTEX_WAVES
            #pragma shader_feature_local _ FSHADER_WATER_FOAM
            #pragma shader_feature_local _ FSHADER_WATER_CAUSTICS
            #pragma shader_feature_local _ FSHADER_BOX_PROJECTION
            #pragma shader_feature_local _ FSHADER_VERTEX_COLOR
            #pragma shader_feature_local _ FSHADER_LTCGI
            #define FSHADER_PLUS_WATER 1
            #define FSHADER_SCREEN_REFRACTION 1
            #define FSHADER_SCREEN_WATER 1
            #include "Packages/com.fshader.core/Runtime/Shaders/Includes/fShaderCommon.cginc"
            ENDCG
        }
    }
    CustomEditor "fShader.Editor.fShaderInspector"
    Fallback Off
}
