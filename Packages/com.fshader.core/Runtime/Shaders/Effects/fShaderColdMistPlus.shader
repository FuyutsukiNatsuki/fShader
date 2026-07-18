Shader "fShader/Effects/ColdMistPlus"
{
    Properties
    {
        [HDR] _TintColor ("Mist Color", Color) = (0.55, 0.86, 1, 0.34)
        _Opacity ("Opacity", Range(0, 1)) = 0.62
        _EdgePower ("Edge Softness", Range(0.5, 6)) = 2.4
        _NoiseScale ("Noise Scale", Range(0.5, 8)) = 2.2
        _NoiseStrength ("Noise Strength", Range(0, 1)) = 0.32
        _FlowSpeed ("Flow Speed", Vector) = (0.035, 0.07, 0, 0)
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
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"

            half4 _TintColor;
            half _Opacity;
            half _EdgePower;
            half _NoiseScale;
            half _NoiseStrength;
            half4 _FlowSpeed;

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
                half2 flowUV = (input.uv + _FlowSpeed.xy * _Time.y) * _NoiseScale;
                half waveA = sin((flowUV.x + flowUV.y * 0.71h) * 6.28318h);
                half waveB = cos((flowUV.y - flowUV.x * 0.43h) * 4.71239h);
                half noise = saturate(0.5h + (waveA + waveB) * 0.19h);
                half modulation = lerp(1.0h, noise, _NoiseStrength);
                half alpha = pow(radial, _EdgePower) * modulation * input.color.a * _Opacity;
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
