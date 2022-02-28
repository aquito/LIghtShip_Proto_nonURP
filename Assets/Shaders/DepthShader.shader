Shader "Custom/DepthShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _DepthTex("_DepthTex", 2D) = "red" {}
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
                //storage for our transformed depth uv

                float3 depth_uv : TEXCOORD1;
            };
            
            // Transforms used to sample the context awareness textures

            float4x4 _depthTransform;
 
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                
                //multiply the uv's by the depth transform to roate them correctly.
                o.depth_uv = mul(_depthTransform, float4(v.uv, 1.0f, 1.0f)).xyz;
                return o;
            }
 
            //our texture samplers
            sampler2D _DepthTex;
            
            fixed4 frag (v2f i) : SV_Target
            {                
                //our depth texture, we need to normalise the uv coords before using.
                float2 depthUV = float2(i.depth_uv.x / i.depth_uv.z, i.depth_uv.y / i.depth_uv.z);
                //read the depth texture pixel

                float depthCol = tex2D(_DepthTex, depthUV).r;
 
                return depthCol;
            }
            ENDCG
        }
    }
}
