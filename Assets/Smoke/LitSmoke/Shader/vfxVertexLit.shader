Shader "Gumou/VFX/vfxVertexLit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("_Color", Color) = (1,1,1,1)

        _Transmittance("_Transmittance ", Range(0,1)) = 0
        _MinDark ("_MinDark", Range(0,0.7)) = 0
        _MaxLit ("_MaxLit", float) = 9

        [Space][Header(__________ Fade __________)][Space]
        [Toggle(_EnableDepthTexture)]_EnableDepthTexture("EnableDepthTexture", float) = 0
        _DepthFade ("_DepthFade", Range(0,3)) = 1


        [Space][Header(__________ Rendering __________)][Space]
        [Toggle(_UseVertexLights)]_UseVertexLights("_UseVertexLights", float) = 1
        [IntRange]_LightCount ("_LightCount", Range(2,8)) = 4
        _AlphaGamma ("_AlphaGamma", Range(0,1)) = 1
        [Toggle]_BlackToAlpha ("_BlackToAlpha", float) = 0
        [Enum(UnityEngine.Rendering.CullMode)]_CullMode ("CullMode", float) = 2


    }
    SubShader
    {
        LOD 100
        Tags { "RenderType"="Opaque" 
        "Queue" = "Transparent" 
        "IgnoreProjector" = "True" 
        "RenderPipeline" = "UniversalPipeline"
         }
        Cull [_CullMode]
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            // #pragma instancing_options procedural:ParticleInstancingSetup


            #pragma shader_feature_local _UseVertexLights
            #pragma shader_feature_local_fragment _EnableDepthTexture

            #include"Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"
            // #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ParticlesInstancing.hlsl"


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
            
            struct appdata{
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                float4 color :COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID

            };
            struct v2f{
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
                float4 color : TEXCOORD3;
                float4 projPos : TEXCOORD4;
                float3 vertLight : TEXCOORD5;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            float4 _Color;
            float _Transmittance;
            float _BlackToAlpha;
            float _DepthFade;
            float _AlphaGamma;
            float _MinDark;
            float _MaxLit;
            float _LightCount;
            CBUFFER_END

            sampler2D _MainTex;
           
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

            half3 LightingLambert2( half3 attenuatedLightColor,half3 lightDir, half3 normal){
                half NdotL= dot(normal, lightDir);
                if(NdotL<0){
                    NdotL*=-1;
                    NdotL*=_Transmittance;
                }
                NdotL = saturate(NdotL);
                NdotL = lerp(_MinDark,1,NdotL);
                return NdotL * attenuatedLightColor;
            }
            
            half3 AdditionalLighting(float3 positionWS, half3 normalWS){
                half3 col = half3(0.0, 0.0, 0.0);
                // uint lightsCount = GetAdditionalLightsCount();
                uint lightsCount = _LightCount;
                for (uint lightIndex = 0u; lightIndex < lightsCount; ++lightIndex)
                {
                    Light light = GetAdditionalLight(lightIndex, positionWS);
                    light.distanceAttenuation = min(_MaxLit,light.distanceAttenuation);
                    half3 lightColor = light.color * light.distanceAttenuation;
                    col += LightingLambert2(lightColor, light.direction, normalWS);
                }
                return col;
            }
            half3 VertexLighting2(float3 positionWS, half3 normalWS)
            {
                half3 vertexLightColor = half3(0.0, 0.0, 0.0);

            #ifdef _UseVertexLights
                vertexLightColor += AdditionalLighting(positionWS,normalWS);
            #endif

                return vertexLightColor;
            }
                        


            v2f vert (appdata v)
            {
                v2f o = (v2f)0;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                // VertexPositionInputs vertexInput = GetVertexPositionInputs(v.vertex.xyz);
                // o.pos = vertexInput.positionCS;
                // o.projPos = vertexInput.positionNDC;
                // o.worldPos = float4(vertexInput.positionWS,1);
                // GetParticleTexcoords(o.uv, v.uv);
                // o.color = GetParticleColor(v.color);
                // o.worldNormal = TransformObjectToWorldNormal(v.normal);
                // o.vertLight = VertexLighting2(o.worldPos,o.worldNormal);

                o.pos = TransformObjectToHClip(v.vertex);
                o.worldPos = TransformObjectToWorld(v.vertex);
                o.uv =v.uv;
                o.color = v.color;
                o.worldNormal = TransformObjectToWorldNormal(v.normal);
                o.projPos = ComputeScreenPos (o.pos);
                o.projPos.z = -TransformWorldToView( mul(unity_ObjectToWorld,v.vertex) ).z;
                o.vertLight = VertexLighting2(o.worldPos,o.worldNormal);


                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                float3 normal = normalize(i.worldNormal);

                //depth fade
                float depth = DepthFadeS(i.projPos,_DepthFade);
                depth = saturate(depth);
                i.color.a *= depth;
                
                //main light
                Light mainLight = GetMainLight();
                half3 finalLight = LightingLambert2(mainLight.color,mainLight.direction, normal);
                //add light
                finalLight += i.vertLight;
                #ifndef _UseVertexLights
                    finalLight += AdditionalLighting(i.worldPos,normal);
                #endif
                //env light
                half4 col = tex2D(_MainTex, i.uv);
                col = lerp(col,half4(1,1,1,col.r),_BlackToAlpha);
                half3 diffuseEnvCol = SampleSHVertex(normal) * col;
                finalLight += diffuseEnvCol;

                col *= half4(finalLight.rgb,1);
                col *= _Color * i.color;
                col.a = lerp(col.a,col.a*col.a,_AlphaGamma) ;
                return col;
            }
            ENDHLSL
        }


        // // ------------------------------------------------------------------
        // //  Scene view outline pass.
        // Pass
        // {
        //     Name "SceneSelectionPass"
        //     Tags { "LightMode" = "SceneSelectionPass" }

        //     BlendOp Add
        //     Blend One Zero
        //     ZWrite On
        //     Cull Off

        //     HLSLPROGRAM
        //     #define PARTICLES_EDITOR_META_PASS
        //     #pragma target 2.0

        //     // -------------------------------------
        //     // Particle Keywords
        //     #pragma shader_feature_local_fragment _ALPHATEST_ON
        //     #pragma shader_feature_local _FLIPBOOKBLENDING_ON

        //     // -------------------------------------
        //     // Unity defined keywords
        //     #pragma multi_compile_instancing
        //     #pragma instancing_options procedural:ParticleInstancingSetup

        //     #pragma vertex vertParticleEditor
        //     #pragma fragment fragParticleSceneHighlight

        //     #include "Packages/com.unity.render-pipelines.universal/Shaders/Particles/ParticlesUnlitInput.hlsl"
        //     #include "Packages/com.unity.render-pipelines.universal/Shaders/Particles/ParticlesEditorPass.hlsl"

        //     ENDHLSL
        // }
        // // ------------------------------------------------------------------
        // //  Scene picking buffer pass.
        // Pass
        // {
        //     Name "ScenePickingPass"
        //     Tags{ "LightMode" = "Picking" }

        //     BlendOp Add
        //     Blend One Zero
        //     ZWrite On
        //     Cull Off

        //     HLSLPROGRAM
        //     #define PARTICLES_EDITOR_META_PASS
        //     #pragma target 2.0

        //     // -------------------------------------
        //     // Particle Keywords
        //     #pragma shader_feature_local_fragment _ALPHATEST_ON
        //     #pragma shader_feature_local _FLIPBOOKBLENDING_ON

        //     // -------------------------------------
        //     // Unity defined keywords
        //     #pragma multi_compile_instancing
        //     #pragma instancing_options procedural:ParticleInstancingSetup

        //     #pragma vertex vertParticleEditor
        //     #pragma fragment fragParticleScenePicking

        //     #include "Packages/com.unity.render-pipelines.universal/Shaders/Particles/ParticlesUnlitInput.hlsl"
        //     #include "Packages/com.unity.render-pipelines.universal/Shaders/Particles/ParticlesEditorPass.hlsl"

        //     ENDHLSL
        // }
    }
}
