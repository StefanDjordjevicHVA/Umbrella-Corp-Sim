// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/TerrainScan"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _DetailTex("Texture", 2D) = "white" {}
        _ScanDistance("Scan Distance", float) = 0
        _ScanWidth("Scan Width", float) = 10
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
                float4 ray : TEXCOORD1; // 4 corners of the farclip plane
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 uv_depth : TEXCOORD1;
                float4 interpolatedRay : TEXCOORD2;
            };

            float4 _MainTex_TexelSize;
			float4 _CameraWS;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv.xy;
                o.uv_depth = v.uv.xy;

                #if UNITY_UV_STARTS_AT_TOP
				if (_MainTex_TexelSize.y < 0)
					o.uv.y = 1 - o.uv.y;
				#endif
                
                o.interpolatedRay = v.ray;
                
                return o;
            }

            sampler2D _MainTex;
			sampler2D _DetailTex;
			sampler2D_float _CameraDepthTexture;
			float4 _WorldSpaceScannerPos;
			float _ScanDistance;
            float _ScanWidth;

            half4 frag (v2f i) : SV_Target
            {
                half4 col = tex2D(_MainTex, i.uv);
                
                float rawDepth = DecodeFloatRG(tex2D(_CameraDepthTexture, i.uv_depth));
                float linearDepth = Linear01Depth(rawDepth); // Waarde tussen 0 en 1 voor de diepte
                float4 wsDir = linearDepth * i.interpolatedRay;
                float3 wsPos = _WorldSpaceCameraPos + wsDir;

                float dist = distance(wsPos, _WorldSpaceCameraPos);
                
                if(dist < _ScanDistance && dist > _ScanDistance - _ScanWidth)
                {
                    return 1;
                }
         
                return col;
                
                    
            }
            ENDCG
        }
    }
}
