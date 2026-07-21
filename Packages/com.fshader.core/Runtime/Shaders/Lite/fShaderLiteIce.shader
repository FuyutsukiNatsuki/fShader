Shader "fShader/Lite/Ice"
{
    Properties
    {
        [MainTexture] _BaseMap ("Base Map", 2D) = "white" {}
        [MainColor] _BaseColor ("Base Color", Color) = (0.75, 0.92, 1, 1)
        [NoScaleOffset] _ARMHMap ("ARMH (AO/Roughness/Metallic/Height)", 2D) = "white" {}
        _AOStrength ("AO Strength", Range(0, 1)) = 1
        _Roughness ("Roughness", Range(0.02, 1)) = 0.25
        _Metallic ("Metallic", Range(0, 1)) = 0
        [Normal] [NoScaleOffset] _NormalMap ("Normal Map", 2D) = "bump" {}
        _NormalScale ("Normal Scale", Range(0, 2)) = 1
        _HeightScale ("Height Scale", Range(0, 0.1)) = 0
        _Opacity ("Opacity", Range(0, 1)) = 1
        [Toggle] _FSIceTransparent ("Transparent Ice", Float) = 0

        _IceColor ("Ice Color", Color) = (0.45, 0.78, 1, 1)
        _IceThickness ("Ice Thickness", Range(0, 1)) = 0.55
        [Toggle] _FSIceFrost ("Frost", Float) = 0
        [NoScaleOffset] _FrostMap ("Frost Map", 2D) = "white" {}
        _FrostStrength ("Frost Strength", Range(0, 2)) = 0.7
        [Toggle] _FSIceCracks ("Cracks", Float) = 0
        [NoScaleOffset] _CrackMap ("Crack Map", 2D) = "black" {}
        _CrackDepth ("Crack Depth", Range(0, 2)) = 0.8
        [Toggle] _FSIceScatter ("Fake Subsurface", Float) = 1
        _ScatterColor ("Scatter Color", Color) = (0.2, 0.65, 1, 1)
        _ScatterStrength ("Scatter Strength", Range(0, 2)) = 0.35
        [Toggle] _FSIceSparkle ("Sparkle", Float) = 0
        _SparkleStrength ("Sparkle Strength", Range(0, 1)) = 0.25
        _SparkleDistance ("Sparkle Fade Distance", Range(0.5, 30)) = 8
        _ReflectionStrength ("Reflection Strength", Range(0, 2)) = 0.65
        _IOR ("Index of Refraction", Range(1, 2.5)) = 1.31
        [Toggle] _FSVertexColor ("Use Vertex Color", Float) = 0

        [HideInInspector] _FSDebugView ("Debug View", Float) = 0
        [HideInInspector] _FSVersion ("fShader Version", Float) = 0.3
        [HideInInspector] _FSEdition ("fShader Edition", Float) = 0
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
            Cull Back ZWrite [_FSZWrite] ZTest LEqual Blend [_FSSrcBlend] [_FSDstBlend]
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
            #pragma shader_feature_local _ FSHADER_ICE_SCATTER
            #pragma shader_feature_local _ FSHADER_ICE_SPARKLE
            #pragma shader_feature_local _ FSHADER_ICE_TRANSPARENT
            #pragma shader_feature_local _ FSHADER_VERTEX_COLOR
            #define FSHADER_MODE_ICE 1
            #include "Packages/com.fshader.core/Runtime/Shaders/Includes/fShaderCommon.cginc"
            ENDCG
        }

        Pass
        {
            Name "SHADOWCASTER"
            Tags { "LightMode"="ShadowCaster" }
            ZWrite On ZTest LEqual Cull Back
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
            half4 _IceColor;
            half _IceThickness;

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
                meta.Albedo = baseSample.rgb * _BaseColor.rgb * lerp(1.0h, _IceColor.rgb, _IceThickness);
                meta.Emission = 0.0h;
                return UnityMetaFragment(meta);
            }
            ENDCG
        }
    }
    CustomEditor "fShader.Editor.fShaderInspector"
    Fallback Off
}
