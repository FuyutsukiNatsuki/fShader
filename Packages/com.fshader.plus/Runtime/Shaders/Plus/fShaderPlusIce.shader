Shader "fShader/Plus/Ice"
{
    Properties
    {
        [MainTexture] _BaseMap ("Base Map", 2D) = "white" {}
        [MainColor] _BaseColor ("Base Color", Color) = (0.62, 0.9, 1, 1)
        [NoScaleOffset] _ARMHMap ("ARMH (AO/Roughness/Metallic/Height)", 2D) = "white" {}
        _AOStrength ("AO Strength", Range(0, 1)) = 1
        _Roughness ("Roughness", Range(0.02, 1)) = 0.2
        _Metallic ("Metallic", Range(0, 1)) = 0
        [Normal] [NoScaleOffset] _NormalMap ("Normal Map", 2D) = "bump" {}
        _NormalScale ("Normal Scale", Range(0, 2)) = 0.8
        _HeightScale ("Height Scale", Range(0, 0.1)) = 0
        _Opacity ("Opacity", Range(0, 1)) = 1
        [Toggle] _FSIceTransparent ("Transparent Ice", Float) = 0

        _IceColor ("Ice Color", Color) = (0.72, 0.92, 1, 1)
        _AbsorptionColor ("Absorption Color", Color) = (0.22, 0.58, 0.82, 1)
        _IceThickness ("Ice Thickness", Range(0, 2)) = 0.65
        _AbsorptionStrength ("View Absorption", Range(0, 2)) = 0.75

        [Toggle] _FSIceFrost ("Two-scale Frost", Float) = 0
        _FrostMap ("Frost Map", 2D) = "black" {}
        _FrostColor ("Frost Color", Color) = (0.86, 0.96, 1, 0.8)
        _FrostStrength ("Frost Strength", Range(0, 2)) = 0.8
        _FrostScaleA ("Frost Scale A", Range(0.1, 8)) = 1
        _FrostScaleB ("Frost Scale B", Range(0.1, 12)) = 3.2
        _FrostEdge ("Edge Frost", Range(0, 1)) = 0.25

        [Toggle] _FSIceCracks ("Parallax Cracks", Float) = 0
        _CrackMap ("Crack Map", 2D) = "black" {}
        _CrackDepth ("Crack Depth", Range(0, 2)) = 0.75
        _CrackParallax ("Crack Parallax", Range(0, 0.08)) = 0.018
        _CrackGlowColor ("Internal Crack Color", Color) = (0.25, 0.8, 1, 1)
        _CrackGlowStrength ("Internal Crack Glow", Range(0, 3)) = 0.35

        [Toggle] _FSIceBackLight ("Back Light", Float) = 1
        _BackLightColor ("Back Light Color", Color) = (0.35, 0.82, 1, 1)
        _BackLightStrength ("Back Light Strength", Range(0, 3)) = 0.65
        _BackLightThickness ("Back Light Thickness", Range(0, 2)) = 0.75

        [Toggle] _FSIceSparkle ("Sparkle", Float) = 0
        _SparkleStrength ("Sparkle Strength", Range(0, 2)) = 0.6
        _SparkleDensity ("Sparkle Density", Range(1, 128)) = 42
        _SparkleSize ("Sparkle Size", Range(0, 1)) = 0.22
        _SparkleDistance ("Sparkle Fade Distance", Range(0.1, 100)) = 18

        _ReflectionStrength ("Reflection Strength", Range(0, 2)) = 0.85
        _IOR ("Index of Refraction", Range(1, 2.5)) = 1.31
        _RefractionStrength ("Screen Distortion", Range(0, 1)) = 0.12
        [Toggle] _FSBoxProjection ("Box Projected Probe", Float) = 1
        [Toggle] _FSScreenRefraction ("Screen Refraction (Heavy)", Float) = 0
        [Toggle] _FSVertexColor ("Use Vertex Color", Float) = 0

        [HideInInspector] _FSDebugView ("Debug View", Float) = 0
        [HideInInspector] _FSQueueOverride ("Queue Override", Float) = 0
        [HideInInspector] _Cull ("Cull", Float) = 2
        [HideInInspector] _FSVersion ("fShader Version", Float) = 0.5
        [HideInInspector] _FSEdition ("fShader Edition", Float) = 1
        [HideInInspector] _FSMode ("fShader Mode", Float) = 1
        [HideInInspector] _FSFeatureFlags ("fShader Feature Flags", Float) = 0
        [HideInInspector] _FSSrcBlend ("Source Blend", Float) = 1
        [HideInInspector] _FSDstBlend ("Destination Blend", Float) = 0
        [HideInInspector] _FSZWrite ("ZWrite", Float) = 1
    }
    SubShader
    {
        Tags { "Queue"="Geometry" "RenderType"="Opaque" "PreviewType"="Sphere" }
        Pass
        {
            Name "FORWARD"
            Tags { "LightMode"="ForwardBase" }
            Cull [_Cull] ZWrite [_FSZWrite] ZTest LEqual Blend [_FSSrcBlend] [_FSDstBlend]
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex FSVert
            #pragma fragment FSFragIce
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #pragma shader_feature_local _ FSHADER_BASEMAP
            #pragma shader_feature_local _ FSHADER_MASKMAP
            #pragma shader_feature_local _ FSHADER_NORMALMAP
            #pragma shader_feature_local _ FSHADER_DEBUG
            #pragma shader_feature_local _ FSHADER_ICE_FROST
            #pragma shader_feature_local _ FSHADER_ICE_CRACKS
            #pragma shader_feature_local _ FSHADER_ICE_BACKLIGHT
            #pragma shader_feature_local _ FSHADER_ICE_SPARKLE
            #pragma shader_feature_local _ FSHADER_ICE_TRANSPARENT
            #pragma shader_feature_local _ FSHADER_BOX_PROJECTION
            #pragma shader_feature_local _ FSHADER_VERTEX_COLOR
            #define FSHADER_PLUS_ICE 1
            #include "Packages/com.fshader.core/Runtime/Shaders/Includes/fShaderCommon.cginc"
            ENDCG
        }
    }
    CustomEditor "fShader.Editor.fShaderInspector"
    Fallback Off
}
