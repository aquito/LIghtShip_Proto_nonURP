Shader "MandalaShaders/SemanticMandala"
{
    Properties
    {
        _MandalaTex ("Texture", 2D) = "white" {}
        [HideInInspector]_MandalaTexST("Main Mandala Source Texture Scale (XY) Translate (ZW)", Vector) = (1, 1, 0, 0)

        _SemanticTex("_SemanticTex", 2D) = "red" {}

		[Toggle(USEMAGICALPHA)]
		_UseMagicAlpha("Use generated Alpha Map", Float) = 0

		_AlphaTex("Alpha Map", 2D) = "white" {}

		_ColorMultiplier("Color Multiplier", Color) = (1, 1, 1, 1)
		_MandalaLayers("Mandala Layer Count", Int) = 0
		_MandalaRotationShift("Mandala Rotation Shift", Float) = 0
		_MandalaScaleShift("Mandala Scale Shift", Float) = 0
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0

		_Darken("Darken", Range(0, 1)) = 0
		_Gamma("Gamma", Range(0, 1)) = 1
		_Lighten("Lighten", Range(0, 1)) = 1

		_Desaturate("Desaturate", Range(0, 1)) = 0

        
    }
    SubShader
    {

		//FIRST PASS - SURFACE SHADER (MANDALA)

		Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 200

        CGPROGRAM 
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows alpha:fade
		#pragma shader_feature USEMAGICALPHA

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

		sampler2D _MandalaTex;		
		fixed4 _MandalaTexST;
		sampler2D _AlphaTex;

		half _Desaturate;
		half _Contrast;

		half _Darken;
		half _Gamma;
		half _Lighten;

        half _Glossiness;
        half _Metallic;

		half _MandalaRotationShift;
		half _MandalaScaleShift;
		uint _MandalaLayers;

		half _UseMagicAlpha;

		fixed4 _ColorMultiplier;

		///--------Additional Methods-------------///
		float2x2 Rotation(float angle) {
			float c = cos(angle);
			float s = sin(angle);
			return float2x2(c, -s, s, c);
		}

		float2 ModifyUV(float2 InputUVs, float InputAngle, float4 ST) {
			float2 pivot = float2(0.5 * ST.x - ST.z, 0.5 * ST.y - ST.w);
			float2 UVs = (InputUVs * ST.xy) - pivot;
			float2 rotPos = mul(UVs, Rotation(InputAngle));
			rotPos += pivot;
			rotPos += ST.zw;
			return rotPos;
		}

		float3 Desaturate(float3 tex) {
			float lum = Luminance(tex.rgb);
			return lerp(tex.rgb, lum.xxx, _Desaturate);
		}

		//https://developer.nvidia.com/gpugems/GPUGems/gpugems_ch22.html
		float AdjustLevelForChannel(float pixel) {								   
			return (pow(((pixel)-_Darken) / (_Lighten - _Darken), _Gamma));
		}

		float3 AdjustLevels(float3 col) {
			return float3 (AdjustLevelForChannel(col.r), AdjustLevelForChannel(col.g), AdjustLevelForChannel(col.b));
		}
		///------------------------------------///

        struct Input
        {
            float2 uv_MandalaTex;
			float2 uv_AlphaTex;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
			//Calculated Scale Translate
			float4 _ScaleTranslate = fixed4(_MandalaTexST.x + (_MandalaScaleShift / 100), _MandalaTexST.y + (_MandalaScaleShift / 100), _MandalaTexST.z, _MandalaTexST.w);

			//Basic Texture with custom ST
			fixed4 mandala = tex2D(_MandalaTex, ModifyUV(IN.uv_MandalaTex, 0, _ScaleTranslate));

			half calculatedAngle = 0;
			fixed4 newLayer;

			//Mandala Creation
			 for (uint i = 0; i < _MandalaLayers; i++) {
				
			 	calculatedAngle = (360 / _MandalaLayers + (_MandalaRotationShift / 100)) * (i + 1);
			 	newLayer = tex2D(_MandalaTex, ModifyUV(IN.uv_MandalaTex, calculatedAngle, _ScaleTranslate));
			 	mandala = max(mandala, newLayer);
			 }

				mandala.rgb = AdjustLevels(mandala.rgb);

			 if (_Desaturate > 0)
			 	mandala.rgb = Desaturate(mandala.rgb);


             o.Albedo = (mandala.rgb * _ColorMultiplier);

			 float alpha = 0;
		 #ifdef USEMAGICALPHA
		 	alpha = Luminance(mandala.rgb);
		 #else
		 	alpha = tex2D(_AlphaTex, IN.uv_AlphaTex).a;
		 #endif

		 	o.Alpha = alpha;

            // Metallic and smoothness come from slider variables

            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
        }
        ENDCG

    
        // VERT FRAG - NEEDS A PASS

        Cull Off ZWrite Off ZTest Always

		Pass
        {
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
           
 
            #include "UnityCG.cginc"
 
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
 
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                //storage for our transformed depth uv
                float3 semantic_uv : TEXCOORD1;
            };
            
            // Transforms used to sample the context awareness textures
            float4x4 _semanticTransform;
 
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                
                //multiply the uv's by the depth transform to rotate them correctly.
                o.semantic_uv = mul(_semanticTransform, float4(v.uv, 1.0f, 1.0f)).xyz;
                return o;
            }
 
            //our texture samplers
            sampler2D _MainTex;
            sampler2D _SemanticTex;
 
            
            fixed4 frag (v2f i) : SV_Target
            {                
                //unity scene
                float4 mainCol = tex2D(_MainTex, i.uv);
                //our semantic texture, we need to normalise the uv coords before using.
                float2 semanticUV = float2(i.semantic_uv.x / i.semantic_uv.z, i.semantic_uv.y / i.semantic_uv.z);
                //read the semantic texture pixel
                float4 semanticCol = tex2D(_SemanticTex, semanticUV);
 
                //add some grid lines to the sky
                //semanticCol.g *= sin(i.uv.x* 100.0);
                //semanticCol.b *= cos(i.uv.y* 100.0);

                //set alpha to blend rather than overight

                //semanticCol.a *= 0.2f; 
 
                //mix the main color and the semantic layer

                return lerp(mainCol,semanticCol, semanticCol.a);

               
            }

            ENDCG

	    }    
    }   
}
