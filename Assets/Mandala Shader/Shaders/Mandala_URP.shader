//https://catlikecoding.com/unity/tutorials/custom-srp/draw-calls/

Shader "MandalaShaders/Mandala_URP"
{
    Properties
    {
        [Enum(On, 1, Off, 0)] _Lighting ("Lighting", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Src Blend", Float) = 1
       [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend", Float) = 0
       [Enum(Off, 0, On, 1)] _ZWrite ("Z Write", Float) = 1
       _BaseColor("Color", Color) = (0.5, 0.5, 0.5, 1)
       _AlphaTex("Alpha Texture", 2D) = "white" {}
       _Alpha("Opacity", Range(1, 0)) = 1
       [Toggle(USEMAGICALPHA)]
		_UseMagicAlpha("Use generated Alpha Map", Float) = 0

       _BaseTexture("Base Texture", 2D) = "white" {}

       _Metallic("Metallic", Range(0, 1)) = 0
       _Smoothness("Smoothness", Range(0, 1)) = 0
        _ReceiveShadows("Receive Shadows", Float) = 1.0

        _MandalaLayers("Mandala Layer Count", Int) = 0
		_MandalaRotationShift("Mandala Rotation Shift", Float) = 0
		_MandalaScaleShift("Mandala Scale Shift", Float) = 0
        _Desaturate("Desaturate", Range(0, 1)) = 0

        _Darken("Darken", Range(0, 1)) = 0
        _Gamma("Gamma", Range(0, 1)) = 1
        _Lighten("Lighten", Range(0, 1)) = 1

    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" }

        Pass 
        {
            Tags { "LightMode" = "UniversalForward" }

            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_Zwrite]

            HLSLPROGRAM


            #pragma vertex Vert
            #pragma fragment Frag
            #pragma target 3.0
            #pragma shader_feature USEMAGICALPHA
            #pragma shader_feature _RECEIVE_SHADOWS_OFF

            #pragma shader_feature _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature _ENVIRONMENTREFLECTIONS_OFF

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            //#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE



            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            
            
            struct VertexInput
            {
                float4 vertPos : POSITION;
                float2 texcoord : TEXCOORD0;
                float3 normalOS : NORMAL;
                float2 lightmapUV : TEXCOORD1;
            };


            struct VertexOutput 
            {
                float4 pos : SV_POSITION;
                float3 normalWS : NORMAL;
                float2 uv : TEXCOORD0;
                DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 1);
                float3 viewDirWS : TEXCOORD2;
                float3 positionWS : TEXCOORD3;
                half4 fogFactorAndVertexLight : TEXCOORD6;
                float4 shadowCoord : TEXCOORD7;
            };

            float4 _BaseColor;
            float4 _AlphaTex_ST;
            float _Alpha;
            float4 _BaseTexture_ST;
            half _Metallic;
            half _Smoothness;
            half _Lighting;
            half _MandalaLayers;
		    half _MandalaRotationShift;
		    half _MandalaScaleShift;
            half _Desaturate;
            half _Darken;
            half _Gamma;
            half _Lighten;


            TEXTURE2D(_BaseTexture);
            SAMPLER(sampler_BaseTexture);

            TEXTURE2D(_AlphaTex);
            SAMPLER(sampler_AlphaTex);

            InputData InitializeInputData(VertexOutput input)
            {
                InputData data;
                data.positionWS = input.pos.xyz;

                half viewDirWS = input.viewDirWS;
                data.normalWS = input.normalWS;

                data.normalWS = NormalizeNormalPerPixel(data.normalWS);
                viewDirWS = SafeNormalize(viewDirWS);
                data.viewDirectionWS = viewDirWS;

                #if defined(_MAIN_LIGHT_SHADOWS) && !defined(_RECEIVE_SHADOWS_OFF)
                    data.shadowCoord = input.shadowCoord;
                #else
                    data.shadowCoord = float4(0, 0, 0, 0);
                #endif

                //FILLER
       
                data.fogCoord = input.fogFactorAndVertexLight.x;
                data.vertexLighting = input.fogFactorAndVertexLight.yzw;
                data.bakedGI = SAMPLE_GI(input.lightmapUV, input.vertexSH, data.normalWS);
                return data;
            }

            float2x2 Rotation(float angle) 
            {
                float c = cos(angle);
                float s = sin(angle);
                return float2x2(c, -s, s, c);
		    }

		float2 ModifyUV(float2 InputUVs, float InputAngle, float4 ST) 
        {
			float2 pivot = float2(0.5 * ST.x - ST.z, 0.5 * ST.y - ST.w);
			float2 UVs = (InputUVs * ST.xy) - pivot;
			float2 rotPos = mul(UVs, Rotation(InputAngle));
			rotPos += pivot;
			rotPos += ST.zw;
			return rotPos;
		}

		float3 Desaturate(float3 tex) 
        {
			float lum = Luminance(tex.rgb);
			return lerp(tex.rgb, lum.xxx, _Desaturate);
		}

		// //https://developer.nvidia.com/gpugems/GPUGems/gpugems_ch22.html
		float AdjustLevelForChannel(float pixel) {								   
			return saturate(pow(saturate((pixel -_Darken) / (_Lighten - _Darken)), _Gamma));
		}

		float3 AdjustLevels(float3 col) {
			return float3 (AdjustLevelForChannel(col.r), AdjustLevelForChannel(col.g), AdjustLevelForChannel(col.b));
		}






            VertexOutput Vert(VertexInput input)
            {
                VertexOutput output;
                input.vertPos.w = 1.0;
                float4 actualPos = mul(unity_ObjectToWorld, input.vertPos);
    
                output.pos = mul(unity_MatrixVP, actualPos);

                output.normalWS = TransformObjectToWorldDir(input.normalOS);
                output.uv = TRANSFORM_TEX(input.texcoord, _BaseTexture);
                output.viewDirWS = GetCameraPositionWS() - actualPos;

                #ifdef _ADDITIONAL_LIGHTS
                    output.positionWS = actualPos;
                #endif


                #if defined(_MAIN_LIGHT_SHADOWS) && !defined(_RECEIVE_SHADOWS_OFF)
                output.shadowCoord = TransformWorldToShadowCoord(actualPos);
                #endif

                half fogFactor = ComputeFogFactor(actualPos.z);

                OUTPUT_LIGHTMAP_UV(input.lightmapUV, unity_LightmapST, output.lightmapUV);
                OUTPUT_SH(output.normalWS.xyz, output.vertexSH);

                output.fogFactorAndVertexLight = half4(fogFactor, 1, 1, 1);

                return output;
            }

            float4 Frag(VertexOutput input) : SV_Target 
            {
                //float4 col = float4(SAMPLE_TEXTURE2D(_BaseTexture, sampler_BaseTexture, input.uv).xyz * _BaseColor.rgb, _Alpha);
                float4 _ScaleTranslate = float4(_BaseTexture_ST.x + (_MandalaScaleShift / 100), _BaseTexture_ST.y + (_MandalaScaleShift / 100), _BaseTexture_ST.z, _BaseTexture_ST.w);
                



                float4 col = float4(SAMPLE_TEXTURE2D(_BaseTexture, sampler_BaseTexture, ModifyUV(input.uv, 0, _ScaleTranslate)).xyz, 1);

                half4 Output;

                 half calculatedAngle = 0;
                float4 newLayer;
                for(uint i = 0; i < (int)_MandalaLayers; i++){
                    calculatedAngle = (360 / _MandalaLayers + (_MandalaRotationShift / 100)) * (i + 1);
                    newLayer = SAMPLE_TEXTURE2D(_BaseTexture, sampler_BaseTexture, ModifyUV(input.uv, calculatedAngle, _ScaleTranslate)); 
                    col = max(col, newLayer);
                }

                float Alpha;
                #ifdef USEMAGICALPHA
                Alpha = Luminance(col.rgb);
                #else
                Alpha = SAMPLE_TEXTURE2D(_AlphaTex, sampler_AlphaTex, input.uv).a * _Alpha;
                #endif

                Output.rgb = AdjustLevels(col.rgb);
                Output.rgb = Desaturate(Output.rgb)* _BaseColor;

               if(_Lighting == 1)
               {
                    InputData data = InitializeInputData(input);

                              //InputData, Albedo, metallic, specular, smoothness, occlusion, emission, alpha
                   Output = UniversalFragmentPBR(data, Output.rgb, _Metallic, 0, _Smoothness, 0, 0, Alpha);
                   //Output.rgb = MixFog(Output.rgb, data.fogCoord);
               }      else {
                   Output = float4(Output.rgb, Alpha);
               }   

    

                return Output;
            }


            ENDHLSL

        }

         Pass
        {
            Name "ShadowCaster"
            Tags{"LightMode" = "ShadowCaster"}

            ZWrite On
            ZTest LEqual
            Cull[_Cull]

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature _ALPHATEST_ON

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma shader_feature _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }
    }
}
