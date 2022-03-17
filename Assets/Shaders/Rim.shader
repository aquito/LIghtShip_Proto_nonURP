Shader "Custom/Rim"
{
    Properties
    {
        _RimColor ("Color", Color) = (0,0.5,0.5,0)
        _RimPower ("Rim Power", Range(0.5, 8.0)) = 3.0 
       
    }
    SubShader
    {
        

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Lambert

        
        struct Input
        {
            float3 viewDir;
            float3 worldPos;
        };

        
        float4 _RimColor;
        float _RimPower;


        void surf (Input IN, inout SurfaceOutput o)
        {
            half rim = 1 - saturate(dot(normalize(IN.viewDir), o.Normal));
            o.Emission = rim > 0.5 ? float3(1,0,0): rim > 0.3 ? float3(0,1,0): 0;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
