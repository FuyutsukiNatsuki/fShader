Shader "fShader/Plus/Glass"
{
    Properties
    {
        [MainTexture] _BaseMap ("Base Map", 2D) = "white" {}
        [MainColor] _BaseColor ("Base Color", Color) = (0.92, 0.98, 1, 0.28)
        [NoScaleOffset] _ARMHMap ("ARMH (AO/Roughness/Metallic/Height)", 2D) = "white" {}
        _AOStrength ("AO Strength", Range(0, 1)) = 1
        _Roughness ("Roughness", Range(0.02, 1)) = 0.06
        _Metallic ("Metallic", Range(0, 1)) = 0
        [Normal] [NoScaleOffset] _NormalMap ("Normal Map", 2D) = "bump" {}
        _NormalScale ("Normal Scale", Range(0, 2)) = 0.35
        _HeightScale ("Height Scale", Range(0, 0.1)) = 0
        _Opacity ("Opacity", Range(0, 1)) = 1

        _TransmissionColor ("Transmission Color", Color) = (0.9, 0.98, 1, 1)
        _AbsorptionColor ("Absorption Color", Color) = (0.55, 0.86, 0.92, 1)
        _GlassThickness ("Glass Thickness", Range(0, 2)) = 0.3
        _AbsorptionStrength ("Absorption Strength", Range(0, 2)) = 0.45
        _IOR ("Index of Refraction", Range(1, 2.5)) = 1.5
        _ReflectionStrength ("Reflection Strength", Range(0, 2)) = 1.1
        _RefractionStrength ("Probe/Screen Distortion", Range(0, 1)) = 0.14

        [Toggle] _FSGlassCondensation ("Packed Condensation", Float) = 0
        _CondensationMap ("Condensation RGB Map", 2D) = "black" {}
        _CondensationColor ("Condensation Color", Color) = (0.82, 0.95, 1, 0.35)
        _CondensationAmount ("Condensation Amount", Range(0, 2)) = 0.75
        _DropletStrength ("Droplet (R)", Range(0, 2)) = 1
        _TrailStrength ("Trail (G)", Range(0, 2)) = 0.75
        _MicroFogStrength ("Micro Fog (B)", Range(0, 2)) = 0.65
        _CondensationRoughness ("Local Roughness", Range(0.02, 1)) = 0.86
        _CondensationOpacity ("Local Opacity", Range(0, 0.5)) = 0.18
        _CondensationFadeDistance ("Trail Fade Distance", Range(0.1, 100)) = 22
        _DropletSpeed ("Droplet/Trail Speed", Vector) = (0, -0.018, 0, -0.007)
        [Toggle] _FSGlassDropletNormal ("Condensation Normal", Float) = 0
        [Normal] _CondensationNormal ("Condensation Normal Map", 2D) = "bump" {}
        _CondensationNormalScale ("Condensation Normal Scale", Range(0, 2)) = 0.85

        [Toggle] _FSBoxProjection ("Box Projected Probe", Float) = 1
        [Toggle] _FSScreenRefraction ("Screen Refraction (Heavy)", Float) = 0
        [Toggle] _FSVertexColor ("Use Vertex Color", Float) = 0
        [Toggle] _LTCGI ("LTCGI", Float) = 0
        _LTCGIDiffuseStrength ("LTCGI Diffuse Strength", Range(0, 2)) = 0.45
        _LTCGISpecularStrength ("LTCGI Specular Strength", Range(0, 2)) = 0.9
        _LTCGICondensationDiffuse ("Condensation Diffuse Boost", Range(0, 2)) = 0.5
        _LTCGIMaxBrightness ("LTCGI Max Brightness", Range(0.1, 10)) = 2

        [HideInInspector] _FSDebugView ("Debug View", Float) = 0
        [HideInInspector] _FSQueueOverride ("Queue Override", Float) = 0
        [HideInInspector] _Cull ("Cull", Float) = 2
        [HideInInspector] _FSTransparentZWrite ("Transparent ZWrite", Float) = 0
        [HideInInspector] _FSVersion ("fShader Version", Float) = 0.5
        [HideInInspector] _FSEdition ("fShader Edition", Float) = 1
        [HideInInspector] _FSMode ("fShader Mode", Float) = 2
        [HideInInspector] _FSFeatureFlags ("fShader Feature Flags", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "PreviewType"="Sphere" "LTCGI"="_LTCGI" }
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
            #pragma shader_feature_local _ FSHADER_GLASS_CONDENSATION
            #pragma shader_feature_local _ FSHADER_GLASS_DROPLET_NORMAL
            #pragma shader_feature_local _ FSHADER_BOX_PROJECTION
            #pragma shader_feature_local _ FSHADER_VERTEX_COLOR
            #pragma shader_feature_local _ FSHADER_LTCGI
            #define FSHADER_PLUS_GLASS 1
            #include "Packages/com.fshader.core/Runtime/Shaders/Includes/fShaderCommon.cginc"
            ENDCG
        }
    }
    CustomEditor "fShader.Editor.fShaderInspector"
    Fallback Off
}
