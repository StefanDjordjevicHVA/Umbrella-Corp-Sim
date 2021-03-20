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
        _EdgeStrength("The strength of the edge of scan", float) = 10
        _LeadColor("Leading Edge Color", Color) = (1,1,1,0)
        _MiddleColor("Middel part scan Color", Color) = (1,1,1,0)
        _TrailColor("Fading Trail part Color", Color) = (1,1,1,0)
        _HorizontalBarColor("Color for the horizontal scan bars", Color) = (1,1,1,0)
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

            float _EdgeStrength;
            float4 _LeadColor;
            float4 _MiddleColor;
            float4 _TrailColor;
            float4 _HorizontalBarColor;

            float4 horizontalScanBars(float2 p)
            {
                // saturate functie clamps waarde tussen 0 en 1
                // round functie rond af naar heel getal ( 0 of 1 )
                // abs functie maakt het positief
                // frac funtie returnt alles achter de komma
                // In deze functie stop je de uv posities maar gebruikt alleen de Y values.
                // Hierdoor krijg je horizontale balkjes (lijnen) die ingevult worden met een kleur

                return 1 - saturate(round(abs(frac(p.y * 200) * 2)));

                // Afrondings fouten. 0.5
                
            }
            
            half4 frag (v2f i) : SV_Target
            {
                half4 col = tex2D(_MainTex, i.uv);
                
                float rawDepth = DecodeFloatRG(tex2D(_CameraDepthTexture, i.uv_depth));
                float linearDepth = Linear01Depth(rawDepth); // Waarde tussen 0 en 1 voor de diepte
                float4 wsDir = linearDepth * i.interpolatedRay;
                float3 wsPos = _WorldSpaceCameraPos + wsDir;

                float dist = distance(wsPos, _WorldSpaceCameraPos);

                // scanCol moet een float4 (half4) zijn om alle rgb waardes te kunnen krijgen.
                half4 scanCol = half4(0, 0, 0, 0);
                
                if(dist < _ScanDistance && dist > _ScanDistance - _ScanWidth && linearDepth < 1)
                {
                    // Doormiddel van deze berekening krijgt de scan in de breedte een waarde die van verstepunt naar het punt die het dichts bij is een waarde 1 tot 0
                    float fade = 1 - (_ScanDistance - dist) / (_ScanWidth);

                    // Eerst Lerp ik de kleur waardes tussen de lead kleur en de middel kleur.
                    // Door de lerp waarde (fade kracht) te machten met de edge strength krijg je een dikkere scan lead.
                    half4 edge = lerp(_MiddleColor, _LeadColor, pow(fade, _EdgeStrength));
                    // Daarna lerp ik de edge float (eerder gelerpte waarden) met de trail kleur.
                    // als laatste voeg je de horizontale fragment shader functie toe en die tel je op bij de scanCol maal de gewenste kleur.
                    scanCol = lerp(_TrailColor, edge, fade) + (horizontalScanBars(i.uv) * _HorizontalBarColor);

                    scanCol *= fade;
                }


                // Door de "scanCol" op te tellen aan de kleur van de originele "col" fade de scan naar de kleur van de originele Render ipv naar zwart (geen kleur "0"). 
                return col + scanCol;
                
                    
            }
            ENDCG
        }
    }
}
