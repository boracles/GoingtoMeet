Shader "Water Ripples 1"
{
    Properties
    {
        Vector1_E5125837("Strengh", Float) = 2
        Vector1_C7406269("Speed", Float) = 1
        Vector1_76A233A4("Tiling", Float) = 5
        Vector1_5327F7C2("Depth", Float) = 0
        Vector1_2FA69C8F("Water Color Offset", Float) = 0
        Color_C343EF87("Color", Color) = (0, 0.5741674, 0.6603774, 0)
        [HideInInspector]_BUILTIN_QueueOffset("Float", Float) = 0
        [HideInInspector]_BUILTIN_QueueControl("Float", Float) = -1
    }
    SubShader
    {
        Tags
        {
            // RenderPipeline: <None>
            "RenderType"="Transparent"
            "BuiltInMaterialType" = "Lit"
            "Queue"="Transparent"
            "ShaderGraphShader"="true"
            "ShaderGraphTargetId"="BuiltInLitSubTarget"
        }
        GrabPass { "_GrabTexture" }
        Pass
        {
            Name "BuiltIn Forward"
            Tags
            {
                "LightMode" = "ForwardBase"
            }
        
        // Render State
        Cull Back
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZTest LEqual
        ZWrite Off
        ColorMask RGB
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
sampler2D _GrabTexture;
        
        // Pragmas
        #pragma target 3.0
        #pragma multi_compile_instancing
        #pragma multi_compile_fog
        #pragma multi_compile_fwdbase
        #pragma vertex vert
        #pragma fragment frag
        
        // DotsInstancingOptions: <None>
        // HybridV1InjectedBuiltinProperties: <None>
        
        // Keywords
        #pragma multi_compile _ _SCREEN_SPACE_OCCLUSION
        #pragma multi_compile _ LIGHTMAP_ON
        #pragma multi_compile _ DIRLIGHTMAP_COMBINED
        #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
        #pragma multi_compile _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS _ADDITIONAL_OFF
        #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
        #pragma multi_compile _ _SHADOWS_SOFT
        #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
        #pragma multi_compile _ SHADOWS_SHADOWMASK
        // GraphKeywords: <None>
        
        // Defines
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define ATTRIBUTES_NEED_TEXCOORD1
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_NORMAL_WS
        #define VARYINGS_NEED_TANGENT_WS
        #define VARYINGS_NEED_TEXCOORD0
        #define VARYINGS_NEED_VIEWDIRECTION_WS
        #define VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_FORWARD
        #define BUILTIN_TARGET_API 1
        #define _BUILTIN_SURFACE_TYPE_TRANSPARENT 1
        #define REQUIRE_DEPTH_TEXTURE
        #define REQUIRE_OPAQUE_TEXTURE
        /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */
        #ifdef _BUILTIN_SURFACE_TYPE_TRANSPARENT
        #define _SURFACE_TYPE_TRANSPARENT _BUILTIN_SURFACE_TYPE_TRANSPARENT
        #endif
        #ifdef _BUILTIN_ALPHATEST_ON
        #define _ALPHATEST_ON _BUILTIN_ALPHATEST_ON
        #endif
        #ifdef _BUILTIN_AlphaClip
        #define _AlphaClip _BUILTIN_AlphaClip
        #endif
        #ifdef _BUILTIN_ALPHAPREMULTIPLY_ON
        #define _ALPHAPREMULTIPLY_ON _BUILTIN_ALPHAPREMULTIPLY_ON
        #endif
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/Shim/Shims.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/LegacySurfaceVertex.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/ShaderGraphFunctions.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
             float4 uv1 : TEXCOORD1;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 positionWS;
             float3 normalWS;
             float4 tangentWS;
             float4 texCoord0;
             float3 viewDirectionWS;
            #if defined(LIGHTMAP_ON)
             float2 lightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
             float3 sh;
            #endif
             float4 fogFactorAndVertexLight;
             float4 shadowCoord;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float3 WorldSpaceNormal;
             float3 TangentSpaceNormal;
             float3 WorldSpaceTangent;
             float3 WorldSpaceBiTangent;
             float3 WorldSpacePosition;
             float4 ScreenPosition;
             float4 uv0;
             float3 TimeParameters;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
            #if defined(LIGHTMAP_ON)
             float2 lightmapUV : INTERP0;
            #endif
            #if !defined(LIGHTMAP_ON)
             float3 sh : INTERP1;
            #endif
             float4 tangentWS : INTERP2;
             float4 texCoord0 : INTERP3;
             float4 fogFactorAndVertexLight : INTERP4;
             float4 shadowCoord : INTERP5;
             float3 positionWS : INTERP6;
             float3 normalWS : INTERP7;
             float3 viewDirectionWS : INTERP8;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            #if defined(LIGHTMAP_ON)
            output.lightmapUV = input.lightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
            output.sh = input.sh;
            #endif
            output.tangentWS.xyzw = input.tangentWS;
            output.texCoord0.xyzw = input.texCoord0;
            output.fogFactorAndVertexLight.xyzw = input.fogFactorAndVertexLight;
            output.shadowCoord.xyzw = input.shadowCoord;
            output.positionWS.xyz = input.positionWS;
            output.normalWS.xyz = input.normalWS;
            output.viewDirectionWS.xyz = input.viewDirectionWS;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            #if defined(LIGHTMAP_ON)
            output.lightmapUV = input.lightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
            output.sh = input.sh;
            #endif
            output.tangentWS = input.tangentWS.xyzw;
            output.texCoord0 = input.texCoord0.xyzw;
            output.fogFactorAndVertexLight = input.fogFactorAndVertexLight.xyzw;
            output.shadowCoord = input.shadowCoord.xyzw;
            output.positionWS = input.positionWS.xyz;
            output.normalWS = input.normalWS.xyz;
            output.viewDirectionWS = input.viewDirectionWS.xyz;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float Vector1_E5125837;
        float Vector1_C7406269;
        float Vector1_76A233A4;
        float Vector1_5327F7C2;
        float Vector1_2FA69C8F;
        float4 Color_C343EF87;
        CBUFFER_END
        
        // Object and Global properties
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // Graph Functions
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
        Out = A * B;
        }
        
        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        void Unity_Floor_float2(float2 In, out float2 Out)
        {
            Out = floor(In);
        }
        
        void Unity_RandomRange_float(float2 Seed, float Min, float Max, out float Out)
        {
             float randomno =  frac(sin(dot(Seed, float2(12.9898, 78.233)))*43758.5453);
             Out = lerp(Min, Max, randomno);
        }
        
        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }
        
        void Unity_Floor_float(float In, out float Out)
        {
            Out = floor(In);
        }
        
        void Unity_Add_float2(float2 A, float2 B, out float2 Out)
        {
            Out = A + B;
        }
        
        void Unity_Fraction_float2(float2 In, out float2 Out)
        {
            Out = frac(In);
        }
        
        void Unity_Distance_float2(float2 A, float2 B, out float Out)
        {
            Out = distance(A, B);
        }
        
        void Unity_Fraction_float(float In, out float Out)
        {
            Out = frac(In);
        }
        
        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }
        
        void Unity_SampleGradientV0_float(Gradient Gradient, float Time, out float4 Out)
        {
            float3 color = Gradient.colors[0].rgb;
            [unroll]
            for (int c = 1; c < Gradient.colorsLength; c++)
            {
                float colorPos = saturate((Time - Gradient.colors[c - 1].w) / (Gradient.colors[c].w - Gradient.colors[c - 1].w)) * step(c, Gradient.colorsLength - 1);
                color = lerp(color, Gradient.colors[c].rgb, lerp(colorPos, step(0.01, colorPos), Gradient.type));
            }
        #ifndef UNITY_COLORSPACE_GAMMA
            color = SRGBToLinear(color);
        #endif
            float alpha = Gradient.alphas[0].x;
            [unroll]
            for (int a = 1; a < Gradient.alphasLength; a++)
            {
                float alphaPos = saturate((Time - Gradient.alphas[a - 1].y) / (Gradient.alphas[a].y - Gradient.alphas[a - 1].y)) * step(a, Gradient.alphasLength - 1);
                alpha = lerp(alpha, Gradient.alphas[a].x, lerp(alphaPos, step(0.01, alphaPos), Gradient.type));
            }
            Out = float4(color, alpha);
        }
        
        void Unity_Lerp_float4(float4 A, float4 B, float4 T, out float4 Out)
        {
            Out = lerp(A, B, T);
        }
        
        void Unity_NormalFromHeight_Tangent_float(float In, float Strength, float3 Position, float3x3 TangentMatrix, out float3 Out)
        {
        
                    #if defined(SHADER_STAGE_RAY_TRACING) && defined(RAYTRACING_SHADER_GRAPH_DEFAULT)
                    #error 'Normal From Height' node is not supported in ray tracing, please provide an alternate implementation, relying for instance on the 'Raytracing Quality' keyword
                    #endif
        float3 worldDerivativeX = ddx(Position);
        float3 worldDerivativeY = ddy(Position);
        
        float3 crossX = cross(TangentMatrix[2].xyz, worldDerivativeX);
        float3 crossY = cross(worldDerivativeY, TangentMatrix[2].xyz);
        float d = dot(worldDerivativeX, crossY);
        float sgn = d < 0.0 ? (-1.0f) : 1.0f;
        float surface = sgn / max(0.000000000000001192093f, abs(d));
        
        float dHdx = ddx(In);
        float dHdy = ddy(In);
        float3 surfGrad = surface * (dHdx*crossY + dHdy*crossX);
        Out = SafeNormalize(TangentMatrix[2].xyz - (Strength * surfGrad));
        Out = TransformWorldToTangent(Out, TangentMatrix);
        }
        
        void Unity_NormalStrength_float(float3 In, float Strength, out float3 Out)
        {
            Out = float3(In.rg * Strength, lerp(1, In.b, saturate(Strength)));
        }
        
        struct Bindings_RipplesSubGraph_8c331c1756d2a9b47b80805d5e9b5760_float
        {
        float3 WorldSpaceNormal;
        float3 WorldSpaceTangent;
        float3 WorldSpaceBiTangent;
        float3 WorldSpacePosition;
        half4 uv0;
        };
        
        void SG_RipplesSubGraph_8c331c1756d2a9b47b80805d5e9b5760_float(float Vector1_A606AD0F, Gradient Gradient_AFB6B065, Gradient Gradient_612A0EE6, float Vector1_38564B71, float Vector1_48991907, float Vector1_E03EF315, float2 Vector2_8FC8A8DE, Bindings_RipplesSubGraph_8c331c1756d2a9b47b80805d5e9b5760_float IN, out float3 OutVector3_1)
        {
        Gradient _Property_9bccf34d24839482816c85b42874663b_Out_0 = Gradient_612A0EE6;
        float _Property_89d96fee4ae9b58596cca58f9ca66d82_Out_0 = Vector1_38564B71;
        float _Property_e7381c82bf07f28e87e55268633f03a6_Out_0 = Vector1_48991907;
        float _Multiply_2b7f465bf86b238ba88c3abadfb4dc6c_Out_2;
        Unity_Multiply_float_float(_Property_89d96fee4ae9b58596cca58f9ca66d82_Out_0, _Property_e7381c82bf07f28e87e55268633f03a6_Out_0, _Multiply_2b7f465bf86b238ba88c3abadfb4dc6c_Out_2);
        float _Property_c9b401be3cccc487963a18b18a5b00c5_Out_0 = Vector1_E03EF315;
        float2 _Property_7fd9512f128cc584b63bd9ba93e3805b_Out_0 = Vector2_8FC8A8DE;
        float2 _TilingAndOffset_054c12590944d280835bf7af8fbcbb8a_Out_3;
        Unity_TilingAndOffset_float(IN.uv0.xy, (_Property_c9b401be3cccc487963a18b18a5b00c5_Out_0.xx), _Property_7fd9512f128cc584b63bd9ba93e3805b_Out_0, _TilingAndOffset_054c12590944d280835bf7af8fbcbb8a_Out_3);
        float2 _Floor_b11cd06caee6518891d995cc8a1ef27a_Out_1;
        Unity_Floor_float2(_TilingAndOffset_054c12590944d280835bf7af8fbcbb8a_Out_3, _Floor_b11cd06caee6518891d995cc8a1ef27a_Out_1);
        float _RandomRange_715aacb97fba9b858184047a525d5dab_Out_3;
        Unity_RandomRange_float(_Floor_b11cd06caee6518891d995cc8a1ef27a_Out_1, float(0), float(1), _RandomRange_715aacb97fba9b858184047a525d5dab_Out_3);
        float _Add_66f573fa287cb288ba1bc9cd69e16fda_Out_2;
        Unity_Add_float(_Multiply_2b7f465bf86b238ba88c3abadfb4dc6c_Out_2, _RandomRange_715aacb97fba9b858184047a525d5dab_Out_3, _Add_66f573fa287cb288ba1bc9cd69e16fda_Out_2);
        float _Floor_751a8067250b49808f479855f31ee1a7_Out_1;
        Unity_Floor_float(_Add_66f573fa287cb288ba1bc9cd69e16fda_Out_2, _Floor_751a8067250b49808f479855f31ee1a7_Out_1);
        float2 _Add_1842fa0e1536f388b8a6d5f64e3e062f_Out_2;
        Unity_Add_float2((_Floor_751a8067250b49808f479855f31ee1a7_Out_1.xx), _Floor_b11cd06caee6518891d995cc8a1ef27a_Out_1, _Add_1842fa0e1536f388b8a6d5f64e3e062f_Out_2);
        float _RandomRange_d7266f27a6b9958f837bf09bdc762cdb_Out_3;
        Unity_RandomRange_float(_Add_1842fa0e1536f388b8a6d5f64e3e062f_Out_2, float(0.3), float(0.7), _RandomRange_d7266f27a6b9958f837bf09bdc762cdb_Out_3);
        float2 _Add_d47dc767dcea72828a6821b1dc7669b0_Out_2;
        Unity_Add_float2(_Add_1842fa0e1536f388b8a6d5f64e3e062f_Out_2, float2(1, 1), _Add_d47dc767dcea72828a6821b1dc7669b0_Out_2);
        float _RandomRange_fb667fb6d6ea1f878b072b6a0b019718_Out_3;
        Unity_RandomRange_float(_Add_d47dc767dcea72828a6821b1dc7669b0_Out_2, float(0.3), float(0.7), _RandomRange_fb667fb6d6ea1f878b072b6a0b019718_Out_3);
        float2 _Vector2_527e5a6aebcea4839bc8a823f63125be_Out_0 = float2(_RandomRange_d7266f27a6b9958f837bf09bdc762cdb_Out_3, _RandomRange_fb667fb6d6ea1f878b072b6a0b019718_Out_3);
        float2 _Fraction_722b542af4cde88480600980a547170a_Out_1;
        Unity_Fraction_float2(_TilingAndOffset_054c12590944d280835bf7af8fbcbb8a_Out_3, _Fraction_722b542af4cde88480600980a547170a_Out_1);
        float _Distance_6bfa0e2d94ebc48bbd4fde991bc032fa_Out_2;
        Unity_Distance_float2(_Vector2_527e5a6aebcea4839bc8a823f63125be_Out_0, _Fraction_722b542af4cde88480600980a547170a_Out_1, _Distance_6bfa0e2d94ebc48bbd4fde991bc032fa_Out_2);
        float _Fraction_4bcfd9567d3dca85be4e0af521de8a7c_Out_1;
        Unity_Fraction_float(_Add_66f573fa287cb288ba1bc9cd69e16fda_Out_2, _Fraction_4bcfd9567d3dca85be4e0af521de8a7c_Out_1);
        float _Add_9355d1e3e12bcd8183d6a28fbd898cb2_Out_2;
        Unity_Add_float(_Fraction_4bcfd9567d3dca85be4e0af521de8a7c_Out_1, float(-0.3), _Add_9355d1e3e12bcd8183d6a28fbd898cb2_Out_2);
        float _Subtract_faef160017a9c983ad052cbecc70feda_Out_2;
        Unity_Subtract_float(_Distance_6bfa0e2d94ebc48bbd4fde991bc032fa_Out_2, _Add_9355d1e3e12bcd8183d6a28fbd898cb2_Out_2, _Subtract_faef160017a9c983ad052cbecc70feda_Out_2);
        float4 _SampleGradient_8d5fe46189810380ac3a508fe44b59d0_Out_2;
        Unity_SampleGradientV0_float(_Property_9bccf34d24839482816c85b42874663b_Out_0, _Subtract_faef160017a9c983ad052cbecc70feda_Out_2, _SampleGradient_8d5fe46189810380ac3a508fe44b59d0_Out_2);
        Gradient _Property_0c08dbb904c8cb84ae02c68bb638aded_Out_0 = Gradient_AFB6B065;
        float4 _SampleGradient_616374cca7d265828808200a0609432b_Out_2;
        Unity_SampleGradientV0_float(_Property_0c08dbb904c8cb84ae02c68bb638aded_Out_0, _Fraction_4bcfd9567d3dca85be4e0af521de8a7c_Out_1, _SampleGradient_616374cca7d265828808200a0609432b_Out_2);
        float4 _Lerp_8a47fa5e2810ac80858fc635b9f6b924_Out_3;
        Unity_Lerp_float4(float4(1, 1, 1, 1), _SampleGradient_8d5fe46189810380ac3a508fe44b59d0_Out_2, _SampleGradient_616374cca7d265828808200a0609432b_Out_2, _Lerp_8a47fa5e2810ac80858fc635b9f6b924_Out_3);
        float3 _NormalFromHeight_cb5f128a1f05298fb493e02ae043a713_Out_1;
        float3x3 _NormalFromHeight_cb5f128a1f05298fb493e02ae043a713_TangentMatrix = float3x3(IN.WorldSpaceTangent, IN.WorldSpaceBiTangent, IN.WorldSpaceNormal);
        float3 _NormalFromHeight_cb5f128a1f05298fb493e02ae043a713_Position = IN.WorldSpacePosition;
        Unity_NormalFromHeight_Tangent_float((_Lerp_8a47fa5e2810ac80858fc635b9f6b924_Out_3).x,float(0.01),_NormalFromHeight_cb5f128a1f05298fb493e02ae043a713_Position,_NormalFromHeight_cb5f128a1f05298fb493e02ae043a713_TangentMatrix, _NormalFromHeight_cb5f128a1f05298fb493e02ae043a713_Out_1);
        float _Property_2c37c06e20892a80b173de7ffa4ab033_Out_0 = Vector1_A606AD0F;
        float3 _NormalStrength_bf4bdc1d176af585801fc908c9d75602_Out_2;
        Unity_NormalStrength_float(_NormalFromHeight_cb5f128a1f05298fb493e02ae043a713_Out_1, _Property_2c37c06e20892a80b173de7ffa4ab033_Out_0, _NormalStrength_bf4bdc1d176af585801fc908c9d75602_Out_2);
        OutVector3_1 = _NormalStrength_bf4bdc1d176af585801fc908c9d75602_Out_2;
        }
        
        void Unity_NormalBlend_float(float3 A, float3 B, out float3 Out)
        {
            Out = SafeNormalize(float3(A.rg + B.rg, A.b * B.b));
        }
        
        void Unity_Add_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A + B;
        }
        
void Unity_SceneColor_float(float4 UV, out float3 Out)
        {
            Out = tex2D(_GrabTexture, UV.xy);
        }
        
        void Unity_SceneDepth_Eye_float(float4 UV, out float Out)
        {
            if (unity_OrthoParams.w == 1.0)
            {
                Out = LinearEyeDepth(ComputeWorldSpacePosition(UV.xy, SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV.xy), UNITY_MATRIX_I_VP), UNITY_MATRIX_V);
            }
            else
            {
                Out = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV.xy), _ZBufferParams);
            }
        }
        
        void Unity_Power_float(float A, float B, out float Out)
        {
            Out = pow(A, B);
        }
        
        void Unity_Saturate_float(float In, out float Out)
        {
            Out = saturate(In);
        }
        
        void Unity_Lerp_float3(float3 A, float3 B, float3 T, out float3 Out)
        {
            Out = lerp(A, B, T);
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float3 BaseColor;
            float3 NormalTS;
            float3 Emission;
            float Metallic;
            float Smoothness;
            float Occlusion;
            float Alpha;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float4 _ScreenPosition_b4a8e582e1e849869b726cda686cfcc9_Out_0 = float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0);
            float _Property_9d6b87e68331c08eb165f43406bf9436_Out_0 = Vector1_E5125837;
            Gradient _Gradient_b8eb42ace63c02869968cbad8b8177e5_Out_0 = NewGradient(0, 2, 2, float4(1, 1, 1, 0.3617609),float4(0, 0, 0, 0.597055),float4(0, 0, 0, 0),float4(0, 0, 0, 0),float4(0, 0, 0, 0),float4(0, 0, 0, 0),float4(0, 0, 0, 0),float4(0, 0, 0, 0), float2(1, 0),float2(1, 1),float2(0, 0),float2(0, 0),float2(0, 0),float2(0, 0),float2(0, 0),float2(0, 0));
            Gradient _Gradient_3fae1ac6f27a5884adb30b86f3dcd25b_Out_0 = NewGradient(0, 3, 2, float4(1, 1, 1, 0),float4(0, 0, 0, 0.0411841),float4(1, 1, 1, 0.08529793),float4(0, 0, 0, 0),float4(0, 0, 0, 0),float4(0, 0, 0, 0),float4(0, 0, 0, 0),float4(0, 0, 0, 0), float2(1, 0),float2(1, 1),float2(0, 0),float2(0, 0),float2(0, 0),float2(0, 0),float2(0, 0),float2(0, 0));
            float _Property_a0cfb5db1fb47081ba331d38287cdb6e_Out_0 = Vector1_C7406269;
            float _Property_36028270ca87d38da7655131fbe5e971_Out_0 = Vector1_76A233A4;
            Bindings_RipplesSubGraph_8c331c1756d2a9b47b80805d5e9b5760_float _RipplesSubGraph_3b6545772f8b41e184862b0bb331f760;
            _RipplesSubGraph_3b6545772f8b41e184862b0bb331f760.WorldSpaceNormal = IN.WorldSpaceNormal;
            _RipplesSubGraph_3b6545772f8b41e184862b0bb331f760.WorldSpaceTangent = IN.WorldSpaceTangent;
            _RipplesSubGraph_3b6545772f8b41e184862b0bb331f760.WorldSpaceBiTangent = IN.WorldSpaceBiTangent;
            _RipplesSubGraph_3b6545772f8b41e184862b0bb331f760.WorldSpacePosition = IN.WorldSpacePosition;
            _RipplesSubGraph_3b6545772f8b41e184862b0bb331f760.uv0 = IN.uv0;
            float3 _RipplesSubGraph_3b6545772f8b41e184862b0bb331f760_OutVector3_1;
            SG_RipplesSubGraph_8c331c1756d2a9b47b80805d5e9b5760_float(_Property_9d6b87e68331c08eb165f43406bf9436_Out_0, _Gradient_b8eb42ace63c02869968cbad8b8177e5_Out_0, _Gradient_3fae1ac6f27a5884adb30b86f3dcd25b_Out_0, _Property_a0cfb5db1fb47081ba331d38287cdb6e_Out_0, IN.TimeParameters.x, _Property_36028270ca87d38da7655131fbe5e971_Out_0, float2 (0, 0), _RipplesSubGraph_3b6545772f8b41e184862b0bb331f760, _RipplesSubGraph_3b6545772f8b41e184862b0bb331f760_OutVector3_1);
            float _Add_d5f962a23d1d47c489c9884b8ec32f01_Out_2;
            Unity_Add_float(IN.TimeParameters.x, float(17.94), _Add_d5f962a23d1d47c489c9884b8ec32f01_Out_2);
            float _Property_312cadf13743d98cb6389ba7879b2ff1_Out_0 = Vector1_76A233A4;
            float _Multiply_b7a01f92c625c48ba25de798aaa70f2a_Out_2;
            Unity_Multiply_float_float(_Property_312cadf13743d98cb6389ba7879b2ff1_Out_0, 0.5, _Multiply_b7a01f92c625c48ba25de798aaa70f2a_Out_2);
            Bindings_RipplesSubGraph_8c331c1756d2a9b47b80805d5e9b5760_float _RipplesSubGraph_e07137b991464b7b9a9828b5c96f81a6;
            _RipplesSubGraph_e07137b991464b7b9a9828b5c96f81a6.WorldSpaceNormal = IN.WorldSpaceNormal;
            _RipplesSubGraph_e07137b991464b7b9a9828b5c96f81a6.WorldSpaceTangent = IN.WorldSpaceTangent;
            _RipplesSubGraph_e07137b991464b7b9a9828b5c96f81a6.WorldSpaceBiTangent = IN.WorldSpaceBiTangent;
            _RipplesSubGraph_e07137b991464b7b9a9828b5c96f81a6.WorldSpacePosition = IN.WorldSpacePosition;
            _RipplesSubGraph_e07137b991464b7b9a9828b5c96f81a6.uv0 = IN.uv0;
            float3 _RipplesSubGraph_e07137b991464b7b9a9828b5c96f81a6_OutVector3_1;
            SG_RipplesSubGraph_8c331c1756d2a9b47b80805d5e9b5760_float(_Property_9d6b87e68331c08eb165f43406bf9436_Out_0, _Gradient_b8eb42ace63c02869968cbad8b8177e5_Out_0, _Gradient_3fae1ac6f27a5884adb30b86f3dcd25b_Out_0, _Property_a0cfb5db1fb47081ba331d38287cdb6e_Out_0, _Add_d5f962a23d1d47c489c9884b8ec32f01_Out_2, _Multiply_b7a01f92c625c48ba25de798aaa70f2a_Out_2, float2 (0, 0), _RipplesSubGraph_e07137b991464b7b9a9828b5c96f81a6, _RipplesSubGraph_e07137b991464b7b9a9828b5c96f81a6_OutVector3_1);
            float3 _NormalBlend_f0858e7eedf2728ebdc013bdd4ec0577_Out_2;
            Unity_NormalBlend_float(_RipplesSubGraph_3b6545772f8b41e184862b0bb331f760_OutVector3_1, _RipplesSubGraph_e07137b991464b7b9a9828b5c96f81a6_OutVector3_1, _NormalBlend_f0858e7eedf2728ebdc013bdd4ec0577_Out_2);
            float3 _Add_6ef18bc46f03d388a7f1ff8beac95197_Out_2;
            Unity_Add_float3((_ScreenPosition_b4a8e582e1e849869b726cda686cfcc9_Out_0.xyz), _NormalBlend_f0858e7eedf2728ebdc013bdd4ec0577_Out_2, _Add_6ef18bc46f03d388a7f1ff8beac95197_Out_2);
            float3 _SceneColor_7f3fccc718c9208ea05c245d4f2f6794_Out_1;
            Unity_SceneColor_float((float4(_Add_6ef18bc46f03d388a7f1ff8beac95197_Out_2, 1.0)), _SceneColor_7f3fccc718c9208ea05c245d4f2f6794_Out_1);
            float4 _Property_c5168c0788e20d88a161c906be99b878_Out_0 = Color_C343EF87;
            float _Property_d9b056fed78dd085b4d0698251bc8753_Out_0 = Vector1_2FA69C8F;
            float _SceneDepth_5041dfd2c8895382ae5d6fd487809adc_Out_1;
            Unity_SceneDepth_Eye_float(float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _SceneDepth_5041dfd2c8895382ae5d6fd487809adc_Out_1);
            float4 _ScreenPosition_8d8b51c518d1818981db05311793998f_Out_0 = IN.ScreenPosition;
            float _Split_8d0366c6012fb585a39dd52842ac3f19_R_1 = _ScreenPosition_8d8b51c518d1818981db05311793998f_Out_0[0];
            float _Split_8d0366c6012fb585a39dd52842ac3f19_G_2 = _ScreenPosition_8d8b51c518d1818981db05311793998f_Out_0[1];
            float _Split_8d0366c6012fb585a39dd52842ac3f19_B_3 = _ScreenPosition_8d8b51c518d1818981db05311793998f_Out_0[2];
            float _Split_8d0366c6012fb585a39dd52842ac3f19_A_4 = _ScreenPosition_8d8b51c518d1818981db05311793998f_Out_0[3];
            float _Subtract_c825aee699188186ad9c2b51c72d4b8d_Out_2;
            Unity_Subtract_float(_SceneDepth_5041dfd2c8895382ae5d6fd487809adc_Out_1, _Split_8d0366c6012fb585a39dd52842ac3f19_A_4, _Subtract_c825aee699188186ad9c2b51c72d4b8d_Out_2);
            float _Property_3db3e2d81cb1a188aca23ece62d7f199_Out_0 = Vector1_5327F7C2;
            float _Multiply_fa4b52c3a2fad089870ad7808d047486_Out_2;
            Unity_Multiply_float_float(_Property_3db3e2d81cb1a188aca23ece62d7f199_Out_0, 0.1, _Multiply_fa4b52c3a2fad089870ad7808d047486_Out_2);
            float _Power_b8dcd558ef12958284e491078c34dedd_Out_2;
            Unity_Power_float(_Subtract_c825aee699188186ad9c2b51c72d4b8d_Out_2, _Multiply_fa4b52c3a2fad089870ad7808d047486_Out_2, _Power_b8dcd558ef12958284e491078c34dedd_Out_2);
            float _Add_9a13fa6ccec69d8fabcbe0a82247f9f2_Out_2;
            Unity_Add_float(_Property_d9b056fed78dd085b4d0698251bc8753_Out_0, _Power_b8dcd558ef12958284e491078c34dedd_Out_2, _Add_9a13fa6ccec69d8fabcbe0a82247f9f2_Out_2);
            float _Saturate_919586c53b6e588587683723594ef3bd_Out_1;
            Unity_Saturate_float(_Add_9a13fa6ccec69d8fabcbe0a82247f9f2_Out_2, _Saturate_919586c53b6e588587683723594ef3bd_Out_1);
            float3 _Lerp_2117a5b13f63f48887d25214347e82c4_Out_3;
            Unity_Lerp_float3(_SceneColor_7f3fccc718c9208ea05c245d4f2f6794_Out_1, (_Property_c5168c0788e20d88a161c906be99b878_Out_0.xyz), (_Saturate_919586c53b6e588587683723594ef3bd_Out_1.xxx), _Lerp_2117a5b13f63f48887d25214347e82c4_Out_3);
            surface.BaseColor = _Lerp_2117a5b13f63f48887d25214347e82c4_Out_3;
            surface.NormalTS = _NormalBlend_f0858e7eedf2728ebdc013bdd4ec0577_Out_2;
            surface.Emission = float3(0, 0, 0);
            surface.Metallic = float(0);
            surface.Smoothness = float(0.8);
            surface.Occlusion = float(1);
            surface.Alpha = float(1);
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
            
        
            // must use interpolated tangent, bitangent and normal before they are normalized in the pixel shader.
            float3 unnormalizedNormalWS = input.normalWS;
            const float renormFactor = 1.0 / length(unnormalizedNormalWS);
        
            // use bitangent on the fly like in hdrp
            // IMPORTANT! If we ever support Flip on double sided materials ensure bitangent and tangent are NOT flipped.
            float crossSign = (input.tangentWS.w > 0.0 ? 1.0 : -1.0)* GetOddNegativeScale();
            float3 bitang = crossSign * cross(input.normalWS.xyz, input.tangentWS.xyz);
        
            output.WorldSpaceNormal = renormFactor * input.normalWS.xyz;      // we want a unit length Normal Vector node in shader graph
            output.TangentSpaceNormal = float3(0.0f, 0.0f, 1.0f);
        
            // to preserve mikktspace compliance we use same scale renormFactor as was used on the normal.
            // This is explained in section 2.2 in "surface gradient based bump mapping framework"
            output.WorldSpaceTangent = renormFactor * input.tangentWS.xyz;
            output.WorldSpaceBiTangent = renormFactor * bitang;
        
            output.WorldSpacePosition = input.positionWS;
            output.ScreenPosition = ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
            output.uv0 = input.texCoord0;
            output.TimeParameters = _TimeParameters.xyz; // This is mainly for LW as HD overwrite this value
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        void BuildAppDataFull(Attributes attributes, VertexDescription vertexDescription, inout appdata_full result)
        {
            result.vertex     = float4(attributes.positionOS, 1);
            result.tangent    = attributes.tangentOS;
            result.normal     = attributes.normalOS;
            result.texcoord   = attributes.uv0;
            result.texcoord1  = attributes.uv1;
            result.vertex     = float4(vertexDescription.Position, 1);
            result.normal     = vertexDescription.Normal;
            result.tangent    = float4(vertexDescription.Tangent, 0);
            #if UNITY_ANY_INSTANCING_ENABLED
            #endif
        }
        
        void VaryingsToSurfaceVertex(Varyings varyings, inout v2f_surf result)
        {
            result.pos = varyings.positionCS;
            result.worldPos = varyings.positionWS;
            result.worldNormal = varyings.normalWS;
            result.viewDir = varyings.viewDirectionWS;
            // World Tangent isn't an available input on v2f_surf
        
            result._ShadowCoord = varyings.shadowCoord;
        
            #if UNITY_ANY_INSTANCING_ENABLED
            #endif
            #if !defined(LIGHTMAP_ON)
            #if UNITY_SHOULD_SAMPLE_SH
            result.sh = varyings.sh;
            #endif
            #endif
            #if defined(LIGHTMAP_ON)
            result.lmap.xy = varyings.lightmapUV;
            #endif
            #ifdef VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
                result.fogCoord = varyings.fogFactorAndVertexLight.x;
                COPY_TO_LIGHT_COORDS(result, varyings.fogFactorAndVertexLight.yzw);
            #endif
        
            DEFAULT_UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(varyings, result);
        }
        
        void SurfaceVertexToVaryings(v2f_surf surfVertex, inout Varyings result)
        {
            result.positionCS = surfVertex.pos;
            result.positionWS = surfVertex.worldPos;
            result.normalWS = surfVertex.worldNormal;
            // viewDirectionWS is never filled out in the legacy pass' function. Always use the value computed by SRP
            // World Tangent isn't an available input on v2f_surf
            result.shadowCoord = surfVertex._ShadowCoord;
        
            #if UNITY_ANY_INSTANCING_ENABLED
            #endif
            #if !defined(LIGHTMAP_ON)
            #if UNITY_SHOULD_SAMPLE_SH
            result.sh = surfVertex.sh;
            #endif
            #endif
            #if defined(LIGHTMAP_ON)
            result.lightmapUV = surfVertex.lmap.xy;
            #endif
            #ifdef VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
                result.fogFactorAndVertexLight.x = surfVertex.fogCoord;
                COPY_FROM_LIGHT_COORDS(result.fogFactorAndVertexLight.yzw, surfVertex);
            #endif
        
            DEFAULT_UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(surfVertex, result);
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/PBRForwardPass.hlsl"
        
        ENDHLSL
        }
        Pass
        {
            Name "BuiltIn ForwardAdd"
            Tags
            {
                "LightMode" = "ForwardAdd"
            }
        
        // Render State
        Blend SrcAlpha One
        ZWrite Off
        ColorMask RGB
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
sampler2D _GrabTexture;
        
        // Pragmas
        #pragma target 3.0
        #pragma multi_compile_instancing
        #pragma multi_compile_fog
        #pragma multi_compile_fwdadd_fullshadows
        #pragma vertex vert
        #pragma fragment frag
        
        // DotsInstancingOptions: <None>
        // HybridV1InjectedBuiltinProperties: <None>
        
        // Keywords
        #pragma multi_compile _ _SCREEN_SPACE_OCCLUSION
        #pragma multi_compile _ LIGHTMAP_ON
        #pragma multi_compile _ DIRLIGHTMAP_COMBINED
        #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
        #pragma multi_compile _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS _ADDITIONAL_OFF
        #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
        #pragma multi_compile _ _SHADOWS_SOFT
        #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
        #pragma multi_compile _ SHADOWS_SHADOWMASK
        // GraphKeywords: <None>
        
        // Defines
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define ATTRIBUTES_NEED_TEXCOORD1
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_NORMAL_WS
        #define VARYINGS_NEED_TANGENT_WS
        #define VARYINGS_NEED_TEXCOORD0
        #define VARYINGS_NEED_VIEWDIRECTION_WS
        #define VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_FORWARD_ADD
        #define BUILTIN_TARGET_API 1
        #define _BUILTIN_SURFACE_TYPE_TRANSPARENT 1
        #define REQUIRE_DEPTH_TEXTURE
        #define REQUIRE_OPAQUE_TEXTURE
        /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */
        #ifdef _BUILTIN_SURFACE_TYPE_TRANSPARENT
        #define _SURFACE_TYPE_TRANSPARENT _BUILTIN_SURFACE_TYPE_TRANSPARENT
        #endif
        #ifdef _BUILTIN_ALPHATEST_ON
        #define _ALPHATEST_ON _BUILTIN_ALPHATEST_ON
        #endif
        #ifdef _BUILTIN_AlphaClip
        #define _AlphaClip _BUILTIN_AlphaClip
        #endif
        #ifdef _BUILTIN_ALPHAPREMULTIPLY_ON
        #define _ALPHAPREMULTIPLY_ON _BUILTIN_ALPHAPREMULTIPLY_ON
        #endif
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/Shim/Shims.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/LegacySurfaceVertex.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/ShaderGraphFunctions.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
             float4 uv1 : TEXCOORD1;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 positionWS;
             float3 normalWS;
             float4 tangentWS;
             float4 texCoord0;
             float3 viewDirectionWS;
            #if defined(LIGHTMAP_ON)
             float2 lightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
             float3 sh;
            #endif
             float4 fogFactorAndVertexLight;
             float4 shadowCoord;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float3 WorldSpaceNormal;
             float3 TangentSpaceNormal;
             float3 WorldSpaceTangent;
             float3 WorldSpaceBiTangent;
             float3 WorldSpacePosition;
             float4 ScreenPosition;
             float4 uv0;
             float3 TimeParameters;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
            #if defined(LIGHTMAP_ON)
             float2 lightmapUV : INTERP0;
            #endif
            #if !defined(LIGHTMAP_ON)
             float3 sh : INTERP1;
            #endif
             float4 tangentWS : INTERP2;
             float4 texCoord0 : INTERP3;
             float4 fogFactorAndVertexLight : INTERP4;
             float4 shadowCoord : INTERP5;
             float3 positionWS : INTERP6;
             float3 normalWS : INTERP7;
             float3 viewDirectionWS : INTERP8;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            #if defined(LIGHTMAP_ON)
            output.lightmapUV = input.lightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
            output.sh = input.sh;
            #endif
            output.tangentWS.xyzw = input.tangentWS;
            output.texCoord0.xyzw = input.texCoord0;
            output.fogFactorAndVertexLight.xyzw = input.fogFactorAndVertexLight;
            output.shadowCoord.xyzw = input.shadowCoord;
            output.positionWS.xyz = input.positionWS;
            output.normalWS.xyz = input.normalWS;
            output.viewDirectionWS.xyz = input.viewDirectionWS;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            #if defined(LIGHTMAP_ON)
            output.lightmapUV = input.lightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
            output.sh = input.sh;
            #endif
            output.tangentWS = input.tangentWS.xyzw;
            output.texCoord0 = input.texCoord0.xyzw;
            output.fogFactorAndVertexLight = input.fogFactorAndVertexLight.xyzw;
            output.shadowCoord = input.shadowCoord.xyzw;
            output.positionWS = input.positionWS.xyz;
            output.normalWS = input.normalWS.xyz;
            output.viewDirectionWS = input.viewDirectionWS.xyz;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float Vector1_E5125837;
        float Vector1_C7406269;
        float Vector1_76A233A4;
        float Vector1_5327F7C2;
        float Vector1_2FA69C8F;
        float4 Color_C343EF87;
        CBUFFER_END
        
        // Object and Global properties
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // Graph Functions
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
        Out = A * B;
        }
        
        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        void Unity_Floor_float2(float2 In, out float2 Out)
        {
            Out = floor(In);
        }
        
        void Unity_RandomRange_float(float2 Seed, float Min, float Max, out float Out)
        {
             float randomno =  frac(sin(dot(Seed, float2(12.9898, 78.233)))*43758.5453);
             Out = lerp(Min, Max, randomno);
        }
        
        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }
        
        void Unity_Floor_float(float In, out float Out)
        {
            Out = floor(In);
        }
        
        void Unity_Add_float2(float2 A, float2 B, out float2 Out)
        {
            Out = A + B;
        }
        
        void Unity_Fraction_float2(float2 In, out float2 Out)
        {
            Out = frac(In);
        }
        
        void Unity_Distance_float2(float2 A, float2 B, out float Out)
        {
            Out = distance(A, B);
        }
        
        void Unity_Fraction_float(float In, out float Out)
        {
            Out = frac(In);
        }
        
        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }
        
        void Unity_SampleGradientV0_float(Gradient Gradient, float Time, out float4 Out)
        {
            float3 color = Gradient.colors[0].rgb;
            [unroll]
            for (int c = 1; c < Gradient.colorsLength; c++)
            {
                float colorPos = saturate((Time - Gradient.colors[c - 1].w) / (Gradient.colors[c].w - Gradient.colors[c - 1].w)) * step(c, Gradient.colorsLength - 1);
                color = lerp(color, Gradient.colors[c].rgb, lerp(colorPos, step(0.01, colorPos), Gradient.type));
            }
        #ifndef UNITY_COLORSPACE_GAMMA
            color = SRGBToLinear(color);
        #endif
            float alpha = Gradient.alphas[0].x;
            [unroll]
            for (int a = 1; a < Gradient.alphasLength; a++)
            {
                float alphaPos = saturate((Time - Gradient.alphas[a - 1].y) / (Gradient.alphas[a].y - Gradient.alphas[a - 1].y)) * step(a, Gradient.alphasLength - 1);
                alpha = lerp(alpha, Gradient.alphas[a].x, lerp(alphaPos, step(0.01, alphaPos), Gradient.type));
            }
            Out = float4(color, alpha);
        }
        
        void Unity_Lerp_float4(float4 A, float4 B, float4 T, out float4 Out)
        {
            Out = lerp(A, B, T);
        }
        
        void Unity_NormalFromHeight_Tangent_float(float In, float Strength, float3 Position, float3x3 TangentMatrix, out float3 Out)
        {
        
                    #if defined(SHADER_STAGE_RAY_TRACING) && defined(RAYTRACING_SHADER_GRAPH_DEFAULT)
                    #error 'Normal From Height' node is not supported in ray tracing, please provide an alternate implementation, relying for instance on the 'Raytracing Quality' keyword
                    #endif
        float3 worldDerivativeX = ddx(Position);
        float3 worldDerivativeY = ddy(Position);
        
        float3 crossX = cross(TangentMatrix[2].xyz, worldDerivativeX);
        float3 crossY = cross(worldDerivativeY, TangentMatrix[2].xyz);
        float d = dot(worldDerivativeX, crossY);
        float sgn = d < 0.0 ? (-1.0f) : 1.0f;
        float surface = sgn / max(0.000000000000001192093f, abs(d));
        
        float dHdx = ddx(In);
        float dHdy = ddy(In);
        float3 surfGrad = surface * (dHdx*crossY + dHdy*crossX);
        Out = SafeNormalize(TangentMatrix[2].xyz - (Strength * surfGrad));
        Out = TransformWorldToTangent(Out, TangentMatrix);
        }
        
        void Unity_NormalStrength_float(float3 In, float Strength, out float3 Out)
        {
            Out = float3(In.rg * Strength, lerp(1, In.b, saturate(Strength)));
        }
        
        struct Bindings_RipplesSubGraph_8c331c1756d2a9b47b80805d5e9b5760_float
        {
        float3 WorldSpaceNormal;
        float3 WorldSpaceTangent;
        float3 WorldSpaceBiTangent;
        float3 WorldSpacePosition;
        half4 uv0;
        };
        
        void SG_RipplesSubGraph_8c331c1756d2a9b47b80805d5e9b5760_float(float Vector1_A606AD0F, Gradient Gradient_AFB6B065, Gradient Gradient_612A0EE6, float Vector1_38564B71, float Vector1_48991907, float Vector1_E03EF315, float2 Vector2_8FC8A8DE, Bindings_RipplesSubGraph_8c331c1756d2a9b47b80805d5e9b5760_float IN, out float3 OutVector3_1)
        {
        Gradient _Property_9bccf34d24839482816c85b42874663b_Out_0 = Gradient_612A0EE6;
        float _Property_89d96fee4ae9b58596cca58f9ca66d82_Out_0 = Vector1_38564B71;
        float _Property_e7381c82bf07f28e87e55268633f03a6_Out_0 = Vector1_48991907;
        float _Multiply_2b7f465bf86b238ba88c3abadfb4dc6c_Out_2;
        Unity_Multiply_float_float(_Property_89d96fee4ae9b58596cca58f9ca66d82_Out_0, _Property_e7381c82bf07f28e87e55268633f03a6_Out_0, _Multiply_2b7f465bf86b238ba88c3abadfb4dc6c_Out_2);
        float _Property_c9b401be3cccc487963a18b18a5b00c5_Out_0 = Vector1_E03EF315;
        float2 _Property_7fd9512f128cc584b63bd9ba93e3805b_Out_0 = Vector2_8FC8A8DE;
        float2 _TilingAndOffset_054c12590944d280835bf7af8fbcbb8a_Out_3;
        Unity_TilingAndOffset_float(IN.uv0.xy, (_Property_c9b401be3cccc487963a18b18a5b00c5_Out_0.xx), _Property_7fd9512f128cc584b63bd9ba93e3805b_Out_0, _TilingAndOffset_054c12590944d280835bf7af8fbcbb8a_Out_3);
        float2 _Floor_b11cd06caee6518891d995cc8a1ef27a_Out_1;
        Unity_Floor_float2(_TilingAndOffset_054c12590944d280835bf7af8fbcbb8a_Out_3, _Floor_b11cd06caee6518891d995cc8a1ef27a_Out_1);
        float _RandomRange_715aacb97fba9b858184047a525d5dab_Out_3;
        Unity_RandomRange_float(_Floor_b11cd06caee6518891d995cc8a1ef27a_Out_1, float(0), float(1), _RandomRange_715aacb97fba9b858184047a525d5dab_Out_3);
        float _Add_66f573fa287cb288ba1bc9cd69e16fda_Out_2;
        Unity_Add_float(_Multiply_2b7f465bf86b238ba88c3abadfb4dc6c_Out_2, _RandomRange_715aacb97fba9b858184047a525d5dab_Out_3, _Add_66f573fa287cb288ba1bc9cd69e16fda_Out_2);
        float _Floor_751a8067250b49808f479855f31ee1a7_Out_1;
        Unity_Floor_float(_Add_66f573fa287cb288ba1bc9cd69e16fda_Out_2, _Floor_751a8067250b49808f479855f31ee1a7_Out_1);
        float2 _Add_1842fa0e1536f388b8a6d5f64e3e062f_Out_2;
        Unity_Add_float2((_Floor_751a8067250b49808f479855f31ee1a7_Out_1.xx), _Floor_b11cd06caee6518891d995cc8a1ef27a_Out_1, _Add_1842fa0e1536f388b8a6d5f64e3e062f_Out_2);
        float _RandomRange_d7266f27a6b9958f837bf09bdc762cdb_Out_3;
        Unity_RandomRange_float(_Add_1842fa0e1536f388b8a6d5f64e3e062f_Out_2, float(0.3), float(0.7), _RandomRange_d7266f27a6b9958f837bf09bdc762cdb_Out_3);
        float2 _Add_d47dc767dcea72828a6821b1dc7669b0_Out_2;
        Unity_Add_float2(_Add_1842fa0e1536f388b8a6d5f64e3e062f_Out_2, float2(1, 1), _Add_d47dc767dcea72828a6821b1dc7669b0_Out_2);
        float _RandomRange_fb667fb6d6ea1f878b072b6a0b019718_Out_3;
        Unity_RandomRange_float(_Add_d47dc767dcea72828a6821b1dc7669b0_Out_2, float(0.3), float(0.7), _RandomRange_fb667fb6d6ea1f878b072b6a0b019718_Out_3);
        float2 _Vector2_527e5a6aebcea4839bc8a823f63125be_Out_0 = float2(_RandomRange_d7266f27a6b9958f837bf09bdc762cdb_Out_3, _RandomRange_fb667fb6d6ea1f878b072b6a0b019718_Out_3);
        float2 _Fraction_722b542af4cde88480600980a547170a_Out_1;
        Unity_Fraction_float2(_TilingAndOffset_054c12590944d280835bf7af8fbcbb8a_Out_3, _Fraction_722b542af4cde88480600980a547170a_Out_1);
        float _Distance_6bfa0e2d94ebc48bbd4fde991bc032fa_Out_2;
        Unity_Distance_float2(_Vector2_527e5a6aebcea4839bc8a823f63125be_Out_0, _Fraction_722b542af4cde88480600980a547170a_Out_1, _Distance_6bfa0e2d94ebc48bbd4fde991bc032fa_Out_2);
        float _Fraction_4bcfd9567d3dca85be4e0af521de8a7c_Out_1;
        Unity_Fraction_float(_Add_66f573fa287cb288ba1bc9cd69e16fda_Out_2, _Fraction_4bcfd9567d3dca85be4e0af521de8a7c_Out_1);
        float _Add_9355d1e3e12bcd8183d6a28fbd898cb2_Out_2;
        Unity_Add_float(_Fraction_4bcfd9567d3dca85be4e0af521de8a7c_Out_1, float(-0.3), _Add_9355d1e3e12bcd8183d6a28fbd898cb2_Out_2);
        float _Subtract_faef160017a9c983ad052cbecc70feda_Out_2;
        Unity_Subtract_float(_Distance_6bfa0e2d94ebc48bbd4fde991bc032fa_Out_2, _Add_9355d1e3e12bcd8183d6a28fbd898cb2_Out_2, _Subtract_faef160017a9c983ad052cbecc70feda_Out_2);
        float4 _SampleGradient_8d5fe46189810380ac3a508fe44b59d0_Out_2;
        Unity_SampleGradientV0_float(_Property_9bccf34d24839482816c85b42874663b_Out_0, _Subtract_faef160017a9c983ad052cbecc70feda_Out_2, _SampleGradient_8d5fe46189810380ac3a508fe44b59d0_Out_2);
        Gradient _Property_0c08dbb904c8cb84ae02c68bb638aded_Out_0 = Gradient_AFB6B065;
        float4 _SampleGradient_616374cca7d265828808200a0609432b_Out_2;
        Unity_SampleGradientV0_float(_Property_0c08dbb904c8cb84ae02c68bb638aded_Out_0, _Fraction_4bcfd9567d3dca85be4e0af521de8a7c_Out_1, _SampleGradient_616374cca7d265828808200a0609432b_Out_2);
        float4 _Lerp_8a47fa5e2810ac80858fc635b9f6b924_Out_3;
        Unity_Lerp_float4(float4(1, 1, 1, 1), _SampleGradient_8d5fe46189810380ac3a508fe44b59d0_Out_2, _SampleGradient_616374cca7d265828808200a0609432b_Out_2, _Lerp_8a47fa5e2810ac80858fc635b9f6b924_Out_3);
        float3 _NormalFromHeight_cb5f128a1f05298fb493e02ae043a713_Out_1;
        float3x3 _NormalFromHeight_cb5f128a1f05298fb493e02ae043a713_TangentMatrix = float3x3(IN.WorldSpaceTangent, IN.WorldSpaceBiTangent, IN.WorldSpaceNormal);
        float3 _NormalFromHeight_cb5f128a1f05298fb493e02ae043a713_Position = IN.WorldSpacePosition;
        Unity_NormalFromHeight_Tangent_float((_Lerp_8a47fa5e2810ac80858fc635b9f6b924_Out_3).x,float(0.01),_NormalFromHeight_cb5f128a1f05298fb493e02ae043a713_Position,_NormalFromHeight_cb5f128a1f05298fb493e02ae043a713_TangentMatrix, _NormalFromHeight_cb5f128a1f05298fb493e02ae043a713_Out_1);
        float _Property_2c37c06e20892a80b173de7ffa4ab033_Out_0 = Vector1_A606AD0F;
        float3 _NormalStrength_bf4bdc1d176af585801fc908c9d75602_Out_2;
        Unity_NormalStrength_float(_NormalFromHeight_cb5f128a1f05298fb493e02ae043a713_Out_1, _Property_2c37c06e20892a80b173de7ffa4ab033_Out_0, _NormalStrength_bf4bdc1d176af585801fc908c9d75602_Out_2);
        OutVector3_1 = _NormalStrength_bf4bdc1d176af585801fc908c9d75602_Out_2;
        }
        
        void Unity_NormalBlend_float(float3 A, float3 B, out float3 Out)
        {
            Out = SafeNormalize(float3(A.rg + B.rg, A.b * B.b));
        }
        
        void Unity_Add_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A + B;
        }
        
void Unity_SceneColor_float(float4 UV, out float3 Out)
        {
            Out = tex2D(_GrabTexture, UV.xy);
        }
        
        void Unity_SceneDepth_Eye_float(float4 UV, out float Out)
        {
            if (unity_OrthoParams.w == 1.0)
            {
                Out = LinearEyeDepth(ComputeWorldSpacePosition(UV.xy, SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV.xy), UNITY_MATRIX_I_VP), UNITY_MATRIX_V);
            }
            else
            {
                Out = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV.xy), _ZBufferParams);
            }
        }
        
        void Unity_Power_float(float A, float B, out float Out)
        {
            Out = pow(A, B);
        }
        
        void Unity_Saturate_float(float In, out float Out)
        {
            Out = saturate(In);
        }
        
        void Unity_Lerp_float3(float3 A, float3 B, float3 T, out float3 Out)
        {
            Out = lerp(A, B, T);
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float3 BaseColor;
            float3 NormalTS;
            float3 Emission;
            float Metallic;
            float Smoothness;
            float Occlusion;
            float Alpha;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float4 _ScreenPosition_b4a8e582e1e849869b726cda686cfcc9_Out_0 = float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0);
            float _Property_9d6b87e68331c08eb165f43406bf9436_Out_0 = Vector1_E5125837;
            Gradient _Gradient_b8eb42ace63c02869968cbad8b8177e5_Out_0 = NewGradient(0, 2, 2, float4(1, 1, 1, 0.3617609),float4(0, 0, 0, 0.597055),float4(0, 0, 0, 0),float4(0, 0, 0, 0),float4(0, 0, 0, 0),float4(0, 0, 0, 0),float4(0, 0, 0, 0),float4(0, 0, 0, 0), float2(1, 0),float2(1, 1),float2(0, 0),float2(0, 0),float2(0, 0),float2(0, 0),float2(0, 0),float2(0, 0));
            Gradient _Gradient_3fae1ac6f27a5884adb30b86f3dcd25b_Out_0 = NewGradient(0, 3, 2, float4(1, 1, 1, 0),float4(0, 0, 0, 0.0411841),float4(1, 1, 1, 0.08529793),float4(0, 0, 0, 0),float4(0, 0, 0, 0),float4(0, 0, 0, 0),float4(0, 0, 0, 0),float4(0, 0, 0, 0), float2(1, 0),float2(1, 1),float2(0, 0),float2(0, 0),float2(0, 0),float2(0, 0),float2(0, 0),float2(0, 0));
            float _Property_a0cfb5db1fb47081ba331d38287cdb6e_Out_0 = Vector1_C7406269;
            float _Property_36028270ca87d38da7655131fbe5e971_Out_0 = Vector1_76A233A4;
            Bindings_RipplesSubGraph_8c331c1756d2a9b47b80805d5e9b5760_float _RipplesSubGraph_3b6545772f8b41e184862b0bb331f760;
            _RipplesSubGraph_3b6545772f8b41e184862b0bb331f760.WorldSpaceNormal = IN.WorldSpaceNormal;
            _RipplesSubGraph_3b6545772f8b41e184862b0bb331f760.WorldSpaceTangent = IN.WorldSpaceTangent;
            _RipplesSubGraph_3b6545772f8b41e184862b0bb331f760.WorldSpaceBiTangent = IN.WorldSpaceBiTangent;
            _RipplesSubGraph_3b6545772f8b41e184862b0bb331f760.WorldSpacePosition = IN.WorldSpacePosition;
            _RipplesSubGraph_3b6545772f8b41e184862b0bb331f760.uv0 = IN.uv0;
            float3 _RipplesSubGraph_3b6545772f8b41e184862b0bb331f760_OutVector3_1;
            SG_RipplesSubGraph_8c331c1756d2a9b47b80805d5e9b5760_float(_Property_9d6b87e68331c08eb165f43406bf9436_Out_0, _Gradient_b8eb42ace63c02869968cbad8b8177e5_Out_0, _Gradient_3fae1ac6f27a5884adb30b86f3dcd25b_Out_0, _Property_a0cfb5db1fb47081ba331d38287cdb6e_Out_0, IN.TimeParameters.x, _Property_36028270ca87d38da7655131fbe5e971_Out_0, float2 (0, 0), _RipplesSubGraph_3b6545772f8b41e184862b0bb331f760, _RipplesSubGraph_3b6545772f8b41e184862b0bb331f760_OutVector3_1);
            float _Add_d5f962a23d1d47c489c9884b8ec32f01_Out_2;
            Unity_Add_float(IN.TimeParameters.x, float(17.94), _Add_d5f962a23d1d47c489c9884b8ec32f01_Out_2);
            float _Property_312cadf13743d98cb6389ba7879b2ff1_Out_0 = Vector1_76A233A4;
            float _Multiply_b7a01f92c625c48ba25de798aaa70f2a_Out_2;
            Unity_Multiply_float_float(_Property_312cadf13743d98cb6389ba7879b2ff1_Out_0, 0.5, _Multiply_b7a01f92c625c48ba25de798aaa70f2a_Out_2);
            Bindings_RipplesSubGraph_8c331c1756d2a9b47b80805d5e9b5760_float _RipplesSubGraph_e07137b991464b7b9a9828b5c96f81a6;
            _RipplesSubGraph_e07137b991464b7b9a9828b5c96f81a6.WorldSpaceNormal = IN.WorldSpaceNormal;
            _RipplesSubGraph_e07137b991464b7b9a9828b5c96f81a6.WorldSpaceTangent = IN.WorldSpaceTangent;
            _RipplesSubGraph_e07137b991464b7b9a9828b5c96f81a6.WorldSpaceBiTangent = IN.WorldSpaceBiTangent;
            _RipplesSubGraph_e07137b991464b7b9a9828b5c96f81a6.WorldSpacePosition = IN.WorldSpacePosition;
            _RipplesSubGraph_e07137b991464b7b9a9828b5c96f81a6.uv0 = IN.uv0;
            float3 _RipplesSubGraph_e07137b991464b7b9a9828b5c96f81a6_OutVector3_1;
            SG_RipplesSubGraph_8c331c1756d2a9b47b80805d5e9b5760_float(_Property_9d6b87e68331c08eb165f43406bf9436_Out_0, _Gradient_b8eb42ace63c02869968cbad8b8177e5_Out_0, _Gradient_3fae1ac6f27a5884adb30b86f3dcd25b_Out_0, _Property_a0cfb5db1fb47081ba331d38287cdb6e_Out_0, _Add_d5f962a23d1d47c489c9884b8ec32f01_Out_2, _Multiply_b7a01f92c625c48ba25de798aaa70f2a_Out_2, float2 (0, 0), _RipplesSubGraph_e07137b991464b7b9a9828b5c96f81a6, _RipplesSubGraph_e07137b991464b7b9a9828b5c96f81a6_OutVector3_1);
            float3 _NormalBlend_f0858e7eedf2728ebdc013bdd4ec0577_Out_2;
            Unity_NormalBlend_float(_RipplesSubGraph_3b6545772f8b41e184862b0bb331f760_OutVector3_1, _RipplesSubGraph_e07137b991464b7b9a9828b5c96f81a6_OutVector3_1, _NormalBlend_f0858e7eedf2728ebdc013bdd4ec0577_Out_2);
            float3 _Add_6ef18bc46f03d388a7f1ff8beac95197_Out_2;
            Unity_Add_float3((_ScreenPosition_b4a8e582e1e849869b726cda686cfcc9_Out_0.xyz), _NormalBlend_f0858e7eedf2728ebdc013bdd4ec0577_Out_2, _Add_6ef18bc46f03d388a7f1ff8beac95197_Out_2);
            float3 _SceneColor_7f3fccc718c9208ea05c245d4f2f6794_Out_1;
            Unity_SceneColor_float((float4(_Add_6ef18bc46f03d388a7f1ff8beac95197_Out_2, 1.0)), _SceneColor_7f3fccc718c9208ea05c245d4f2f6794_Out_1);
            float4 _Property_c5168c0788e20d88a161c906be99b878_Out_0 = Color_C343EF87;
            float _Property_d9b056fed78dd085b4d0698251bc8753_Out_0 = Vector1_2FA69C8F;
            float _SceneDepth_5041dfd2c8895382ae5d6fd487809adc_Out_1;
            Unity_SceneDepth_Eye_float(float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _SceneDepth_5041dfd2c8895382ae5d6fd487809adc_Out_1);
            float4 _ScreenPosition_8d8b51c518d1818981db05311793998f_Out_0 = IN.ScreenPosition;
            float _Split_8d0366c6012fb585a39dd52842ac3f19_R_1 = _ScreenPosition_8d8b51c518d1818981db05311793998f_Out_0[0];
            float _Split_8d0366c6012fb585a39dd52842ac3f19_G_2 = _ScreenPosition_8d8b51c518d1818981db05311793998f_Out_0[1];
            float _Split_8d0366c6012fb585a39dd52842ac3f19_B_3 = _ScreenPosition_8d8b51c518d1818981db05311793998f_Out_0[2];
            float _Split_8d0366c6012fb585a39dd52842ac3f19_A_4 = _ScreenPosition_8d8b51c518d1818981db05311793998f_Out_0[3];
            float _Subtract_c825aee699188186ad9c2b51c72d4b8d_Out_2;
            Unity_Subtract_float(_SceneDepth_5041dfd2c8895382ae5d6fd487809adc_Out_1, _Split_8d0366c6012fb585a39dd52842ac3f19_A_4, _Subtract_c825aee699188186ad9c2b51c72d4b8d_Out_2);
            float _Property_3db3e2d81cb1a188aca23ece62d7f199_Out_0 = Vector1_5327F7C2;
            float _Multiply_fa4b52c3a2fad089870ad7808d047486_Out_2;
            Unity_Multiply_float_float(_Property_3db3e2d81cb1a188aca23ece62d7f199_Out_0, 0.1, _Multiply_fa4b52c3a2fad089870ad7808d047486_Out_2);
            float _Power_b8dcd558ef12958284e491078c34dedd_Out_2;
            Unity_Power_float(_Subtract_c825aee699188186ad9c2b51c72d4b8d_Out_2, _Multiply_fa4b52c3a2fad089870ad7808d047486_Out_2, _Power_b8dcd558ef12958284e491078c34dedd_Out_2);
            float _Add_9a13fa6ccec69d8fabcbe0a82247f9f2_Out_2;
            Unity_Add_float(_Property_d9b056fed78dd085b4d0698251bc8753_Out_0, _Power_b8dcd558ef12958284e491078c34dedd_Out_2, _Add_9a13fa6ccec69d8fabcbe0a82247f9f2_Out_2);
            float _Saturate_919586c53b6e588587683723594ef3bd_Out_1;
            Unity_Saturate_float(_Add_9a13fa6ccec69d8fabcbe0a82247f9f2_Out_2, _Saturate_919586c53b6e588587683723594ef3bd_Out_1);
            float3 _Lerp_2117a5b13f63f48887d25214347e82c4_Out_3;
            Unity_Lerp_float3(_SceneColor_7f3fccc718c9208ea05c245d4f2f6794_Out_1, (_Property_c5168c0788e20d88a161c906be99b878_Out_0.xyz), (_Saturate_919586c53b6e588587683723594ef3bd_Out_1.xxx), _Lerp_2117a5b13f63f48887d25214347e82c4_Out_3);
            surface.BaseColor = _Lerp_2117a5b13f63f48887d25214347e82c4_Out_3;
            surface.NormalTS = _NormalBlend_f0858e7eedf2728ebdc013bdd4ec0577_Out_2;
            surface.Emission = float3(0, 0, 0);
            surface.Metallic = float(0);
            surface.Smoothness = float(0.8);
            surface.Occlusion = float(1);
            surface.Alpha = float(1);
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
            
        
            // must use interpolated tangent, bitangent and normal before they are normalized in the pixel shader.
            float3 unnormalizedNormalWS = input.normalWS;
            const float renormFactor = 1.0 / length(unnormalizedNormalWS);
        
            // use bitangent on the fly like in hdrp
            // IMPORTANT! If we ever support Flip on double sided materials ensure bitangent and tangent are NOT flipped.
            float crossSign = (input.tangentWS.w > 0.0 ? 1.0 : -1.0)* GetOddNegativeScale();
            float3 bitang = crossSign * cross(input.normalWS.xyz, input.tangentWS.xyz);
        
            output.WorldSpaceNormal = renormFactor * input.normalWS.xyz;      // we want a unit length Normal Vector node in shader graph
            output.TangentSpaceNormal = float3(0.0f, 0.0f, 1.0f);
        
            // to preserve mikktspace compliance we use same scale renormFactor as was used on the normal.
            // This is explained in section 2.2 in "surface gradient based bump mapping framework"
            output.WorldSpaceTangent = renormFactor * input.tangentWS.xyz;
            output.WorldSpaceBiTangent = renormFactor * bitang;
        
            output.WorldSpacePosition = input.positionWS;
            output.ScreenPosition = ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
            output.uv0 = input.texCoord0;
            output.TimeParameters = _TimeParameters.xyz; // This is mainly for LW as HD overwrite this value
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        void BuildAppDataFull(Attributes attributes, VertexDescription vertexDescription, inout appdata_full result)
        {
            result.vertex     = float4(attributes.positionOS, 1);
            result.tangent    = attributes.tangentOS;
            result.normal     = attributes.normalOS;
            result.texcoord   = attributes.uv0;
            result.texcoord1  = attributes.uv1;
            result.vertex     = float4(vertexDescription.Position, 1);
            result.normal     = vertexDescription.Normal;
            result.tangent    = float4(vertexDescription.Tangent, 0);
            #if UNITY_ANY_INSTANCING_ENABLED
            #endif
        }
        
        void VaryingsToSurfaceVertex(Varyings varyings, inout v2f_surf result)
        {
            result.pos = varyings.positionCS;
            result.worldPos = varyings.positionWS;
            result.worldNormal = varyings.normalWS;
            result.viewDir = varyings.viewDirectionWS;
            // World Tangent isn't an available input on v2f_surf
        
            result._ShadowCoord = varyings.shadowCoord;
        
            #if UNITY_ANY_INSTANCING_ENABLED
            #endif
            #if !defined(LIGHTMAP_ON)
            #if UNITY_SHOULD_SAMPLE_SH
            result.sh = varyings.sh;
            #endif
            #endif
            #if defined(LIGHTMAP_ON)
            result.lmap.xy = varyings.lightmapUV;
            #endif
            #ifdef VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
                result.fogCoord = varyings.fogFactorAndVertexLight.x;
                COPY_TO_LIGHT_COORDS(result, varyings.fogFactorAndVertexLight.yzw);
            #endif
        
            DEFAULT_UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(varyings, result);
        }
        
        void SurfaceVertexToVaryings(v2f_surf surfVertex, inout Varyings result)
        {
            result.positionCS = surfVertex.pos;
            result.positionWS = surfVertex.worldPos;
            result.normalWS = surfVertex.worldNormal;
            // viewDirectionWS is never filled out in the legacy pass' function. Always use the value computed by SRP
            // World Tangent isn't an available input on v2f_surf
            result.shadowCoord = surfVertex._ShadowCoord;
        
            #if UNITY_ANY_INSTANCING_ENABLED
            #endif
            #if !defined(LIGHTMAP_ON)
            #if UNITY_SHOULD_SAMPLE_SH
            result.sh = surfVertex.sh;
            #endif
            #endif
            #if defined(LIGHTMAP_ON)
            result.lightmapUV = surfVertex.lmap.xy;
            #endif
            #ifdef VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
                result.fogFactorAndVertexLight.x = surfVertex.fogCoord;
                COPY_FROM_LIGHT_COORDS(result.fogFactorAndVertexLight.yzw, surfVertex);
            #endif
        
            DEFAULT_UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(surfVertex, result);
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/PBRForwardAddPass.hlsl"
        
        ENDHLSL
        }
        Pass
        {
            Name "BuiltIn Deferred"
            Tags
            {
                "LightMode" = "Deferred"
            }
        
        // Render State
        Cull Back
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZTest LEqual
        ZWrite Off
        ColorMask RGB
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
sampler2D _GrabTexture;
        
        // Pragmas
        #pragma target 4.5
        #pragma multi_compile_instancing
        #pragma exclude_renderers nomrt
        #pragma multi_compile_prepassfinal
        #pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
        #pragma vertex vert
        #pragma fragment frag
        
        // DotsInstancingOptions: <None>
        // HybridV1InjectedBuiltinProperties: <None>
        
        // Keywords
        #pragma multi_compile _ LIGHTMAP_ON
        #pragma multi_compile _ DIRLIGHTMAP_COMBINED
        #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
        #pragma multi_compile _ _SHADOWS_SOFT
        #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
        #pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
        #pragma multi_compile _ _GBUFFER_NORMALS_OCT
        // GraphKeywords: <None>
        
        // Defines
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define ATTRIBUTES_NEED_TEXCOORD1
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_NORMAL_WS
        #define VARYINGS_NEED_TANGENT_WS
        #define VARYINGS_NEED_TEXCOORD0
        #define VARYINGS_NEED_VIEWDIRECTION_WS
        #define VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_DEFERRED
        #define BUILTIN_TARGET_API 1
        #define _BUILTIN_SURFACE_TYPE_TRANSPARENT 1
        #define REQUIRE_DEPTH_TEXTURE
        #define REQUIRE_OPAQUE_TEXTURE
        /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */
        #ifdef _BUILTIN_SURFACE_TYPE_TRANSPARENT
        #define _SURFACE_TYPE_TRANSPARENT _BUILTIN_SURFACE_TYPE_TRANSPARENT
        #endif
        #ifdef _BUILTIN_ALPHATEST_ON
        #define _ALPHATEST_ON _BUILTIN_ALPHATEST_ON
        #endif
        #ifdef _BUILTIN_AlphaClip
        #define _AlphaClip _BUILTIN_AlphaClip
        #endif
        #ifdef _BUILTIN_ALPHAPREMULTIPLY_ON
        #define _ALPHAPREMULTIPLY_ON _BUILTIN_ALPHAPREMULTIPLY_ON
        #endif
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/Shim/Shims.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/LegacySurfaceVertex.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/ShaderGraphFunctions.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
             float4 uv1 : TEXCOORD1;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 positionWS;
             float3 normalWS;
             float4 tangentWS;
             float4 texCoord0;
             float3 viewDirectionWS;
            #if defined(LIGHTMAP_ON)
             float2 lightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
             float3 sh;
            #endif
             float4 fogFactorAndVertexLight;
             float4 shadowCoord;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float3 WorldSpaceNormal;
             float3 TangentSpaceNormal;
             float3 WorldSpaceTangent;
             float3 WorldSpaceBiTangent;
             float3 WorldSpacePosition;
             float4 ScreenPosition;
             float4 uv0;
             float3 TimeParameters;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
            #if defined(LIGHTMAP_ON)
             float2 lightmapUV : INTERP0;
            #endif
            #if !defined(LIGHTMAP_ON)
             float3 sh : INTERP1;
            #endif
             float4 tangentWS : INTERP2;
             float4 texCoord0 : INTERP3;
             float4 fogFactorAndVertexLight : INTERP4;
             float4 shadowCoord : INTERP5;
             float3 positionWS : INTERP6;
             float3 normalWS : INTERP7;
             float3 viewDirectionWS : INTERP8;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            #if defined(LIGHTMAP_ON)
            output.lightmapUV = input.lightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
            output.sh = input.sh;
            #endif
            output.tangentWS.xyzw = input.tangentWS;
            output.texCoord0.xyzw = input.texCoord0;
            output.fogFactorAndVertexLight.xyzw = input.fogFactorAndVertexLight;
            output.shadowCoord.xyzw = input.shadowCoord;
            output.positionWS.xyz = input.positionWS;
            output.normalWS.xyz = input.normalWS;
            output.viewDirectionWS.xyz = input.viewDirectionWS;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            #if defined(LIGHTMAP_ON)
            output.lightmapUV = input.lightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
            output.sh = input.sh;
            #endif
            output.tangentWS = input.tangentWS.xyzw;
            output.texCoord0 = input.texCoord0.xyzw;
            output.fogFactorAndVertexLight = input.fogFactorAndVertexLight.xyzw;
            output.shadowCoord = input.shadowCoord.xyzw;
            output.positionWS = input.positionWS.xyz;
            output.normalWS = input.normalWS.xyz;
            output.viewDirectionWS = input.viewDirectionWS.xyz;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float Vector1_E5125837;
        float Vector1_C7406269;
        float Vector1_76A233A4;
        float Vector1_5327F7C2;
        float Vector1_2FA69C8F;
        float4 Color_C343EF87;
        CBUFFER_END
        
        // Object and Global properties
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // Graph Functions
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
        Out = A * B;
        }
        
        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        void Unity_Floor_float2(float2 In, out float2 Out)
        {
            Out = floor(In);
        }
        
        void Unity_RandomRange_float(float2 Seed, float Min, float Max, out float Out)
        {
             float randomno =  frac(sin(dot(Seed, float2(12.9898, 78.233)))*43758.5453);
             Out = lerp(Min, Max, randomno);
        }
        
        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }
        
        void Unity_Floor_float(float In, out float Out)
        {
            Out = floor(In);
        }
        
        void Unity_Add_float2(float2 A, float2 B, out float2 Out)
        {
            Out = A + B;
        }
        
        void Unity_Fraction_float2(float2 In, out float2 Out)
        {
            Out = frac(In);
        }
        
        void Unity_Distance_float2(float2 A, float2 B, out float Out)
        {
            Out = distance(A, B);
        }
        
        void Unity_Fraction_float(float In, out float Out)
        {
            Out = frac(In);
        }
        
        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }
        
        void Unity_SampleGradientV0_float(Gradient Gradient, float Time, out float4 Out)
        {
            float3 color = Gradient.colors[0].rgb;
            [unroll]
            for (int c = 1; c < Gradient.colorsLength; c++)
            {
                float colorPos = saturate((Time - Gradient.colors[c - 1].w) / (Gradient.colors[c].w - Gradient.colors[c - 1].w)) * step(c, Gradient.colorsLength - 1);
                color = lerp(color, Gradient.colors[c].rgb, lerp(colorPos, step(0.01, colorPos), Gradient.type));
            }
        #ifndef UNITY_COLORSPACE_GAMMA
            color = SRGBToLinear(color);
        #endif
            float alpha = Gradient.alphas[0].x;
            [unroll]
            for (int a = 1; a < Gradient.alphasLength; a++)
            {
                float alphaPos = saturate((Time - Gradient.alphas[a - 1].y) / (Gradient.alphas[a].y - Gradient.alphas[a - 1].y)) * step(a, Gradient.alphasLength - 1);
                alpha = lerp(alpha, Gradient.alphas[a].x, lerp(alphaPos, step(0.01, alphaPos), Gradient.type));
            }
            Out = float4(color, alpha);
        }
        
        void Unity_Lerp_float4(float4 A, float4 B, float4 T, out float4 Out)
        {
            Out = lerp(A, B, T);
        }
        
        void Unity_NormalFromHeight_Tangent_float(float In, float Strength, float3 Position, float3x3 TangentMatrix, out float3 Out)
        {
        
                    #if defined(SHADER_STAGE_RAY_TRACING) && defined(RAYTRACING_SHADER_GRAPH_DEFAULT)
                    #error 'Normal From Height' node is not supported in ray tracing, please provide an alternate implementation, relying for instance on the 'Raytracing Quality' keyword
                    #endif
        float3 worldDerivativeX = ddx(Position);
        float3 worldDerivativeY = ddy(Position);
        
        float3 crossX = cross(TangentMatrix[2].xyz, worldDerivativeX);
        float3 crossY = cross(worldDerivativeY, TangentMatrix[2].xyz);
        float d = dot(worldDerivativeX, crossY);
        float sgn = d < 0.0 ? (-1.0f) : 1.0f;
        float surface = sgn / max(0.000000000000001192093f, abs(d));
        
        float dHdx = ddx(In);
        float dHdy = ddy(In);
        float3 surfGrad = surface * (dHdx*crossY + dHdy*crossX);
        Out = SafeNormalize(TangentMatrix[2].xyz - (Strength * surfGrad));
        Out = TransformWorldToTangent(Out, TangentMatrix);
        }
        
        void Unity_NormalStrength_float(float3 In, float Strength, out float3 Out)
        {
            Out = float3(In.rg * Strength, lerp(1, In.b, saturate(Strength)));
        }
        
        struct Bindings_RipplesSubGraph_8c331c1756d2a9b47b80805d5e9b5760_float
        {
        float3 WorldSpaceNormal;
        float3 WorldSpaceTangent;
        float3 WorldSpaceBiTangent;
        float3 WorldSpacePosition;
        half4 uv0;
        };
        
        void SG_RipplesSubGraph_8c331c1756d2a9b47b80805d5e9b5760_float(float Vector1_A606AD0F, Gradient Gradient_AFB6B065, Gradient Gradient_612A0EE6, float Vector1_38564B71, float Vector1_48991907, float Vector1_E03EF315, float2 Vector2_8FC8A8DE, Bindings_RipplesSubGraph_8c331c1756d2a9b47b80805d5e9b5760_float IN, out float3 OutVector3_1)
        {
        Gradient _Property_9bccf34d24839482816c85b42874663b_Out_0 = Gradient_612A0EE6;
        float _Property_89d96fee4ae9b58596cca58f9ca66d82_Out_0 = Vector1_38564B71;
        float _Property_e7381c82bf07f28e87e55268633f03a6_Out_0 = Vector1_48991907;
        float _Multiply_2b7f465bf86b238ba88c3abadfb4dc6c_Out_2;
        Unity_Multiply_float_float(_Property_89d96fee4ae9b58596cca58f9ca66d82_Out_0, _Property_e7381c82bf07f28e87e55268633f03a6_Out_0, _Multiply_2b7f465bf86b238ba88c3abadfb4dc6c_Out_2);
        float _Property_c9b401be3cccc487963a18b18a5b00c5_Out_0 = Vector1_E03EF315;
        float2 _Property_7fd9512f128cc584b63bd9ba93e3805b_Out_0 = Vector2_8FC8A8DE;
        float2 _TilingAndOffset_054c12590944d280835bf7af8fbcbb8a_Out_3;
        Unity_TilingAndOffset_float(IN.uv0.xy, (_Property_c9b401be3cccc487963a18b18a5b00c5_Out_0.xx), _Property_7fd9512f128cc584b63bd9ba93e3805b_Out_0, _TilingAndOffset_054c12590944d280835bf7af8fbcbb8a_Out_3);
        float2 _Floor_b11cd06caee6518891d995cc8a1ef27a_Out_1;
        Unity_Floor_float2(_TilingAndOffset_054c12590944d280835bf7af8fbcbb8a_Out_3, _Floor_b11cd06caee6518891d995cc8a1ef27a_Out_1);
        float _RandomRange_715aacb97fba9b858184047a525d5dab_Out_3;
        Unity_RandomRange_float(_Floor_b11cd06caee6518891d995cc8a1ef27a_Out_1, float(0), float(1), _RandomRange_715aacb97fba9b858184047a525d5dab_Out_3);
        float _Add_66f573fa287cb288ba1bc9cd69e16fda_Out_2;
        Unity_Add_float(_Multiply_2b7f465bf86b238ba88c3abadfb4dc6c_Out_2, _RandomRange_715aacb97fba9b858184047a525d5dab_Out_3, _Add_66f573fa287cb288ba1bc9cd69e16fda_Out_2);
        float _Floor_751a8067250b49808f479855f31ee1a7_Out_1;
        Unity_Floor_float(_Add_66f573fa287cb288ba1bc9cd69e16fda_Out_2, _Floor_751a8067250b49808f479855f31ee1a7_Out_1);
        float2 _Add_1842fa0e1536f388b8a6d5f64e3e062f_Out_2;
        Unity_Add_float2((_Floor_751a8067250b49808f479855f31ee1a7_Out_1.xx), _Floor_b11cd06caee6518891d995cc8a1ef27a_Out_1, _Add_1842fa0e1536f388b8a6d5f64e3e062f_Out_2);
        float _RandomRange_d7266f27a6b9958f837bf09bdc762cdb_Out_3;
        Unity_RandomRange_float(_Add_1842fa0e1536f388b8a6d5f64e3e062f_Out_2, float(0.3), float(0.7), _RandomRange_d7266f27a6b9958f837bf09bdc762cdb_Out_3);
        float2 _Add_d47dc767dcea72828a6821b1dc7669b0_Out_2;
        Unity_Add_float2(_Add_1842fa0e1536f388b8a6d5f64e3e062f_Out_2, float2(1, 1), _Add_d47dc767dcea72828a6821b1dc7669b0_Out_2);
        float _RandomRange_fb667fb6d6ea1f878b072b6a0b019718_Out_3;
        Unity_RandomRange_float(_Add_d47dc767dcea72828a6821b1dc7669b0_Out_2, float(0.3), float(0.7), _RandomRange_fb667fb6d6ea1f878b072b6a0b019718_Out_3);
        float2 _Vector2_527e5a6aebcea4839bc8a823f63125be_Out_0 = float2(_RandomRange_d7266f27a6b9958f837bf09bdc762cdb_Out_3, _RandomRange_fb667fb6d6ea1f878b072b6a0b019718_Out_3);
        float2 _Fraction_722b542af4cde88480600980a547170a_Out_1;
        Unity_Fraction_float2(_TilingAndOffset_054c12590944d280835bf7af8fbcbb8a_Out_3, _Fraction_722b542af4cde88480600980a547170a_Out_1);
        float _Distance_6bfa0e2d94ebc48bbd4fde991bc032fa_Out_2;
        Unity_Distance_float2(_Vector2_527e5a6aebcea4839bc8a823f63125be_Out_0, _Fraction_722b542af4cde88480600980a547170a_Out_1, _Distance_6bfa0e2d94ebc48bbd4fde991bc032fa_Out_2);
        float _Fraction_4bcfd9567d3dca85be4e0af521de8a7c_Out_1;
        Unity_Fraction_float(_Add_66f573fa287cb288ba1bc9cd69e16fda_Out_2, _Fraction_4bcfd9567d3dca85be4e0af521de8a7c_Out_1);
        float _Add_9355d1e3e12bcd8183d6a28fbd898cb2_Out_2;
        Unity_Add_float(_Fraction_4bcfd9567d3dca85be4e0af521de8a7c_Out_1, float(-0.3), _Add_9355d1e3e12bcd8183d6a28fbd898cb2_Out_2);
        float _Subtract_faef160017a9c983ad052cbecc70feda_Out_2;
        Unity_Subtract_float(_Distance_6bfa0e2d94ebc48bbd4fde991bc032fa_Out_2, _Add_9355d1e3e12bcd8183d6a28fbd898cb2_Out_2, _Subtract_faef160017a9c983ad052cbecc70feda_Out_2);
        float4 _SampleGradient_8d5fe46189810380ac3a508fe44b59d0_Out_2;
        Unity_SampleGradientV0_float(_Property_9bccf34d24839482816c85b42874663b_Out_0, _Subtract_faef160017a9c983ad052cbecc70feda_Out_2, _SampleGradient_8d5fe46189810380ac3a508fe44b59d0_Out_2);
        Gradient _Property_0c08dbb904c8cb84ae02c68bb638aded_Out_0 = Gradient_AFB6B065;
        float4 _SampleGradient_616374cca7d265828808200a0609432b_Out_2;
        Unity_SampleGradientV0_float(_Property_0c08dbb904c8cb84ae02c68bb638aded_Out_0, _Fraction_4bcfd9567d3dca85be4e0af521de8a7c_Out_1, _SampleGradient_616374cca7d265828808200a0609432b_Out_2);
        float4 _Lerp_8a47fa5e2810ac80858fc635b9f6b924_Out_3;
        Unity_Lerp_float4(float4(1, 1, 1, 1), _SampleGradient_8d5fe46189810380ac3a508fe44b59d0_Out_2, _SampleGradient_616374cca7d265828808200a0609432b_Out_2, _Lerp_8a47fa5e2810ac80858fc635b9f6b924_Out_3);
        float3 _NormalFromHeight_cb5f128a1f05298fb493e02ae043a713_Out_1;
        float3x3 _NormalFromHeight_cb5f128a1f05298fb493e02ae043a713_TangentMatrix = float3x3(IN.WorldSpaceTangent, IN.WorldSpaceBiTangent, IN.WorldSpaceNormal);
        float3 _NormalFromHeight_cb5f128a1f05298fb493e02ae043a713_Position = IN.WorldSpacePosition;
        Unity_NormalFromHeight_Tangent_float((_Lerp_8a47fa5e2810ac80858fc635b9f6b924_Out_3).x,float(0.01),_NormalFromHeight_cb5f128a1f05298fb493e02ae043a713_Position,_NormalFromHeight_cb5f128a1f05298fb493e02ae043a713_TangentMatrix, _NormalFromHeight_cb5f128a1f05298fb493e02ae043a713_Out_1);
        float _Property_2c37c06e20892a80b173de7ffa4ab033_Out_0 = Vector1_A606AD0F;
        float3 _NormalStrength_bf4bdc1d176af585801fc908c9d75602_Out_2;
        Unity_NormalStrength_float(_NormalFromHeight_cb5f128a1f05298fb493e02ae043a713_Out_1, _Property_2c37c06e20892a80b173de7ffa4ab033_Out_0, _NormalStrength_bf4bdc1d176af585801fc908c9d75602_Out_2);
        OutVector3_1 = _NormalStrength_bf4bdc1d176af585801fc908c9d75602_Out_2;
        }
        
        void Unity_NormalBlend_float(float3 A, float3 B, out float3 Out)
        {
            Out = SafeNormalize(float3(A.rg + B.rg, A.b * B.b));
        }
        
        void Unity_Add_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A + B;
        }
        
void Unity_SceneColor_float(float4 UV, out float3 Out)
        {
            Out = tex2D(_GrabTexture, UV.xy);
        }
        
        void Unity_SceneDepth_Eye_float(float4 UV, out float Out)
        {
            if (unity_OrthoParams.w == 1.0)
            {
                Out = LinearEyeDepth(ComputeWorldSpacePosition(UV.xy, SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV.xy), UNITY_MATRIX_I_VP), UNITY_MATRIX_V);
            }
            else
            {
                Out = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV.xy), _ZBufferParams);
            }
        }
        
        void Unity_Power_float(float A, float B, out float Out)
        {
            Out = pow(A, B);
        }
        
        void Unity_Saturate_float(float In, out float Out)
        {
            Out = saturate(In);
        }
        
        void Unity_Lerp_float3(float3 A, float3 B, float3 T, out float3 Out)
        {
            Out = lerp(A, B, T);
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float3 BaseColor;
            float3 NormalTS;
            float3 Emission;
            float Metallic;
            float Smoothness;
            float Occlusion;
            float Alpha;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float4 _ScreenPosition_b4a8e582e1e849869b726cda686cfcc9_Out_0 = float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0);
            float _Property_9d6b87e68331c08eb165f43406bf9436_Out_0 = Vector1_E5125837;
            Gradient _Gradient_b8eb42ace63c02869968cbad8b8177e5_Out_0 = NewGradient(0, 2, 2, float4(1, 1, 1, 0.3617609),float4(0, 0, 0, 0.597055),float4(0, 0, 0, 0),float4(0, 0, 0, 0),float4(0, 0, 0, 0),float4(0, 0, 0, 0),float4(0, 0, 0, 0),float4(0, 0, 0, 0), float2(1, 0),float2(1, 1),float2(0, 0),float2(0, 0),float2(0, 0),float2(0, 0),float2(0, 0),float2(0, 0));
            Gradient _Gradient_3fae1ac6f27a5884adb30b86f3dcd25b_Out_0 = NewGradient(0, 3, 2, float4(1, 1, 1, 0),float4(0, 0, 0, 0.0411841),float4(1, 1, 1, 0.08529793),float4(0, 0, 0, 0),float4(0, 0, 0, 0),float4(0, 0, 0, 0),float4(0, 0, 0, 0),float4(0, 0, 0, 0), float2(1, 0),float2(1, 1),float2(0, 0),float2(0, 0),float2(0, 0),float2(0, 0),float2(0, 0),float2(0, 0));
            float _Property_a0cfb5db1fb47081ba331d38287cdb6e_Out_0 = Vector1_C7406269;
            float _Property_36028270ca87d38da7655131fbe5e971_Out_0 = Vector1_76A233A4;
            Bindings_RipplesSubGraph_8c331c1756d2a9b47b80805d5e9b5760_float _RipplesSubGraph_3b6545772f8b41e184862b0bb331f760;
            _RipplesSubGraph_3b6545772f8b41e184862b0bb331f760.WorldSpaceNormal = IN.WorldSpaceNormal;
            _RipplesSubGraph_3b6545772f8b41e184862b0bb331f760.WorldSpaceTangent = IN.WorldSpaceTangent;
            _RipplesSubGraph_3b6545772f8b41e184862b0bb331f760.WorldSpaceBiTangent = IN.WorldSpaceBiTangent;
            _RipplesSubGraph_3b6545772f8b41e184862b0bb331f760.WorldSpacePosition = IN.WorldSpacePosition;
            _RipplesSubGraph_3b6545772f8b41e184862b0bb331f760.uv0 = IN.uv0;
            float3 _RipplesSubGraph_3b6545772f8b41e184862b0bb331f760_OutVector3_1;
            SG_RipplesSubGraph_8c331c1756d2a9b47b80805d5e9b5760_float(_Property_9d6b87e68331c08eb165f43406bf9436_Out_0, _Gradient_b8eb42ace63c02869968cbad8b8177e5_Out_0, _Gradient_3fae1ac6f27a5884adb30b86f3dcd25b_Out_0, _Property_a0cfb5db1fb47081ba331d38287cdb6e_Out_0, IN.TimeParameters.x, _Property_36028270ca87d38da7655131fbe5e971_Out_0, float2 (0, 0), _RipplesSubGraph_3b6545772f8b41e184862b0bb331f760, _RipplesSubGraph_3b6545772f8b41e184862b0bb331f760_OutVector3_1);
            float _Add_d5f962a23d1d47c489c9884b8ec32f01_Out_2;
            Unity_Add_float(IN.TimeParameters.x, float(17.94), _Add_d5f962a23d1d47c489c9884b8ec32f01_Out_2);
            float _Property_312cadf13743d98cb6389ba7879b2ff1_Out_0 = Vector1_76A233A4;
            float _Multiply_b7a01f92c625c48ba25de798aaa70f2a_Out_2;
            Unity_Multiply_float_float(_Property_312cadf13743d98cb6389ba7879b2ff1_Out_0, 0.5, _Multiply_b7a01f92c625c48ba25de798aaa70f2a_Out_2);
            Bindings_RipplesSubGraph_8c331c1756d2a9b47b80805d5e9b5760_float _RipplesSubGraph_e07137b991464b7b9a9828b5c96f81a6;
            _RipplesSubGraph_e07137b991464b7b9a9828b5c96f81a6.WorldSpaceNormal = IN.WorldSpaceNormal;
            _RipplesSubGraph_e07137b991464b7b9a9828b5c96f81a6.WorldSpaceTangent = IN.WorldSpaceTangent;
            _RipplesSubGraph_e07137b991464b7b9a9828b5c96f81a6.WorldSpaceBiTangent = IN.WorldSpaceBiTangent;
            _RipplesSubGraph_e07137b991464b7b9a9828b5c96f81a6.WorldSpacePosition = IN.WorldSpacePosition;
            _RipplesSubGraph_e07137b991464b7b9a9828b5c96f81a6.uv0 = IN.uv0;
            float3 _RipplesSubGraph_e07137b991464b7b9a9828b5c96f81a6_OutVector3_1;
            SG_RipplesSubGraph_8c331c1756d2a9b47b80805d5e9b5760_float(_Property_9d6b87e68331c08eb165f43406bf9436_Out_0, _Gradient_b8eb42ace63c02869968cbad8b8177e5_Out_0, _Gradient_3fae1ac6f27a5884adb30b86f3dcd25b_Out_0, _Property_a0cfb5db1fb47081ba331d38287cdb6e_Out_0, _Add_d5f962a23d1d47c489c9884b8ec32f01_Out_2, _Multiply_b7a01f92c625c48ba25de798aaa70f2a_Out_2, float2 (0, 0), _RipplesSubGraph_e07137b991464b7b9a9828b5c96f81a6, _RipplesSubGraph_e07137b991464b7b9a9828b5c96f81a6_OutVector3_1);
            float3 _NormalBlend_f0858e7eedf2728ebdc013bdd4ec0577_Out_2;
            Unity_NormalBlend_float(_RipplesSubGraph_3b6545772f8b41e184862b0bb331f760_OutVector3_1, _RipplesSubGraph_e07137b991464b7b9a9828b5c96f81a6_OutVector3_1, _NormalBlend_f0858e7eedf2728ebdc013bdd4ec0577_Out_2);
            float3 _Add_6ef18bc46f03d388a7f1ff8beac95197_Out_2;
            Unity_Add_float3((_ScreenPosition_b4a8e582e1e849869b726cda686cfcc9_Out_0.xyz), _NormalBlend_f0858e7eedf2728ebdc013bdd4ec0577_Out_2, _Add_6ef18bc46f03d388a7f1ff8beac95197_Out_2);
            float3 _SceneColor_7f3fccc718c9208ea05c245d4f2f6794_Out_1;
            Unity_SceneColor_float((float4(_Add_6ef18bc46f03d388a7f1ff8beac95197_Out_2, 1.0)), _SceneColor_7f3fccc718c9208ea05c245d4f2f6794_Out_1);
            float4 _Property_c5168c0788e20d88a161c906be99b878_Out_0 = Color_C343EF87;
            float _Property_d9b056fed78dd085b4d0698251bc8753_Out_0 = Vector1_2FA69C8F;
            float _SceneDepth_5041dfd2c8895382ae5d6fd487809adc_Out_1;
            Unity_SceneDepth_Eye_float(float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _SceneDepth_5041dfd2c8895382ae5d6fd487809adc_Out_1);
            float4 _ScreenPosition_8d8b51c518d1818981db05311793998f_Out_0 = IN.ScreenPosition;
            float _Split_8d0366c6012fb585a39dd52842ac3f19_R_1 = _ScreenPosition_8d8b51c518d1818981db05311793998f_Out_0[0];
            float _Split_8d0366c6012fb585a39dd52842ac3f19_G_2 = _ScreenPosition_8d8b51c518d1818981db05311793998f_Out_0[1];
            float _Split_8d0366c6012fb585a39dd52842ac3f19_B_3 = _ScreenPosition_8d8b51c518d1818981db05311793998f_Out_0[2];
            float _Split_8d0366c6012fb585a39dd52842ac3f19_A_4 = _ScreenPosition_8d8b51c518d1818981db05311793998f_Out_0[3];
            float _Subtract_c825aee699188186ad9c2b51c72d4b8d_Out_2;
            Unity_Subtract_float(_SceneDepth_5041dfd2c8895382ae5d6fd487809adc_Out_1, _Split_8d0366c6012fb585a39dd52842ac3f19_A_4, _Subtract_c825aee699188186ad9c2b51c72d4b8d_Out_2);
            float _Property_3db3e2d81cb1a188aca23ece62d7f199_Out_0 = Vector1_5327F7C2;
            float _Multiply_fa4b52c3a2fad089870ad7808d047486_Out_2;
            Unity_Multiply_float_float(_Property_3db3e2d81cb1a188aca23ece62d7f199_Out_0, 0.1, _Multiply_fa4b52c3a2fad089870ad7808d047486_Out_2);
            float _Power_b8dcd558ef12958284e491078c34dedd_Out_2;
            Unity_Power_float(_Subtract_c825aee699188186ad9c2b51c72d4b8d_Out_2, _Multiply_fa4b52c3a2fad089870ad7808d047486_Out_2, _Power_b8dcd558ef12958284e491078c34dedd_Out_2);
            float _Add_9a13fa6ccec69d8fabcbe0a82247f9f2_Out_2;
            Unity_Add_float(_Property_d9b056fed78dd085b4d0698251bc8753_Out_0, _Power_b8dcd558ef12958284e491078c34dedd_Out_2, _Add_9a13fa6ccec69d8fabcbe0a82247f9f2_Out_2);
            float _Saturate_919586c53b6e588587683723594ef3bd_Out_1;
            Unity_Saturate_float(_Add_9a13fa6ccec69d8fabcbe0a82247f9f2_Out_2, _Saturate_919586c53b6e588587683723594ef3bd_Out_1);
            float3 _Lerp_2117a5b13f63f48887d25214347e82c4_Out_3;
            Unity_Lerp_float3(_SceneColor_7f3fccc718c9208ea05c245d4f2f6794_Out_1, (_Property_c5168c0788e20d88a161c906be99b878_Out_0.xyz), (_Saturate_919586c53b6e588587683723594ef3bd_Out_1.xxx), _Lerp_2117a5b13f63f48887d25214347e82c4_Out_3);
            surface.BaseColor = _Lerp_2117a5b13f63f48887d25214347e82c4_Out_3;
            surface.NormalTS = _NormalBlend_f0858e7eedf2728ebdc013bdd4ec0577_Out_2;
            surface.Emission = float3(0, 0, 0);
            surface.Metallic = float(0);
            surface.Smoothness = float(0.8);
            surface.Occlusion = float(1);
            surface.Alpha = float(1);
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
            
        
            // must use interpolated tangent, bitangent and normal before they are normalized in the pixel shader.
            float3 unnormalizedNormalWS = input.normalWS;
            const float renormFactor = 1.0 / length(unnormalizedNormalWS);
        
            // use bitangent on the fly like in hdrp
            // IMPORTANT! If we ever support Flip on double sided materials ensure bitangent and tangent are NOT flipped.
            float crossSign = (input.tangentWS.w > 0.0 ? 1.0 : -1.0)* GetOddNegativeScale();
            float3 bitang = crossSign * cross(input.normalWS.xyz, input.tangentWS.xyz);
        
            output.WorldSpaceNormal = renormFactor * input.normalWS.xyz;      // we want a unit length Normal Vector node in shader graph
            output.TangentSpaceNormal = float3(0.0f, 0.0f, 1.0f);
        
            // to preserve mikktspace compliance we use same scale renormFactor as was used on the normal.
            // This is explained in section 2.2 in "surface gradient based bump mapping framework"
            output.WorldSpaceTangent = renormFactor * input.tangentWS.xyz;
            output.WorldSpaceBiTangent = renormFactor * bitang;
        
            output.WorldSpacePosition = input.positionWS;
            output.ScreenPosition = ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
            output.uv0 = input.texCoord0;
            output.TimeParameters = _TimeParameters.xyz; // This is mainly for LW as HD overwrite this value
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        void BuildAppDataFull(Attributes attributes, VertexDescription vertexDescription, inout appdata_full result)
        {
            result.vertex     = float4(attributes.positionOS, 1);
            result.tangent    = attributes.tangentOS;
            result.normal     = attributes.normalOS;
            result.texcoord   = attributes.uv0;
            result.texcoord1  = attributes.uv1;
            result.vertex     = float4(vertexDescription.Position, 1);
            result.normal     = vertexDescription.Normal;
            result.tangent    = float4(vertexDescription.Tangent, 0);
            #if UNITY_ANY_INSTANCING_ENABLED
            #endif
        }
        
        void VaryingsToSurfaceVertex(Varyings varyings, inout v2f_surf result)
        {
            result.pos = varyings.positionCS;
            result.worldPos = varyings.positionWS;
            result.worldNormal = varyings.normalWS;
            result.viewDir = varyings.viewDirectionWS;
            // World Tangent isn't an available input on v2f_surf
        
            result._ShadowCoord = varyings.shadowCoord;
        
            #if UNITY_ANY_INSTANCING_ENABLED
            #endif
            #if !defined(LIGHTMAP_ON)
            #if UNITY_SHOULD_SAMPLE_SH
            result.sh = varyings.sh;
            #endif
            #endif
            #if defined(LIGHTMAP_ON)
            result.lmap.xy = varyings.lightmapUV;
            #endif
            #ifdef VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
                result.fogCoord = varyings.fogFactorAndVertexLight.x;
                COPY_TO_LIGHT_COORDS(result, varyings.fogFactorAndVertexLight.yzw);
            #endif
        
            DEFAULT_UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(varyings, result);
        }
        
        void SurfaceVertexToVaryings(v2f_surf surfVertex, inout Varyings result)
        {
            result.positionCS = surfVertex.pos;
            result.positionWS = surfVertex.worldPos;
            result.normalWS = surfVertex.worldNormal;
            // viewDirectionWS is never filled out in the legacy pass' function. Always use the value computed by SRP
            // World Tangent isn't an available input on v2f_surf
            result.shadowCoord = surfVertex._ShadowCoord;
        
            #if UNITY_ANY_INSTANCING_ENABLED
            #endif
            #if !defined(LIGHTMAP_ON)
            #if UNITY_SHOULD_SAMPLE_SH
            result.sh = surfVertex.sh;
            #endif
            #endif
            #if defined(LIGHTMAP_ON)
            result.lightmapUV = surfVertex.lmap.xy;
            #endif
            #ifdef VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
                result.fogFactorAndVertexLight.x = surfVertex.fogCoord;
                COPY_FROM_LIGHT_COORDS(result.fogFactorAndVertexLight.yzw, surfVertex);
            #endif
        
            DEFAULT_UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(surfVertex, result);
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/PBRDeferredPass.hlsl"
        
        ENDHLSL
        }
        Pass
        {
            Name "ShadowCaster"
            Tags
            {
                "LightMode" = "ShadowCaster"
            }
        
        // Render State
        Cull Back
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZTest LEqual
        ZWrite On
        ColorMask 0
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
sampler2D _GrabTexture;
        
        // Pragmas
        #pragma target 3.0
        #pragma multi_compile_shadowcaster
        #pragma vertex vert
        #pragma fragment frag
        
        // DotsInstancingOptions: <None>
        // HybridV1InjectedBuiltinProperties: <None>
        
        // Keywords
        #pragma multi_compile _ _CASTING_PUNCTUAL_LIGHT_SHADOW
        // GraphKeywords: <None>
        
        // Defines
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_SHADOWCASTER
        #define BUILTIN_TARGET_API 1
        #define _BUILTIN_SURFACE_TYPE_TRANSPARENT 1
        /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */
        #ifdef _BUILTIN_SURFACE_TYPE_TRANSPARENT
        #define _SURFACE_TYPE_TRANSPARENT _BUILTIN_SURFACE_TYPE_TRANSPARENT
        #endif
        #ifdef _BUILTIN_ALPHATEST_ON
        #define _ALPHATEST_ON _BUILTIN_ALPHATEST_ON
        #endif
        #ifdef _BUILTIN_AlphaClip
        #define _AlphaClip _BUILTIN_AlphaClip
        #endif
        #ifdef _BUILTIN_ALPHAPREMULTIPLY_ON
        #define _ALPHAPREMULTIPLY_ON _BUILTIN_ALPHAPREMULTIPLY_ON
        #endif
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/Shim/Shims.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/LegacySurfaceVertex.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/ShaderGraphFunctions.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float Vector1_E5125837;
        float Vector1_C7406269;
        float Vector1_76A233A4;
        float Vector1_5327F7C2;
        float Vector1_2FA69C8F;
        float4 Color_C343EF87;
        CBUFFER_END
        
        // Object and Global properties
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // Graph Functions
        // GraphFunctions: <None>
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float Alpha;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            surface.Alpha = float(1);
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
            
        
        
        
        
        
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        void BuildAppDataFull(Attributes attributes, VertexDescription vertexDescription, inout appdata_full result)
        {
            result.vertex     = float4(attributes.positionOS, 1);
            result.tangent    = attributes.tangentOS;
            result.normal     = attributes.normalOS;
            result.vertex     = float4(vertexDescription.Position, 1);
            result.normal     = vertexDescription.Normal;
            result.tangent    = float4(vertexDescription.Tangent, 0);
            #if UNITY_ANY_INSTANCING_ENABLED
            #endif
        }
        
        void VaryingsToSurfaceVertex(Varyings varyings, inout v2f_surf result)
        {
            result.pos = varyings.positionCS;
            // World Tangent isn't an available input on v2f_surf
        
        
            #if UNITY_ANY_INSTANCING_ENABLED
            #endif
            #if !defined(LIGHTMAP_ON)
            #if UNITY_SHOULD_SAMPLE_SH
            #endif
            #endif
            #if defined(LIGHTMAP_ON)
            #endif
            #ifdef VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
                result.fogCoord = varyings.fogFactorAndVertexLight.x;
                COPY_TO_LIGHT_COORDS(result, varyings.fogFactorAndVertexLight.yzw);
            #endif
        
            DEFAULT_UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(varyings, result);
        }
        
        void SurfaceVertexToVaryings(v2f_surf surfVertex, inout Varyings result)
        {
            result.positionCS = surfVertex.pos;
            // viewDirectionWS is never filled out in the legacy pass' function. Always use the value computed by SRP
            // World Tangent isn't an available input on v2f_surf
        
            #if UNITY_ANY_INSTANCING_ENABLED
            #endif
            #if !defined(LIGHTMAP_ON)
            #if UNITY_SHOULD_SAMPLE_SH
            #endif
            #endif
            #if defined(LIGHTMAP_ON)
            #endif
            #ifdef VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
                result.fogFactorAndVertexLight.x = surfVertex.fogCoord;
                COPY_FROM_LIGHT_COORDS(result.fogFactorAndVertexLight.yzw, surfVertex);
            #endif
        
            DEFAULT_UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(surfVertex, result);
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/ShadowCasterPass.hlsl"
        
        ENDHLSL
        }
        Pass
        {
            Name "Meta"
            Tags
            {
                "LightMode" = "Meta"
            }
        
        // Render State
        Cull Off
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
sampler2D _GrabTexture;
        
        // Pragmas
        #pragma target 3.0
        #pragma vertex vert
        #pragma fragment frag
        
        // DotsInstancingOptions: <None>
        // HybridV1InjectedBuiltinProperties: <None>
        
        // Keywords
        #pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
        // GraphKeywords: <None>
        
        // Defines
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define ATTRIBUTES_NEED_TEXCOORD1
        #define ATTRIBUTES_NEED_TEXCOORD2
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_NORMAL_WS
        #define VARYINGS_NEED_TANGENT_WS
        #define VARYINGS_NEED_TEXCOORD0
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_META
        #define BUILTIN_TARGET_API 1
        #define _BUILTIN_SURFACE_TYPE_TRANSPARENT 1
        #define REQUIRE_DEPTH_TEXTURE
        #define REQUIRE_OPAQUE_TEXTURE
        /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */
        #ifdef _BUILTIN_SURFACE_TYPE_TRANSPARENT
        #define _SURFACE_TYPE_TRANSPARENT _BUILTIN_SURFACE_TYPE_TRANSPARENT
        #endif
        #ifdef _BUILTIN_ALPHATEST_ON
        #define _ALPHATEST_ON _BUILTIN_ALPHATEST_ON
        #endif
        #ifdef _BUILTIN_AlphaClip
        #define _AlphaClip _BUILTIN_AlphaClip
        #endif
        #ifdef _BUILTIN_ALPHAPREMULTIPLY_ON
        #define _ALPHAPREMULTIPLY_ON _BUILTIN_ALPHAPREMULTIPLY_ON
        #endif
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/Shim/Shims.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/LegacySurfaceVertex.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/ShaderGraphFunctions.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
             float4 uv1 : TEXCOORD1;
             float4 uv2 : TEXCOORD2;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 positionWS;
             float3 normalWS;
             float4 tangentWS;
             float4 texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float3 WorldSpaceNormal;
             float3 WorldSpaceTangent;
             float3 WorldSpaceBiTangent;
             float3 WorldSpacePosition;
             float4 ScreenPosition;
             float4 uv0;
             float3 TimeParameters;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float4 tangentWS : INTERP0;
             float4 texCoord0 : INTERP1;
             float3 positionWS : INTERP2;
             float3 normalWS : INTERP3;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.tangentWS.xyzw = input.tangentWS;
            output.texCoord0.xyzw = input.texCoord0;
            output.positionWS.xyz = input.positionWS;
            output.normalWS.xyz = input.normalWS;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.tangentWS = input.tangentWS.xyzw;
            output.texCoord0 = input.texCoord0.xyzw;
            output.positionWS = input.positionWS.xyz;
            output.normalWS = input.normalWS.xyz;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float Vector1_E5125837;
        float Vector1_C7406269;
        float Vector1_76A233A4;
        float Vector1_5327F7C2;
        float Vector1_2FA69C8F;
        float4 Color_C343EF87;
        CBUFFER_END
        
        // Object and Global properties
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // Graph Functions
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
        Out = A * B;
        }
        
        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        void Unity_Floor_float2(float2 In, out float2 Out)
        {
            Out = floor(In);
        }
        
        void Unity_RandomRange_float(float2 Seed, float Min, float Max, out float Out)
        {
             float randomno =  frac(sin(dot(Seed, float2(12.9898, 78.233)))*43758.5453);
             Out = lerp(Min, Max, randomno);
        }
        
        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }
        
        void Unity_Floor_float(float In, out float Out)
        {
            Out = floor(In);
        }
        
        void Unity_Add_float2(float2 A, float2 B, out float2 Out)
        {
            Out = A + B;
        }
        
        void Unity_Fraction_float2(float2 In, out float2 Out)
        {
            Out = frac(In);
        }
        
        void Unity_Distance_float2(float2 A, float2 B, out float Out)
        {
            Out = distance(A, B);
        }
        
        void Unity_Fraction_float(float In, out float Out)
        {
            Out = frac(In);
        }
        
        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }
        
        void Unity_SampleGradientV0_float(Gradient Gradient, float Time, out float4 Out)
        {
            float3 color = Gradient.colors[0].rgb;
            [unroll]
            for (int c = 1; c < Gradient.colorsLength; c++)
            {
                float colorPos = saturate((Time - Gradient.colors[c - 1].w) / (Gradient.colors[c].w - Gradient.colors[c - 1].w)) * step(c, Gradient.colorsLength - 1);
                color = lerp(color, Gradient.colors[c].rgb, lerp(colorPos, step(0.01, colorPos), Gradient.type));
            }
        #ifndef UNITY_COLORSPACE_GAMMA
            color = SRGBToLinear(color);
        #endif
            float alpha = Gradient.alphas[0].x;
            [unroll]
            for (int a = 1; a < Gradient.alphasLength; a++)
            {
                float alphaPos = saturate((Time - Gradient.alphas[a - 1].y) / (Gradient.alphas[a].y - Gradient.alphas[a - 1].y)) * step(a, Gradient.alphasLength - 1);
                alpha = lerp(alpha, Gradient.alphas[a].x, lerp(alphaPos, step(0.01, alphaPos), Gradient.type));
            }
            Out = float4(color, alpha);
        }
        
        void Unity_Lerp_float4(float4 A, float4 B, float4 T, out float4 Out)
        {
            Out = lerp(A, B, T);
        }
        
        void Unity_NormalFromHeight_Tangent_float(float In, float Strength, float3 Position, float3x3 TangentMatrix, out float3 Out)
        {
        
                    #if defined(SHADER_STAGE_RAY_TRACING) && defined(RAYTRACING_SHADER_GRAPH_DEFAULT)
                    #error 'Normal From Height' node is not supported in ray tracing, please provide an alternate implementation, relying for instance on the 'Raytracing Quality' keyword
                    #endif
        float3 worldDerivativeX = ddx(Position);
        float3 worldDerivativeY = ddy(Position);
        
        float3 crossX = cross(TangentMatrix[2].xyz, worldDerivativeX);
        float3 crossY = cross(worldDerivativeY, TangentMatrix[2].xyz);
        float d = dot(worldDerivativeX, crossY);
        float sgn = d < 0.0 ? (-1.0f) : 1.0f;
        float surface = sgn / max(0.000000000000001192093f, abs(d));
        
        float dHdx = ddx(In);
        float dHdy = ddy(In);
        float3 surfGrad = surface * (dHdx*crossY + dHdy*crossX);
        Out = SafeNormalize(TangentMatrix[2].xyz - (Strength * surfGrad));
        Out = TransformWorldToTangent(Out, TangentMatrix);
        }
        
        void Unity_NormalStrength_float(float3 In, float Strength, out float3 Out)
        {
            Out = float3(In.rg * Strength, lerp(1, In.b, saturate(Strength)));
        }
        
        struct Bindings_RipplesSubGraph_8c331c1756d2a9b47b80805d5e9b5760_float
        {
        float3 WorldSpaceNormal;
        float3 WorldSpaceTangent;
        float3 WorldSpaceBiTangent;
        float3 WorldSpacePosition;
        half4 uv0;
        };
        
        void SG_RipplesSubGraph_8c331c1756d2a9b47b80805d5e9b5760_float(float Vector1_A606AD0F, Gradient Gradient_AFB6B065, Gradient Gradient_612A0EE6, float Vector1_38564B71, float Vector1_48991907, float Vector1_E03EF315, float2 Vector2_8FC8A8DE, Bindings_RipplesSubGraph_8c331c1756d2a9b47b80805d5e9b5760_float IN, out float3 OutVector3_1)
        {
        Gradient _Property_9bccf34d24839482816c85b42874663b_Out_0 = Gradient_612A0EE6;
        float _Property_89d96fee4ae9b58596cca58f9ca66d82_Out_0 = Vector1_38564B71;
        float _Property_e7381c82bf07f28e87e55268633f03a6_Out_0 = Vector1_48991907;
        float _Multiply_2b7f465bf86b238ba88c3abadfb4dc6c_Out_2;
        Unity_Multiply_float_float(_Property_89d96fee4ae9b58596cca58f9ca66d82_Out_0, _Property_e7381c82bf07f28e87e55268633f03a6_Out_0, _Multiply_2b7f465bf86b238ba88c3abadfb4dc6c_Out_2);
        float _Property_c9b401be3cccc487963a18b18a5b00c5_Out_0 = Vector1_E03EF315;
        float2 _Property_7fd9512f128cc584b63bd9ba93e3805b_Out_0 = Vector2_8FC8A8DE;
        float2 _TilingAndOffset_054c12590944d280835bf7af8fbcbb8a_Out_3;
        Unity_TilingAndOffset_float(IN.uv0.xy, (_Property_c9b401be3cccc487963a18b18a5b00c5_Out_0.xx), _Property_7fd9512f128cc584b63bd9ba93e3805b_Out_0, _TilingAndOffset_054c12590944d280835bf7af8fbcbb8a_Out_3);
        float2 _Floor_b11cd06caee6518891d995cc8a1ef27a_Out_1;
        Unity_Floor_float2(_TilingAndOffset_054c12590944d280835bf7af8fbcbb8a_Out_3, _Floor_b11cd06caee6518891d995cc8a1ef27a_Out_1);
        float _RandomRange_715aacb97fba9b858184047a525d5dab_Out_3;
        Unity_RandomRange_float(_Floor_b11cd06caee6518891d995cc8a1ef27a_Out_1, float(0), float(1), _RandomRange_715aacb97fba9b858184047a525d5dab_Out_3);
        float _Add_66f573fa287cb288ba1bc9cd69e16fda_Out_2;
        Unity_Add_float(_Multiply_2b7f465bf86b238ba88c3abadfb4dc6c_Out_2, _RandomRange_715aacb97fba9b858184047a525d5dab_Out_3, _Add_66f573fa287cb288ba1bc9cd69e16fda_Out_2);
        float _Floor_751a8067250b49808f479855f31ee1a7_Out_1;
        Unity_Floor_float(_Add_66f573fa287cb288ba1bc9cd69e16fda_Out_2, _Floor_751a8067250b49808f479855f31ee1a7_Out_1);
        float2 _Add_1842fa0e1536f388b8a6d5f64e3e062f_Out_2;
        Unity_Add_float2((_Floor_751a8067250b49808f479855f31ee1a7_Out_1.xx), _Floor_b11cd06caee6518891d995cc8a1ef27a_Out_1, _Add_1842fa0e1536f388b8a6d5f64e3e062f_Out_2);
        float _RandomRange_d7266f27a6b9958f837bf09bdc762cdb_Out_3;
        Unity_RandomRange_float(_Add_1842fa0e1536f388b8a6d5f64e3e062f_Out_2, float(0.3), float(0.7), _RandomRange_d7266f27a6b9958f837bf09bdc762cdb_Out_3);
        float2 _Add_d47dc767dcea72828a6821b1dc7669b0_Out_2;
        Unity_Add_float2(_Add_1842fa0e1536f388b8a6d5f64e3e062f_Out_2, float2(1, 1), _Add_d47dc767dcea72828a6821b1dc7669b0_Out_2);
        float _RandomRange_fb667fb6d6ea1f878b072b6a0b019718_Out_3;
        Unity_RandomRange_float(_Add_d47dc767dcea72828a6821b1dc7669b0_Out_2, float(0.3), float(0.7), _RandomRange_fb667fb6d6ea1f878b072b6a0b019718_Out_3);
        float2 _Vector2_527e5a6aebcea4839bc8a823f63125be_Out_0 = float2(_RandomRange_d7266f27a6b9958f837bf09bdc762cdb_Out_3, _RandomRange_fb667fb6d6ea1f878b072b6a0b019718_Out_3);
        float2 _Fraction_722b542af4cde88480600980a547170a_Out_1;
        Unity_Fraction_float2(_TilingAndOffset_054c12590944d280835bf7af8fbcbb8a_Out_3, _Fraction_722b542af4cde88480600980a547170a_Out_1);
        float _Distance_6bfa0e2d94ebc48bbd4fde991bc032fa_Out_2;
        Unity_Distance_float2(_Vector2_527e5a6aebcea4839bc8a823f63125be_Out_0, _Fraction_722b542af4cde88480600980a547170a_Out_1, _Distance_6bfa0e2d94ebc48bbd4fde991bc032fa_Out_2);
        float _Fraction_4bcfd9567d3dca85be4e0af521de8a7c_Out_1;
        Unity_Fraction_float(_Add_66f573fa287cb288ba1bc9cd69e16fda_Out_2, _Fraction_4bcfd9567d3dca85be4e0af521de8a7c_Out_1);
        float _Add_9355d1e3e12bcd8183d6a28fbd898cb2_Out_2;
        Unity_Add_float(_Fraction_4bcfd9567d3dca85be4e0af521de8a7c_Out_1, float(-0.3), _Add_9355d1e3e12bcd8183d6a28fbd898cb2_Out_2);
        float _Subtract_faef160017a9c983ad052cbecc70feda_Out_2;
        Unity_Subtract_float(_Distance_6bfa0e2d94ebc48bbd4fde991bc032fa_Out_2, _Add_9355d1e3e12bcd8183d6a28fbd898cb2_Out_2, _Subtract_faef160017a9c983ad052cbecc70feda_Out_2);
        float4 _SampleGradient_8d5fe46189810380ac3a508fe44b59d0_Out_2;
        Unity_SampleGradientV0_float(_Property_9bccf34d24839482816c85b42874663b_Out_0, _Subtract_faef160017a9c983ad052cbecc70feda_Out_2, _SampleGradient_8d5fe46189810380ac3a508fe44b59d0_Out_2);
        Gradient _Property_0c08dbb904c8cb84ae02c68bb638aded_Out_0 = Gradient_AFB6B065;
        float4 _SampleGradient_616374cca7d265828808200a0609432b_Out_2;
        Unity_SampleGradientV0_float(_Property_0c08dbb904c8cb84ae02c68bb638aded_Out_0, _Fraction_4bcfd9567d3dca85be4e0af521de8a7c_Out_1, _SampleGradient_616374cca7d265828808200a0609432b_Out_2);
        float4 _Lerp_8a47fa5e2810ac80858fc635b9f6b924_Out_3;
        Unity_Lerp_float4(float4(1, 1, 1, 1), _SampleGradient_8d5fe46189810380ac3a508fe44b59d0_Out_2, _SampleGradient_616374cca7d265828808200a0609432b_Out_2, _Lerp_8a47fa5e2810ac80858fc635b9f6b924_Out_3);
        float3 _NormalFromHeight_cb5f128a1f05298fb493e02ae043a713_Out_1;
        float3x3 _NormalFromHeight_cb5f128a1f05298fb493e02ae043a713_TangentMatrix = float3x3(IN.WorldSpaceTangent, IN.WorldSpaceBiTangent, IN.WorldSpaceNormal);
        float3 _NormalFromHeight_cb5f128a1f05298fb493e02ae043a713_Position = IN.WorldSpacePosition;
        Unity_NormalFromHeight_Tangent_float((_Lerp_8a47fa5e2810ac80858fc635b9f6b924_Out_3).x,float(0.01),_NormalFromHeight_cb5f128a1f05298fb493e02ae043a713_Position,_NormalFromHeight_cb5f128a1f05298fb493e02ae043a713_TangentMatrix, _NormalFromHeight_cb5f128a1f05298fb493e02ae043a713_Out_1);
        float _Property_2c37c06e20892a80b173de7ffa4ab033_Out_0 = Vector1_A606AD0F;
        float3 _NormalStrength_bf4bdc1d176af585801fc908c9d75602_Out_2;
        Unity_NormalStrength_float(_NormalFromHeight_cb5f128a1f05298fb493e02ae043a713_Out_1, _Property_2c37c06e20892a80b173de7ffa4ab033_Out_0, _NormalStrength_bf4bdc1d176af585801fc908c9d75602_Out_2);
        OutVector3_1 = _NormalStrength_bf4bdc1d176af585801fc908c9d75602_Out_2;
        }
        
        void Unity_NormalBlend_float(float3 A, float3 B, out float3 Out)
        {
            Out = SafeNormalize(float3(A.rg + B.rg, A.b * B.b));
        }
        
        void Unity_Add_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A + B;
        }
        
void Unity_SceneColor_float(float4 UV, out float3 Out)
        {
            Out = tex2D(_GrabTexture, UV.xy);
        }
        
        void Unity_SceneDepth_Eye_float(float4 UV, out float Out)
        {
            if (unity_OrthoParams.w == 1.0)
            {
                Out = LinearEyeDepth(ComputeWorldSpacePosition(UV.xy, SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV.xy), UNITY_MATRIX_I_VP), UNITY_MATRIX_V);
            }
            else
            {
                Out = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV.xy), _ZBufferParams);
            }
        }
        
        void Unity_Power_float(float A, float B, out float Out)
        {
            Out = pow(A, B);
        }
        
        void Unity_Saturate_float(float In, out float Out)
        {
            Out = saturate(In);
        }
        
        void Unity_Lerp_float3(float3 A, float3 B, float3 T, out float3 Out)
        {
            Out = lerp(A, B, T);
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float3 BaseColor;
            float3 Emission;
            float Alpha;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float4 _ScreenPosition_b4a8e582e1e849869b726cda686cfcc9_Out_0 = float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0);
            float _Property_9d6b87e68331c08eb165f43406bf9436_Out_0 = Vector1_E5125837;
            Gradient _Gradient_b8eb42ace63c02869968cbad8b8177e5_Out_0 = NewGradient(0, 2, 2, float4(1, 1, 1, 0.3617609),float4(0, 0, 0, 0.597055),float4(0, 0, 0, 0),float4(0, 0, 0, 0),float4(0, 0, 0, 0),float4(0, 0, 0, 0),float4(0, 0, 0, 0),float4(0, 0, 0, 0), float2(1, 0),float2(1, 1),float2(0, 0),float2(0, 0),float2(0, 0),float2(0, 0),float2(0, 0),float2(0, 0));
            Gradient _Gradient_3fae1ac6f27a5884adb30b86f3dcd25b_Out_0 = NewGradient(0, 3, 2, float4(1, 1, 1, 0),float4(0, 0, 0, 0.0411841),float4(1, 1, 1, 0.08529793),float4(0, 0, 0, 0),float4(0, 0, 0, 0),float4(0, 0, 0, 0),float4(0, 0, 0, 0),float4(0, 0, 0, 0), float2(1, 0),float2(1, 1),float2(0, 0),float2(0, 0),float2(0, 0),float2(0, 0),float2(0, 0),float2(0, 0));
            float _Property_a0cfb5db1fb47081ba331d38287cdb6e_Out_0 = Vector1_C7406269;
            float _Property_36028270ca87d38da7655131fbe5e971_Out_0 = Vector1_76A233A4;
            Bindings_RipplesSubGraph_8c331c1756d2a9b47b80805d5e9b5760_float _RipplesSubGraph_3b6545772f8b41e184862b0bb331f760;
            _RipplesSubGraph_3b6545772f8b41e184862b0bb331f760.WorldSpaceNormal = IN.WorldSpaceNormal;
            _RipplesSubGraph_3b6545772f8b41e184862b0bb331f760.WorldSpaceTangent = IN.WorldSpaceTangent;
            _RipplesSubGraph_3b6545772f8b41e184862b0bb331f760.WorldSpaceBiTangent = IN.WorldSpaceBiTangent;
            _RipplesSubGraph_3b6545772f8b41e184862b0bb331f760.WorldSpacePosition = IN.WorldSpacePosition;
            _RipplesSubGraph_3b6545772f8b41e184862b0bb331f760.uv0 = IN.uv0;
            float3 _RipplesSubGraph_3b6545772f8b41e184862b0bb331f760_OutVector3_1;
            SG_RipplesSubGraph_8c331c1756d2a9b47b80805d5e9b5760_float(_Property_9d6b87e68331c08eb165f43406bf9436_Out_0, _Gradient_b8eb42ace63c02869968cbad8b8177e5_Out_0, _Gradient_3fae1ac6f27a5884adb30b86f3dcd25b_Out_0, _Property_a0cfb5db1fb47081ba331d38287cdb6e_Out_0, IN.TimeParameters.x, _Property_36028270ca87d38da7655131fbe5e971_Out_0, float2 (0, 0), _RipplesSubGraph_3b6545772f8b41e184862b0bb331f760, _RipplesSubGraph_3b6545772f8b41e184862b0bb331f760_OutVector3_1);
            float _Add_d5f962a23d1d47c489c9884b8ec32f01_Out_2;
            Unity_Add_float(IN.TimeParameters.x, float(17.94), _Add_d5f962a23d1d47c489c9884b8ec32f01_Out_2);
            float _Property_312cadf13743d98cb6389ba7879b2ff1_Out_0 = Vector1_76A233A4;
            float _Multiply_b7a01f92c625c48ba25de798aaa70f2a_Out_2;
            Unity_Multiply_float_float(_Property_312cadf13743d98cb6389ba7879b2ff1_Out_0, 0.5, _Multiply_b7a01f92c625c48ba25de798aaa70f2a_Out_2);
            Bindings_RipplesSubGraph_8c331c1756d2a9b47b80805d5e9b5760_float _RipplesSubGraph_e07137b991464b7b9a9828b5c96f81a6;
            _RipplesSubGraph_e07137b991464b7b9a9828b5c96f81a6.WorldSpaceNormal = IN.WorldSpaceNormal;
            _RipplesSubGraph_e07137b991464b7b9a9828b5c96f81a6.WorldSpaceTangent = IN.WorldSpaceTangent;
            _RipplesSubGraph_e07137b991464b7b9a9828b5c96f81a6.WorldSpaceBiTangent = IN.WorldSpaceBiTangent;
            _RipplesSubGraph_e07137b991464b7b9a9828b5c96f81a6.WorldSpacePosition = IN.WorldSpacePosition;
            _RipplesSubGraph_e07137b991464b7b9a9828b5c96f81a6.uv0 = IN.uv0;
            float3 _RipplesSubGraph_e07137b991464b7b9a9828b5c96f81a6_OutVector3_1;
            SG_RipplesSubGraph_8c331c1756d2a9b47b80805d5e9b5760_float(_Property_9d6b87e68331c08eb165f43406bf9436_Out_0, _Gradient_b8eb42ace63c02869968cbad8b8177e5_Out_0, _Gradient_3fae1ac6f27a5884adb30b86f3dcd25b_Out_0, _Property_a0cfb5db1fb47081ba331d38287cdb6e_Out_0, _Add_d5f962a23d1d47c489c9884b8ec32f01_Out_2, _Multiply_b7a01f92c625c48ba25de798aaa70f2a_Out_2, float2 (0, 0), _RipplesSubGraph_e07137b991464b7b9a9828b5c96f81a6, _RipplesSubGraph_e07137b991464b7b9a9828b5c96f81a6_OutVector3_1);
            float3 _NormalBlend_f0858e7eedf2728ebdc013bdd4ec0577_Out_2;
            Unity_NormalBlend_float(_RipplesSubGraph_3b6545772f8b41e184862b0bb331f760_OutVector3_1, _RipplesSubGraph_e07137b991464b7b9a9828b5c96f81a6_OutVector3_1, _NormalBlend_f0858e7eedf2728ebdc013bdd4ec0577_Out_2);
            float3 _Add_6ef18bc46f03d388a7f1ff8beac95197_Out_2;
            Unity_Add_float3((_ScreenPosition_b4a8e582e1e849869b726cda686cfcc9_Out_0.xyz), _NormalBlend_f0858e7eedf2728ebdc013bdd4ec0577_Out_2, _Add_6ef18bc46f03d388a7f1ff8beac95197_Out_2);
            float3 _SceneColor_7f3fccc718c9208ea05c245d4f2f6794_Out_1;
            Unity_SceneColor_float((float4(_Add_6ef18bc46f03d388a7f1ff8beac95197_Out_2, 1.0)), _SceneColor_7f3fccc718c9208ea05c245d4f2f6794_Out_1);
            float4 _Property_c5168c0788e20d88a161c906be99b878_Out_0 = Color_C343EF87;
            float _Property_d9b056fed78dd085b4d0698251bc8753_Out_0 = Vector1_2FA69C8F;
            float _SceneDepth_5041dfd2c8895382ae5d6fd487809adc_Out_1;
            Unity_SceneDepth_Eye_float(float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _SceneDepth_5041dfd2c8895382ae5d6fd487809adc_Out_1);
            float4 _ScreenPosition_8d8b51c518d1818981db05311793998f_Out_0 = IN.ScreenPosition;
            float _Split_8d0366c6012fb585a39dd52842ac3f19_R_1 = _ScreenPosition_8d8b51c518d1818981db05311793998f_Out_0[0];
            float _Split_8d0366c6012fb585a39dd52842ac3f19_G_2 = _ScreenPosition_8d8b51c518d1818981db05311793998f_Out_0[1];
            float _Split_8d0366c6012fb585a39dd52842ac3f19_B_3 = _ScreenPosition_8d8b51c518d1818981db05311793998f_Out_0[2];
            float _Split_8d0366c6012fb585a39dd52842ac3f19_A_4 = _ScreenPosition_8d8b51c518d1818981db05311793998f_Out_0[3];
            float _Subtract_c825aee699188186ad9c2b51c72d4b8d_Out_2;
            Unity_Subtract_float(_SceneDepth_5041dfd2c8895382ae5d6fd487809adc_Out_1, _Split_8d0366c6012fb585a39dd52842ac3f19_A_4, _Subtract_c825aee699188186ad9c2b51c72d4b8d_Out_2);
            float _Property_3db3e2d81cb1a188aca23ece62d7f199_Out_0 = Vector1_5327F7C2;
            float _Multiply_fa4b52c3a2fad089870ad7808d047486_Out_2;
            Unity_Multiply_float_float(_Property_3db3e2d81cb1a188aca23ece62d7f199_Out_0, 0.1, _Multiply_fa4b52c3a2fad089870ad7808d047486_Out_2);
            float _Power_b8dcd558ef12958284e491078c34dedd_Out_2;
            Unity_Power_float(_Subtract_c825aee699188186ad9c2b51c72d4b8d_Out_2, _Multiply_fa4b52c3a2fad089870ad7808d047486_Out_2, _Power_b8dcd558ef12958284e491078c34dedd_Out_2);
            float _Add_9a13fa6ccec69d8fabcbe0a82247f9f2_Out_2;
            Unity_Add_float(_Property_d9b056fed78dd085b4d0698251bc8753_Out_0, _Power_b8dcd558ef12958284e491078c34dedd_Out_2, _Add_9a13fa6ccec69d8fabcbe0a82247f9f2_Out_2);
            float _Saturate_919586c53b6e588587683723594ef3bd_Out_1;
            Unity_Saturate_float(_Add_9a13fa6ccec69d8fabcbe0a82247f9f2_Out_2, _Saturate_919586c53b6e588587683723594ef3bd_Out_1);
            float3 _Lerp_2117a5b13f63f48887d25214347e82c4_Out_3;
            Unity_Lerp_float3(_SceneColor_7f3fccc718c9208ea05c245d4f2f6794_Out_1, (_Property_c5168c0788e20d88a161c906be99b878_Out_0.xyz), (_Saturate_919586c53b6e588587683723594ef3bd_Out_1.xxx), _Lerp_2117a5b13f63f48887d25214347e82c4_Out_3);
            surface.BaseColor = _Lerp_2117a5b13f63f48887d25214347e82c4_Out_3;
            surface.Emission = float3(0, 0, 0);
            surface.Alpha = float(1);
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
            
        
            // must use interpolated tangent, bitangent and normal before they are normalized in the pixel shader.
            float3 unnormalizedNormalWS = input.normalWS;
            const float renormFactor = 1.0 / length(unnormalizedNormalWS);
        
            // use bitangent on the fly like in hdrp
            // IMPORTANT! If we ever support Flip on double sided materials ensure bitangent and tangent are NOT flipped.
            float crossSign = (input.tangentWS.w > 0.0 ? 1.0 : -1.0)* GetOddNegativeScale();
            float3 bitang = crossSign * cross(input.normalWS.xyz, input.tangentWS.xyz);
        
            output.WorldSpaceNormal = renormFactor * input.normalWS.xyz;      // we want a unit length Normal Vector node in shader graph
        
            // to preserve mikktspace compliance we use same scale renormFactor as was used on the normal.
            // This is explained in section 2.2 in "surface gradient based bump mapping framework"
            output.WorldSpaceTangent = renormFactor * input.tangentWS.xyz;
            output.WorldSpaceBiTangent = renormFactor * bitang;
        
            output.WorldSpacePosition = input.positionWS;
            output.ScreenPosition = ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
            output.uv0 = input.texCoord0;
            output.TimeParameters = _TimeParameters.xyz; // This is mainly for LW as HD overwrite this value
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        void BuildAppDataFull(Attributes attributes, VertexDescription vertexDescription, inout appdata_full result)
        {
            result.vertex     = float4(attributes.positionOS, 1);
            result.tangent    = attributes.tangentOS;
            result.normal     = attributes.normalOS;
            result.texcoord   = attributes.uv0;
            result.texcoord1  = attributes.uv1;
            result.texcoord2  = attributes.uv2;
            result.vertex     = float4(vertexDescription.Position, 1);
            result.normal     = vertexDescription.Normal;
            result.tangent    = float4(vertexDescription.Tangent, 0);
            #if UNITY_ANY_INSTANCING_ENABLED
            #endif
        }
        
        void VaryingsToSurfaceVertex(Varyings varyings, inout v2f_surf result)
        {
            result.pos = varyings.positionCS;
            result.worldPos = varyings.positionWS;
            result.worldNormal = varyings.normalWS;
            // World Tangent isn't an available input on v2f_surf
        
        
            #if UNITY_ANY_INSTANCING_ENABLED
            #endif
            #if !defined(LIGHTMAP_ON)
            #if UNITY_SHOULD_SAMPLE_SH
            #endif
            #endif
            #if defined(LIGHTMAP_ON)
            #endif
            #ifdef VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
                result.fogCoord = varyings.fogFactorAndVertexLight.x;
                COPY_TO_LIGHT_COORDS(result, varyings.fogFactorAndVertexLight.yzw);
            #endif
        
            DEFAULT_UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(varyings, result);
        }
        
        void SurfaceVertexToVaryings(v2f_surf surfVertex, inout Varyings result)
        {
            result.positionCS = surfVertex.pos;
            result.positionWS = surfVertex.worldPos;
            result.normalWS = surfVertex.worldNormal;
            // viewDirectionWS is never filled out in the legacy pass' function. Always use the value computed by SRP
            // World Tangent isn't an available input on v2f_surf
        
            #if UNITY_ANY_INSTANCING_ENABLED
            #endif
            #if !defined(LIGHTMAP_ON)
            #if UNITY_SHOULD_SAMPLE_SH
            #endif
            #endif
            #if defined(LIGHTMAP_ON)
            #endif
            #ifdef VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
                result.fogFactorAndVertexLight.x = surfVertex.fogCoord;
                COPY_FROM_LIGHT_COORDS(result.fogFactorAndVertexLight.yzw, surfVertex);
            #endif
        
            DEFAULT_UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(surfVertex, result);
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/LightingMetaPass.hlsl"
        
        ENDHLSL
        }
        Pass
        {
            Name "SceneSelectionPass"
            Tags
            {
                "LightMode" = "SceneSelectionPass"
            }
        
        // Render State
        Cull Off
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
sampler2D _GrabTexture;
        
        // Pragmas
        #pragma target 3.0
        #pragma multi_compile_instancing
        #pragma vertex vert
        #pragma fragment frag
        
        // DotsInstancingOptions: <None>
        // HybridV1InjectedBuiltinProperties: <None>
        
        // Keywords
        // PassKeywords: <None>
        // GraphKeywords: <None>
        
        // Defines
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SceneSelectionPass
        #define BUILTIN_TARGET_API 1
        #define SCENESELECTIONPASS 1
        #define _BUILTIN_SURFACE_TYPE_TRANSPARENT 1
        /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */
        #ifdef _BUILTIN_SURFACE_TYPE_TRANSPARENT
        #define _SURFACE_TYPE_TRANSPARENT _BUILTIN_SURFACE_TYPE_TRANSPARENT
        #endif
        #ifdef _BUILTIN_ALPHATEST_ON
        #define _ALPHATEST_ON _BUILTIN_ALPHATEST_ON
        #endif
        #ifdef _BUILTIN_AlphaClip
        #define _AlphaClip _BUILTIN_AlphaClip
        #endif
        #ifdef _BUILTIN_ALPHAPREMULTIPLY_ON
        #define _ALPHAPREMULTIPLY_ON _BUILTIN_ALPHAPREMULTIPLY_ON
        #endif
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/Shim/Shims.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/LegacySurfaceVertex.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/ShaderGraphFunctions.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float Vector1_E5125837;
        float Vector1_C7406269;
        float Vector1_76A233A4;
        float Vector1_5327F7C2;
        float Vector1_2FA69C8F;
        float4 Color_C343EF87;
        CBUFFER_END
        
        // Object and Global properties
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // Graph Functions
        // GraphFunctions: <None>
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float Alpha;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            surface.Alpha = float(1);
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
            
        
        
        
        
        
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        void BuildAppDataFull(Attributes attributes, VertexDescription vertexDescription, inout appdata_full result)
        {
            result.vertex     = float4(attributes.positionOS, 1);
            result.tangent    = attributes.tangentOS;
            result.normal     = attributes.normalOS;
            result.vertex     = float4(vertexDescription.Position, 1);
            result.normal     = vertexDescription.Normal;
            result.tangent    = float4(vertexDescription.Tangent, 0);
            #if UNITY_ANY_INSTANCING_ENABLED
            #endif
        }
        
        void VaryingsToSurfaceVertex(Varyings varyings, inout v2f_surf result)
        {
            result.pos = varyings.positionCS;
            // World Tangent isn't an available input on v2f_surf
        
        
            #if UNITY_ANY_INSTANCING_ENABLED
            #endif
            #if !defined(LIGHTMAP_ON)
            #if UNITY_SHOULD_SAMPLE_SH
            #endif
            #endif
            #if defined(LIGHTMAP_ON)
            #endif
            #ifdef VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
                result.fogCoord = varyings.fogFactorAndVertexLight.x;
                COPY_TO_LIGHT_COORDS(result, varyings.fogFactorAndVertexLight.yzw);
            #endif
        
            DEFAULT_UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(varyings, result);
        }
        
        void SurfaceVertexToVaryings(v2f_surf surfVertex, inout Varyings result)
        {
            result.positionCS = surfVertex.pos;
            // viewDirectionWS is never filled out in the legacy pass' function. Always use the value computed by SRP
            // World Tangent isn't an available input on v2f_surf
        
            #if UNITY_ANY_INSTANCING_ENABLED
            #endif
            #if !defined(LIGHTMAP_ON)
            #if UNITY_SHOULD_SAMPLE_SH
            #endif
            #endif
            #if defined(LIGHTMAP_ON)
            #endif
            #ifdef VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
                result.fogFactorAndVertexLight.x = surfVertex.fogCoord;
                COPY_FROM_LIGHT_COORDS(result.fogFactorAndVertexLight.yzw, surfVertex);
            #endif
        
            DEFAULT_UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(surfVertex, result);
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/DepthOnlyPass.hlsl"
        
        ENDHLSL
        }
        Pass
        {
            Name "ScenePickingPass"
            Tags
            {
                "LightMode" = "Picking"
            }
        
        // Render State
        Cull Back
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
sampler2D _GrabTexture;
        
        // Pragmas
        #pragma target 3.0
        #pragma multi_compile_instancing
        #pragma vertex vert
        #pragma fragment frag
        
        // DotsInstancingOptions: <None>
        // HybridV1InjectedBuiltinProperties: <None>
        
        // Keywords
        // PassKeywords: <None>
        // GraphKeywords: <None>
        
        // Defines
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS ScenePickingPass
        #define BUILTIN_TARGET_API 1
        #define SCENEPICKINGPASS 1
        #define _BUILTIN_SURFACE_TYPE_TRANSPARENT 1
        /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */
        #ifdef _BUILTIN_SURFACE_TYPE_TRANSPARENT
        #define _SURFACE_TYPE_TRANSPARENT _BUILTIN_SURFACE_TYPE_TRANSPARENT
        #endif
        #ifdef _BUILTIN_ALPHATEST_ON
        #define _ALPHATEST_ON _BUILTIN_ALPHATEST_ON
        #endif
        #ifdef _BUILTIN_AlphaClip
        #define _AlphaClip _BUILTIN_AlphaClip
        #endif
        #ifdef _BUILTIN_ALPHAPREMULTIPLY_ON
        #define _ALPHAPREMULTIPLY_ON _BUILTIN_ALPHAPREMULTIPLY_ON
        #endif
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/Shim/Shims.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/LegacySurfaceVertex.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/ShaderGraphFunctions.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float Vector1_E5125837;
        float Vector1_C7406269;
        float Vector1_76A233A4;
        float Vector1_5327F7C2;
        float Vector1_2FA69C8F;
        float4 Color_C343EF87;
        CBUFFER_END
        
        // Object and Global properties
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // Graph Functions
        // GraphFunctions: <None>
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float Alpha;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            surface.Alpha = float(1);
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
            
        
        
        
        
        
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        void BuildAppDataFull(Attributes attributes, VertexDescription vertexDescription, inout appdata_full result)
        {
            result.vertex     = float4(attributes.positionOS, 1);
            result.tangent    = attributes.tangentOS;
            result.normal     = attributes.normalOS;
            result.vertex     = float4(vertexDescription.Position, 1);
            result.normal     = vertexDescription.Normal;
            result.tangent    = float4(vertexDescription.Tangent, 0);
            #if UNITY_ANY_INSTANCING_ENABLED
            #endif
        }
        
        void VaryingsToSurfaceVertex(Varyings varyings, inout v2f_surf result)
        {
            result.pos = varyings.positionCS;
            // World Tangent isn't an available input on v2f_surf
        
        
            #if UNITY_ANY_INSTANCING_ENABLED
            #endif
            #if !defined(LIGHTMAP_ON)
            #if UNITY_SHOULD_SAMPLE_SH
            #endif
            #endif
            #if defined(LIGHTMAP_ON)
            #endif
            #ifdef VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
                result.fogCoord = varyings.fogFactorAndVertexLight.x;
                COPY_TO_LIGHT_COORDS(result, varyings.fogFactorAndVertexLight.yzw);
            #endif
        
            DEFAULT_UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(varyings, result);
        }
        
        void SurfaceVertexToVaryings(v2f_surf surfVertex, inout Varyings result)
        {
            result.positionCS = surfVertex.pos;
            // viewDirectionWS is never filled out in the legacy pass' function. Always use the value computed by SRP
            // World Tangent isn't an available input on v2f_surf
        
            #if UNITY_ANY_INSTANCING_ENABLED
            #endif
            #if !defined(LIGHTMAP_ON)
            #if UNITY_SHOULD_SAMPLE_SH
            #endif
            #endif
            #if defined(LIGHTMAP_ON)
            #endif
            #ifdef VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
                result.fogFactorAndVertexLight.x = surfVertex.fogCoord;
                COPY_FROM_LIGHT_COORDS(result.fogFactorAndVertexLight.yzw, surfVertex);
            #endif
        
            DEFAULT_UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(surfVertex, result);
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/DepthOnlyPass.hlsl"
        
        ENDHLSL
        }
    }
    CustomEditorForRenderPipeline "UnityEditor.Rendering.BuiltIn.ShaderGraph.BuiltInLitGUI" ""
    CustomEditor "UnityEditor.ShaderGraph.GenericShaderGraphMaterialGUI"
    FallBack "Hidden/Shader Graph/FallbackError"
}