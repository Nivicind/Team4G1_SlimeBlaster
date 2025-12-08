Shader "Custom/WaterWaveSprite"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        // Wave properties
        _WaveAmplitude ("Wave Amplitude", Range(0, 0.5)) = 0.1
        _WaveFrequency ("Wave Frequency", Range(0, 20)) = 5.0
        _WaveSpeed ("Wave Speed", Range(0, 10)) = 1.0
        
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile_local _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            sampler2D _AlphaTex;
            fixed4 _Color;
            fixed4 _RendererColor;
            float4 _MainTex_ST;
            
            // Wave parameters
            float _WaveAmplitude;
            float _WaveFrequency;
            float _WaveSpeed;

            v2f vert(appdata_t IN)
            {
                v2f OUT;

                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                // Calculate wave offset for vertices near the top
                // UV.y ranges from 0 (bottom) to 1 (top)
                float waveInfluence = smoothstep(0.5, 1.0, IN.texcoord.y);
                
                // Calculate sine wave based on position and time
                float wave = sin((IN.vertex.x * _WaveFrequency) + (_Time.y * _WaveSpeed)) * _WaveAmplitude;
                
                // Apply wave offset only to Y position, scaled by how close to top edge
                IN.vertex.y += wave * waveInfluence;

                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = TRANSFORM_TEX(IN.texcoord, _MainTex);
                OUT.color = IN.color * _Color * _RendererColor;

                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap(OUT.vertex);
                #endif

                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;
                
                #if ETC1_EXTERNAL_ALPHA
                fixed4 alpha = tex2D(_AlphaTex, IN.texcoord);
                c.a = lerp(c.a, alpha.r, _EnableExternalAlpha);
                #endif

                c.rgb *= c.a;
                return c;
            }
            ENDCG
        }
    }
}
