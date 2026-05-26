Shader "Custom/SpriteDissolve"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Glow Color", Color) = (0, 3.12, 4.0, 1.0)
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _NoiseTiling ("Noise Tiling", Vector) = (5, 5, 0, 0)
        _Dissolve_Amount ("Dissolve Amount", Range(0, 1)) = 0.0
        _EdgeWidth ("Edge Width", Range(0, 0.2)) = 0.05
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
                float2 localPos : TEXCOORD1; // Store local object space coordinates
                UNITY_VERTEX_OUTPUT_STEREO
            };

            fixed4 _Color;
            sampler2D _MainTex;
            sampler2D _NoiseTex;
            float4 _NoiseTiling;
            float _Dissolve_Amount;
            float _EdgeWidth;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.localPos = IN.vertex.xy; // Store local vertex coordinate before clip projection
                OUT.color = IN.color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 spriteColor = tex2D(_MainTex, IN.texcoord) * IN.color;
                
                // Sample noise using local object-space positions scaled by tiling
                float2 noiseUV = IN.localPos * _NoiseTiling.xy;
                float noise = tex2D(_NoiseTex, noiseUV).r;
                
                // Dissolve threshold mapping
                float threshold = _Dissolve_Amount;
                
                // If noise is below the threshold, discard the pixel
                if (noise < threshold)
                    discard;

                // Glowing edge calculations
                float edgeCheck = threshold + _EdgeWidth;
                if (noise < edgeCheck)
                {
                    float edgeLerp = (edgeCheck - noise) / _EdgeWidth;
                    fixed4 glow = _Color * edgeLerp;
                    spriteColor.rgb = lerp(spriteColor.rgb, glow.rgb, edgeLerp);
                }

                spriteColor.rgb *= spriteColor.a;
                return spriteColor;
            }
        ENDCG
        }
    }
}
