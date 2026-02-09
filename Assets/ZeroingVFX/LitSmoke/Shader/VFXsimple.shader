Shader "Gumou/VFX/VFXsimple"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [HDR]_Color ("_Color", Color) = (1,1,1,1)

        [Space][Header(__________ Fade __________)][Space]
        [Toggle(_EnableDepthTexture)]_EnableDepthTexture("EnableDepthTexture", float) = 0
        _DepthFade ("_DepthFade", Range(0,3)) = 1

        [Space][Header(__________ Other __________)][Space]
        _AlphaGamma ("_AlphaGamma", Range(0,1)) = 1
        [Toggle]_BlackToAlpha ("_BlackToAlpha", float) = 0
        _CamOffset("Z Offset",float) = 0
        [Enum(UnityEngine.Rendering.BlendMode)]_BlendSrc("_BlendSrc",float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)]_BlendDst("_BlendDst",float) = 10
    }
    SubShader
    {
        LOD 100
        Tags { "RenderType"="Opaque" 
        "Queue" = "Transparent" 
        "IgnoreProjector" = "True" 
        "RenderPipeline" = "UniversalPipeline"
         }
        ZWrite Off
        Cull Off
        Blend [_BlendSrc] [_BlendDst]

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            // #pragma instancing_options procedural:ParticleInstancingSetup


            #pragma shader_feature_local_fragment _EnableDepthTexture

            #include"Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            // #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ParticlesInstancing.hlsl"



            void GetParticleTexcoords(out float2 outputTexcoord, out float3 outputTexcoord2AndBlend, in float4 inputTexcoords, in float inputBlend)
            {
            #if defined(UNITY_PARTICLE_INSTANCING_ENABLED)
                if (unity_ParticleUVShiftData.x != 0.0)
                {
                    UNITY_PARTICLE_INSTANCE_DATA data = unity_ParticleInstanceData[unity_InstanceID];

                    float numTilesX = unity_ParticleUVShiftData.y;
                    float2 animScale = unity_ParticleUVShiftData.zw;
            #ifdef UNITY_PARTICLE_INSTANCE_DATA_NO_ANIM_FRAME
                    float sheetIndex = 0.0;
            #else
                    float sheetIndex = data.animFrame;
            #endif

                    float index0 = floor(sheetIndex);
                    float vIdx0 = floor(index0 / numTilesX);
                    float uIdx0 = floor(index0 - vIdx0 * numTilesX);
                    float2 offset0 = float2(uIdx0 * animScale.x, (1.0 - animScale.y) - vIdx0 * animScale.y); // Copied from built-in as is and it looks like upside-down flip

                    outputTexcoord = inputTexcoords.xy * animScale.xy + offset0.xy;

            #ifdef _FLIPBOOKBLENDING_ON
                    float index1 = floor(sheetIndex + 1.0);
                    float vIdx1 = floor(index1 / numTilesX);
                    float uIdx1 = floor(index1 - vIdx1 * numTilesX);
                    float2 offset1 = float2(uIdx1 * animScale.x, (1.0 - animScale.y) - vIdx1 * animScale.y);

                    outputTexcoord2AndBlend.xy = inputTexcoords.xy * animScale.xy + offset1.xy;
                    outputTexcoord2AndBlend.z = frac(sheetIndex);
            #endif
                }
                else
            #endif
                {
                    outputTexcoord = inputTexcoords.xy;
            #ifdef _FLIPBOOKBLENDING_ON
                    outputTexcoord2AndBlend.xy = inputTexcoords.zw;
                    outputTexcoord2AndBlend.z = inputBlend;
            #endif
                }

            #ifndef _FLIPBOOKBLENDING_ON
                outputTexcoord2AndBlend.xy = inputTexcoords.xy;
                outputTexcoord2AndBlend.z = 0.5;
            #endif
            }
            void GetParticleTexcoords(out float2 outputTexcoord, in float2 inputTexcoord)
            {
                float3 dummyTexcoord2AndBlend = 0.0;
                GetParticleTexcoords(outputTexcoord, dummyTexcoord2AndBlend, inputTexcoord.xyxy, 0.0);
            }
            half4 GetParticleColor(half4 color)
            {
            #if defined(UNITY_PARTICLE_INSTANCING_ENABLED)
            #if !defined(UNITY_PARTICLE_INSTANCE_DATA_NO_COLOR)
                UNITY_PARTICLE_INSTANCE_DATA data = unity_ParticleInstanceData[unity_InstanceID];
                color = lerp(half4(1.0, 1.0, 1.0, 1.0), color, unity_ParticleUseMeshColors);
                color *= half4(UnpackFromR8G8B8A8(data.color));
            #endif
            #endif
                return color;
            }
            float GetDepth(float4 projPos){
                float depthTex = SampleSceneDepth(projPos.xy/ projPos.w);
                if(unity_OrthoParams.w == 1)
                {
                    #if defined(UNITY_REVERSED_Z)
                    #if UNITY_REVERSED_Z == 1
                    depthTex = 1.0f - depthTex;
                    #endif
                    #endif
                    return lerp(_ProjectionParams.y, _ProjectionParams.z, depthTex);
                }
                return LinearEyeDepth(depthTex , _ZBufferParams);
            }
            float DepthFadeS(float4 projPos,float fadeStrength){
                #ifdef _EnableDepthTexture
                float sceneZ = GetDepth(projPos);
                float partZ = projPos.z;
                float fade = saturate((sceneZ - partZ)/fadeStrength);
                return fade;
                #else
                return 1;
                #endif
            }

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID

            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : TEXCOORD1;
                float4 projPos : TEXCOORD4;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO

            };

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _MainTex_ST;
                float _DepthFade;
                float _CamOffset;
                float _BlackToAlpha;
                float _AlphaGamma;
            CBUFFER_END
            sampler2D _MainTex;


            v2f vert (appdata v)
            {
                v2f o = (v2f)0;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                // cam offset/
                float3 viewDirLoc= normalize( mul(unity_WorldToObject,float4(_WorldSpaceCameraPos,1)) -  v.vertex); 
                float3 camDir = mul(unity_WorldToObject,float4(UNITY_MATRIX_V[2].xyz,0));
                float dotdot = dot(viewDirLoc, camDir);
                viewDirLoc /= dotdot;
                v.vertex.xyz += viewDirLoc*_CamOffset;

                o.vertex = TransformObjectToHClip(v.vertex);
                GetParticleTexcoords(o.uv, v.uv);
                o.color = GetParticleColor(v.color);
                o.projPos = ComputeScreenPos (o.vertex);
                o.projPos.z = -TransformWorldToView( mul(unity_ObjectToWorld,v.vertex) ).z;
                return o;
            }



            half4 frag (v2f i) : SV_Target{

                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                //depth fade
                float depth = DepthFadeS(i.projPos,_DepthFade);
                depth = saturate(depth);
                i.color.a *= depth;

                half4 col = tex2D(_MainTex, i.uv);
                col = lerp(col,half4(1,1,1,col.r),_BlackToAlpha);
                col *= _Color * i.color;
                
                col.a = lerp(col.a,col.a*col.a,_AlphaGamma) ;
                // #ifdef UNITY_COLORSPACE_GAMMA
                //     col.a = col.a;
                // #else
                //     col.a = col.a * col.a;
                // #endif
                return col;
            }
            ENDHLSL
        }
    }
}
