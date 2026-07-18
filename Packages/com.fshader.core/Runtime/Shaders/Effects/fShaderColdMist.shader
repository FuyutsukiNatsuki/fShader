Shader "fShader/Effects/ColdMist"
{
    Properties
    {
        [HDR] _TintColor ("Mist Color", Color) = (0.45, 0.8, 1, 0.32)
        _Opacity ("Opacity", Range(0, 1)) = 0.55
        _EdgePower ("Edge Softness", Range(0.5, 6)) = 2.2
    }
    SubShader
    {
        Tags { "Queue"="Transparent+10" "RenderType"="Transparent" "IgnoreProjector"="True" "PreviewType"="Plane" }
        Pass
        {
            Name "FORWARD"
            Cull Off ZWrite Off ZTest LEqual Blend One OneMinusSrcAlpha
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            half4 _TintColor;
            half _Opacity;
            half _EdgePower;

            struct appdata
            {
                float4 vertex : POSITION;
                half4 color : COLOR;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                half4 color : COLOR0;
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert(appdata input)
            {
                v2f output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_OUTPUT(v2f, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.pos = UnityObjectToClipPos(input.vertex);
                output.color = input.color * _TintColor;
                output.uv = input.uv;
                UNITY_TRANSFER_FOG(output, output.pos);
                return output;
            }

            half4 frag(v2f input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                half2 centered = input.uv * 2.0h - 1.0h;
                half radial = saturate(1.0h - dot(centered, centered));
                half alpha = pow(radial, _EdgePower) * input.color.a * _Opacity;
                half4 color = half4(input.color.rgb * alpha, alpha);
                half4 premultipliedFog = half4(unity_FogColor.rgb * alpha, alpha);
                UNITY_APPLY_FOG_COLOR(input.fogCoord, color, premultipliedFog);
                return color;
            }
            ENDCG
        }
    }
    Fallback Off
}
