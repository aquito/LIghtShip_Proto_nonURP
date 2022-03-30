Shader "Custom/SemanticShaderVariation01"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _SemanticTex("_SemanticTex", 2D) = "red" {}

        // let's add a property to rotate the UV
        _Rotation ("Rotation", Range(0, 360)) = 0
        
    }
    SubShader
    {
        
        

        Pass
        {

            // No culling or depth 
            Cull Off ZWrite Off ZTest Always

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
                float4 color: COLOR;
                //storage for our transformed depth uv 
                float3 semantic_uv : TEXCOORD1;
                float3 normal: NORMAL;
               

            };
            
            // Transforms used to sample the context awareness textures
            float4x4 _semanticTransform;


            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;

                //multiply the uv's by the depth transform to roate them correctly. 
                o.semantic_uv = mul(_semanticTransform, float4(v.uv, 1.0f, 1.0f)).xyz; 
                

                return o;
            }
 
            //our texture samplers
            
            sampler2D _MainTex;
            sampler2D _SemanticTex;
            float4 _MainTex_ST;
            float _Rotation;
                   
             void Unity_Rotate_Degrees_float
            (
                float2 UV,
                float2 Center,
                float Rotation,
                out float2 Out
            )

            {
 
                Rotation = Rotation * (UNITY_PI/180.0f);
                UV -= Center;
                float s = sin(Rotation);
                float c = cos(Rotation);
                float2x2 rMatrix = float2x2(c, -s, s, c);
                rMatrix *= 0.5;
                rMatrix += 0.5;
                rMatrix = rMatrix * 2 - 1;
                UV.xy = mul(UV.yx, rMatrix);
                UV += Center;
                Out = UV;
            }



            fixed4 frag (v2f i) : SV_Target
            {                

                // let's calculate the absolute value of U
                float u = abs(i.uv.x - 0.5);

                // let's calculate the absolute value of V

                float v = abs(i.uv.y - 0.5);


                float rotation = _Rotation;
                // we center the rotation pivot
                float center = 0.5;

                // let's generate new UV coordinates for the texture
                float2 uv = 0;

                Unity_Rotate_Degrees_float(float2(u,v), center, rotation, uv);

                fixed4 mainCol = tex2D(_MainTex, uv);
                UNITY_APPLY_FOG(i.fogCoord, mainCol);

                //i.uv *=2;
                //float2 fuv = frac(i.uv);
                //unity scene
                //float4 mainCol = tex2D(_MainTex, i.uv);
                //our semantic texture, we need to normalise the uv coords before using.

                float2 semanticUV = float2(i.semantic_uv.x / i.semantic_uv.z, i.semantic_uv.y / i.semantic_uv.z);
                //read the semantic texture pixel

                float4 semanticCol = tex2D(_SemanticTex, semanticUV);


                
    
                //add some grid lines to the sky
                //semanticCol.g *= sin(i.uv.x* 100.0);
                //semanticCol.b *= cos(i.uv.y* 100.0);

                // changing color
                //semanticCol.r = i.color;
                //semanticCol.g = i.color;

      
                
                //set alpha to blend rather than overight

               // semanticCol.a *= 0.5f; 
 
                //mix the main color and the semantic layer

                //return lerp(mainCol,semanticCol, semanticCol.a);

                return mainCol;

            }
            ENDCG
        }

        
        /*
            CGPROGRAM
            #pragma surface surf Lambert vertex:vert

            struct Input {
                float2 uv_MainTex;
                float3 vertColor;
            };

            float4 _Tint;
            float _Freq;
            float _Speed;
            float _Amp;

            struct appdata{
                float4 vertex: POSITION;
                float3 normal: NORMAL;
                float4 texcoord: TEXCOORD0;
                float4 texcoord1: TEXCOORD1;
                float4 texcoord2: TEXCOORD2;
            
            };

            void vert(inout appdata v, out Input o)
            {
                UNITY_INITIALIZE_OUTPUT(Input, o);
                float t = _Time * _Speed;
                float waveHeight = sin(t + v.vertex.x * _Freq) * _Amp;
                v.vertex.y = v.vertex.y + waveHeight;
                v.normal = normalize(float3(v.normal.x + waveHeight, v.normal.y, v.normal.z));
                o.vertColor = waveHeight + 2;    
            }


            sampler2D _MainTex;
            void surf(Input IN, inout SurfaceOutput o)
            {
                float4 c = tex2D(_MainTex, IN.uv_MainTex);
                o.Albedo = c * IN.vertColor.rgb; 
            }
        
            ENDCG
*/
    }
}
