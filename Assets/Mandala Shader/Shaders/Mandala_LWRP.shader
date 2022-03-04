Shader "MandalaShaders/Mandala_LWRP"
{
	Properties
	{
		
		_MandalaTex("Mandala Texture", 2D) = "white" {}

		[Toggle(USEMAGICALPHA)]
		_UseMagicAlpha("Use generated Alpha Map", Float) = 0

		_AlphaTex("Alpha Map", 2D) = "white" {}
		_Color("Color Multiplier", Color) = (1,1,1,1)
		_MandalaLayers("Mandala Layer Count", Int) = 0
		_MandalaRotationShift("Mandala Rotation Shift", Float) = 0
		_MandalaScaleShift("Mandala Scale Shift", Float) = 0

		_Darken("Darken", Range(0, 1)) = 0
		_Gamma("Gamma", Range(0, 1)) = 1
		_Lighten("Lighten", Range(0, 1)) = 1

		_Desaturate("Desaturate", Range(0, 1)) = 0

	}
	SubShader
	{
		//Tags { "RenderType" = "Opaque" }
			 Tags { "Queue" = "Transparent" "RenderPipeline" = "LightweightPipeline" "RenderType" = "Transparent" }
		LOD 200
		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			#pragma shader_feature USEMAGICALPHA

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
					float4 vertex : SV_POSITION;
			};

			sampler2D _MandalaTex;
			sampler2D _AlphaTex;
			float4 _MainTex_ST;
			float4 _Color;
			fixed4 _MainTexST;
			half _MandalaRotationShift;
			half _MandalaScaleShift;
			uint _MandalaLayers;

			half _Desaturate;
			half _Contrast;

			half _Darken;
			half _Gamma;
			half _Lighten;

			half _UseMagicAlpha;

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


			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o, o.vertex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				//Calculated Scale Translate
				float4 _ScaleTranslate = fixed4(_MainTexST.x + (_MandalaScaleShift / 100), _MainTexST.y + (_MandalaScaleShift / 100), _MainTexST.z, _MainTexST.w);

				//Basic Texture with custom ST
				fixed4 mandala = tex2D(_MandalaTex, ModifyUV(i.uv, 0, _ScaleTranslate));

				half calculatedAngle = 0;
				fixed4 newLayer;

				float2 MainUVs = i.uv;

				//Mandala Creation
				for (uint i = 0; i < _MandalaLayers; i++) 
				{
					calculatedAngle = (360 / _MandalaLayers + (_MandalaRotationShift / 100)) * (i + 1);
					newLayer = tex2D(_MandalaTex, ModifyUV(MainUVs, calculatedAngle, _ScaleTranslate));
					mandala = max(mandala, newLayer);
				}

				mandala.rgb = AdjustLevels(mandala.rgb);

				if (_Desaturate > 0)
					mandala.rgb = Desaturate(mandala.rgb);

				// mandala.rgb;
				fixed4 col = mandala.rgba;

				float alpha = 0;
				#ifdef USEMAGICALPHA
					alpha = Luminance(mandala.rgb);
				#else
					alpha = tex2D(_AlphaTex, MainUVs).a;
				#endif



				col.a = alpha;

				// sample the texture
				//fixed4 col = tex2D(_MainTex, i.uv) * _Color;
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
		// shadow caster rendering pass, implemented manually
        // using macros from UnityCG.cginc
        Pass
        {
            Tags {"LightMode"="ShadowCaster"}

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_shadowcaster
            #include "UnityCG.cginc"

            struct v2f { 
                V2F_SHADOW_CASTER;
            };

            v2f vert(appdata_base v)
            {
                v2f o;
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }
}
