Shader "fShader/Lite/Glass"
{
    Properties
    {
        [MainTexture] _BaseMap ("Base Map", 2D) = "white" {}
        [MainColor] _BaseColor ("Base Color", Color) = (0.92, 0.98, 1, 0.28)
        [NoScaleOffset] _ARMHMap ("ARMH (AO/Roughness/Metallic/Height)", 2D) = "white" {}
        _AOStrength ("AO Strength", Range(0, 1)) = 1
        _Roughness ("Roughness", Range(0.02, 1)) = 0.08
        _Metallic ("Metallic", Range(0, 1)) = 0
        [Normal] [NoScaleOffset] _NormalMap ("Normal Map", 2D) = "bump" {}
        _NormalScale ("Normal Scale", Range(0, 2)) = 1
        _HeightScale ("Height Scale", Range(0, 0.1)) = 0
        _Opacity ("Opacity", Range(0, 1)) = 1

        _TransmissionColor ("Transmission Color", Color) = (0.88, 0.97, 1, 1)
        _GlassThickness ("Glass Thickness", Range(0, 1)) = 0.25
        _IOR ("Index of Refraction", Range(1, 2.5)) = 1.5
        _ReflectionStrength ("Reflection Strength", Range(0, 2)) = 1
        _RefractionStrength ("Probe Distortion", Range(0, 1)) = 0.12
        [Toggle] _FSGlassCondensation ("Condensation", Float) = 0
        [NoScaleOffset] _CondensationMap ("Condensation Map", 2D) = "black" {}
        [Toggle] _FSGlassDropletNormal ("Droplet Normal", Float) = 0
        [Normal] [NoScaleOffset] _CondensationNormal ("Condensation Normal", 2D) = "bump" {}
        _CondensationAmount ("Condensation Amount", Range(0, 2)) = 0.65
        _DropletSpeed ("Droplet Speed", Vector) = (0, -0.015, 0, -0.008)
        [Toggle] _FSScreenRefraction ("Screen Refraction (Heavy)", Float) = 0
        [Toggle] _FSVertexColor ("Use Vertex Color", Float) = 0

        [HideInInspector] _FSDebugView ("Debug View", Float) = 0
        [HideInInspector] _FSVersion ("fShader Version", Float) = 0.3
        [HideInInspector] _FSEdition ("fShader Edition", Float) = 0
        [HideInInspector] _FSMode ("fShader Mode", Float) = 2
        [HideInInspector] _FSFeatureFlags ("fShader Feature Flags", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "PreviewType"="Sphere" }
        Pass
        {
            Name "FORWARD"
            Tags { "LightMode"="ForwardBase" }
            Cull Back ZWrite Off ZTest LEqual Blend One OneMinusSrcAlpha
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
            #pragma shader_feature_local _ FSHADER_GLASS_CONDENSATION
            #pragma shader_feature_local _ FSHADER_GLASS_DROPLET_NORMAL
            #pragma shader_feature_local _ FSHADER_VERTEX_COLOR
            #define FSHADER_MODE_GLASS 1
            #include "Packages/com.fshader.core/Runtime/Shaders/Includes/fShaderCommon.cginc"
            ENDCG
        }
    }
    CustomEditor "fShader.Editor.fShaderInspector"
    Fallback Off
}
