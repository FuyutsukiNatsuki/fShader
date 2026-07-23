Shader "fShader/Lite/Water"
{
    Properties
    {
        [MainTexture] _BaseMap ("Base Map", 2D) = "white" {}
        [MainColor] _BaseColor ("Base Color", Color) = (0.35, 0.75, 0.9, 0.72)
        [NoScaleOffset] _ARMHMap ("ARMH (AO/Roughness/Metallic/Height)", 2D) = "white" {}
        _AOStrength ("AO Strength", Range(0, 1)) = 1
        _Roughness ("Roughness", Range(0.02, 1)) = 0.18
        _Metallic ("Metallic", Range(0, 1)) = 0
        [Normal] [NoScaleOffset] _NormalMap ("Normal Map", 2D) = "bump" {}
        _NormalScale ("Normal Scale", Range(0, 2)) = 1
        _HeightScale ("Height Scale", Range(0, 0.1)) = 0
        _Opacity ("Opacity", Range(0, 1)) = 1

        _ShallowColor ("Shallow Color", Color) = (0.28, 0.8, 0.92, 1)
        _DeepColor ("Deep Color", Color) = (0.015, 0.16, 0.32, 1)
        [Toggle] _FSWaterWaveNormal ("Wave Normal", Float) = 1
        [Normal] _WaveNormalMap ("Wave Normal Map", 2D) = "bump" {}
        _WaveNormalScale ("Wave Normal Scale", Range(0, 2)) = 0.65
        _WaveSpeedA ("Wave Speed A", Vector) = (0.035, 0.02, 0, 0)
        _WaveSpeedB ("Wave Speed B", Vector) = (-0.02, 0.03, 0, 0)
        [Toggle] _FSWaterVertexWaves ("Vertex Waves", Float) = 0
        _WaveAmplitude ("Wave Amplitude", Range(0, 0.5)) = 0.04
        _WaveLength ("Wave Length", Range(0.05, 20)) = 2.5
        _WaveDirection ("Wave Direction", Vector) = (1, 0.35, 0, 0)
        _FresnelStrength ("Fresnel Strength", Range(0, 2)) = 0.8
        [Toggle] _FSWaterFoam ("Foam", Float) = 0
        _FoamMap ("Foam Map", 2D) = "white" {}
        _FoamStrength ("Foam Strength", Range(0, 2)) = 0.65
        _RefractionStrength ("Probe Distortion", Range(0, 1)) = 0.12
        _ReflectionStrength ("Reflection Strength", Range(0, 2)) = 0.65
        _IOR ("Index of Refraction", Range(1, 2.5)) = 1.333
        [Toggle] _FSVertexColor ("Use Vertex Color", Float) = 0

        [HideInInspector] _FSDebugView ("Debug View", Float) = 0
        [HideInInspector] _FSQueueOverride ("Queue Override", Float) = 0
        [HideInInspector] _Cull ("Cull", Float) = 2
        [HideInInspector] _FSTransparentZWrite ("Transparent ZWrite", Float) = 0
        [HideInInspector] _FSVersion ("fShader Version", Float) = 0.3
        [HideInInspector] _FSEdition ("fShader Edition", Float) = 0
        [HideInInspector] _FSMode ("fShader Mode", Float) = 0
        [HideInInspector] _FSFeatureFlags ("fShader Feature Flags", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "PreviewType"="Plane" }
        Pass
        {
            Name "FORWARD"
            Tags { "LightMode"="ForwardBase" }
            Cull [_Cull] ZWrite [_FSTransparentZWrite] ZTest LEqual Blend One OneMinusSrcAlpha
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex FSVert
            #pragma fragment FSFragTransparent
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
            #pragma shader_feature_local _ FSHADER_VERTEX_COLOR
            #define FSHADER_MODE_WATER 1
            #include "Packages/com.fshader.core/Runtime/Shaders/Includes/fShaderCommon.cginc"
            ENDCG
        }
    }
    CustomEditor "fShader.Editor.fShaderInspector"
    Fallback Off
}
