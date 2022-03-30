Shader "Custom/SemanticShaderVariation05"
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
        // No culling or depth
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
 
                Rotation = Rotation * (UNITY_PI/180.0f); // 180 in the original
                UV -= Center;
                float s = sin(Rotation);
                float c = cos(Rotation);
                float2x2 rMatrix = float2x2(c, -s, s, c);
                rMatrix *= 0.2;
                rMatrix += 0.2;
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


                float2 semanticUV = float2(i.semantic_uv.x / i.semantic_uv.z, i.semantic_uv.y / i.semantic_uv.z);
                //read the semantic texture pixel

                float4 semanticCol = tex2D(_SemanticTex, semanticUV);

                return mainCol;

            }
            ENDCG
        }
    }
}
