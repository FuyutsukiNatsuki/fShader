Shader "fShader/Plus/Standard"
{
    Properties
    {
        [MainTexture] _BaseMap ("Base Map", 2D) = "white" {}
        [MainColor] _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
        [NoScaleOffset] _ARMHMap ("ARMH (AO/Roughness/Metallic/Height)", 2D) = "white" {}
        _AOStrength ("AO Strength", Range(0, 1)) = 1
        _Roughness ("Roughness", Range(0.02, 1)) = 0.5
        _Metallic ("Metallic", Range(0, 1)) = 0
        [Normal] [NoScaleOffset] _NormalMap ("Normal Map", 2D) = "bump" {}
        _NormalScale ("Normal Scale", Range(0, 2)) = 1
        _HeightScale ("Height Scale", Range(0, 0.1)) = 0
        _Opacity ("Opacity", Range(0, 1)) = 1

        _ReflectionStrength ("Reflection Strength", Range(0, 2)) = 0.75
        _IOR ("Index of Refraction", Range(1, 2.5)) = 1.45
        [Toggle] _FSBoxProjection ("Box Projected Probe", Float) = 1
        [Toggle] _FSVertexColor ("Use Vertex Color", Float) = 0
        [Toggle] _LTCGI ("LTCGI", Float) = 0
        _LTCGIDiffuseStrength ("LTCGI Diffuse Strength", Range(0, 2)) = 0.65
        _LTCGISpecularStrength ("LTCGI Specular Strength", Range(0, 2)) = 0.8
        _LTCGIMaxBrightness ("LTCGI Max Brightness", Range(0.1, 10)) = 2

        [HideInInspector] _FSDebugView ("Debug View", Float) = 0
        [HideInInspector] _FSQueueOverride ("Queue Override", Float) = 0
        [HideInInspector] _Cull ("Cull", Float) = 2
        [HideInInspector] _FSVersion ("fShader Version", Float) = 0.5
        [HideInInspector] _FSEdition ("fShader Edition", Float) = 1
        [HideInInspector] _FSMode ("fShader Mode", Float) = 3
        [HideInInspector] _FSFeatureFlags ("fShader Feature Flags", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Geometry" "RenderType"="Opaque" "PreviewType"="Sphere" "LTCGI"="_LTCGI" }
        Pass
        {
            Name "FORWARD"
            Tags { "LightMode"="ForwardBase" }
            Cull [_Cull] ZWrite On ZTest LEqual Blend Off
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex FSVert
            #pragma fragment FSFragOpaque
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #pragma shader_feature_local _ FSHADER_BASEMAP
            #pragma shader_feature_local _ FSHADER_MASKMAP
            #pragma shader_feature_local _ FSHADER_NORMALMAP
            #pragma shader_feature_local _ FSHADER_DEBUG
            #pragma shader_feature_local _ FSHADER_BOX_PROJECTION
            #pragma shader_feature_local _ FSHADER_VERTEX_COLOR
            #pragma shader_feature_local _ FSHADER_LTCGI
            #include "Packages/com.fshader.core/Runtime/Shaders/Includes/fShaderCommon.cginc"
            ENDCG
        }

        Pass
        {
            Name "SHADOWCASTER"
            Tags { "LightMode"="ShadowCaster" }
            ZWrite On ZTest LEqual Cull [_Cull]
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex FSShadowVertex
            #pragma fragment FSShadowFragment
            #pragma multi_compile_shadowcaster
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"

            struct FSShadowAppData
            {
                float4 vertex : POSITION;
                half3 normal : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct FSShadowVaryings
            {
                V2F_SHADOW_CASTER;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            FSShadowVaryings FSShadowVertex(FSShadowAppData v)
            {
                FSShadowVaryings output;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(FSShadowVaryings, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(output)
                return output;
            }
            float4 FSShadowFragment(FSShadowVaryings input) : SV_Target
            {
                SHADOW_CASTER_FRAGMENT(input)
            }
            ENDCG
        }

        Pass
        {
            Name "META"
            Tags { "LightMode"="Meta" }
            Cull Off
            CGPROGRAM
            #pragma vertex FSMetaVertex
            #pragma fragment FSMetaFragment
            #pragma shader_feature EDITOR_VISUALIZATION
            #pragma shader_feature_local _ FSHADER_BASEMAP
            #include "UnityCG.cginc"
            #include "UnityMetaPass.cginc"

            sampler2D _BaseMap;
            float4 _BaseMap_ST;
            half4 _BaseColor;

            struct FSMetaVaryings
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };
            FSMetaVaryings FSMetaVertex(appdata_full input)
            {
                FSMetaVaryings output;
                output.pos = UnityMetaVertexPosition(
                    input.vertex,
                    input.texcoord1.xy,
                    input.texcoord2.xy,
                    unity_LightmapST,
                    unity_DynamicLightmapST);
                output.uv = TRANSFORM_TEX(input.texcoord.xy, _BaseMap);
                return output;
            }
            half4 FSMetaFragment(FSMetaVaryings input) : SV_Target
            {
                half4 baseSample = 1.0h;
                #if defined(FSHADER_BASEMAP)
                    baseSample = tex2D(_BaseMap, input.uv);
                #endif
                UnityMetaInput meta;
                UNITY_INITIALIZE_OUTPUT(UnityMetaInput, meta);
                meta.Albedo = baseSample.rgb * _BaseColor.rgb;
                meta.Emission = 0.0h;
                return UnityMetaFragment(meta);
            }
            ENDCG
        }
    }
    CustomEditor "fShader.Editor.fShaderInspector"
    Fallback Off
}
