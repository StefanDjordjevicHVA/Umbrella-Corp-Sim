Shader "Custom/TerrainShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _WallTex ("WallTexture", 2D) = "white" {}
        _TexScale ("Texture Scale", Float) = 1
    }

    SubShader
    {
        Tags {"RenderType"="Opaque"}
        LOD 200
        
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _WallTex;
        float _TexScale;

        struct Input
        {
            float3 worldPos;
            float3 worldNormal;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // simpele scaler zodat je vanuit de shader/material de texture kan scalen
            float3 scaledWorldPos = IN.worldPos / _TexScale;

            float3 pWeight = abs(IN.worldNormal);

            // Blend value
            pWeight /= pWeight.x + pWeight.y +pWeight.z;

            // 3 float3 P staat voor projection.
            float3 xP = tex2D(_WallTex, scaledWorldPos.yz) * pWeight.x; // projectie van y -> z
            float3 yP = tex2D(_MainTex, scaledWorldPos.xz) * pWeight.y; // projectie van x -> z
            float3 zP = tex2D(_WallTex, scaledWorldPos.xy) * pWeight.z; // projectie van x -> y

            o.Albedo = xP + yP + zP;
        }

        ENDCG
    }
    Fallback "Diffuse"

}
