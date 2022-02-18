Shader "Unlit/USB_simple_color"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _SemanticTex("_SemanticTex", 2D) = "red" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

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
               
                float3 semantic_uv : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _SemanticTex;

// Transforms used to sample the context awareness textures
            float4x4 _semanticTransform;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);

                //multiply the uv's by the depth transform to roate them correctly.
                o.semantic_uv = mul(_semanticTransform, float4(v.uv, 1.0f, 1.0f)).xyz;
                return o;
                
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);

                
                //our semantic texture, we need to normalise the uv coords before using.
                float2 semanticUV = float2(i.semantic_uv.x / i.semantic_uv.z, i.semantic_uv.y / i.semantic_uv.z);
                //read the semantic texture pixel
                float4 semanticCol = tex2D(_SemanticTex, semanticUV);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
 
                return col+semanticCol;
                

                //return col;
            }
            ENDCG
        }
    }
}
