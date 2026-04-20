Shader "Unlit/S_Wireframe"
{
    Properties
    {
        _TopLeftAnchor ("Top Left Anchor", Vector) = (-10, 10, 0)
        _BottomRightAnchor ("Bottom Right Anchor", Vector) = (10, -10, 0)
        _WireColor ("Wire Color", Color) = (0, 1, 0, 1)
        _WireThickness ("Wire Thickness", Range(0, 0.1)) = 0.02
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Name "WIREFRAME"
            Tags { "RenderType"="Opaque" }
            
            Offset 1, 1
            ZWrite Off
            ZTest Always
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag
            #pragma target 4.0
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
            };

            struct v2g {
                float4 vertex : TEXCOORD0;
            };

            struct g2f {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD1;
                noperspective float3 bary : TEXCOORD2;
            };

            float3 _TopLeftAnchor;
            float3 _BottomRightAnchor;
            fixed4 _WireColor;
            float _WireThickness;

            v2g vert (appdata v) {
                v2g o;
                o.vertex = v.vertex;
                return o;
            }

            [maxvertexcount(3)]
            void geom(triangle v2g IN[3], inout TriangleStream<g2f> triStream) {
                float4 p0 = UnityObjectToClipPos(IN[0].vertex);
                float4 p1 = UnityObjectToClipPos(IN[1].vertex);
                float4 p2 = UnityObjectToClipPos(IN[2].vertex);

                float2 w0 = _ScreenParams.xy * (p0.xy / p0.w);
                float2 w1 = _ScreenParams.xy * (p1.xy / p1.w);
                float2 w2 = _ScreenParams.xy * (p2.xy / p2.w);

                float edge0 = length(w2 - w1);
                float edge1 = length(w2 - w0);
                float edge2 = length(w1 - w0);

                float area = abs((w1.x - w0.x) * (w2.y - w0.y) - (w1.y - w0.y) * (w2.x - w0.x));

                float3 dists[3];
                dists[0] = float3(area / edge0, 0, 0);
                dists[1] = float3(0, area / edge1, 0);
                dists[2] = float3(0, 0, area / edge2);
                
                for (int i = 0; i < 3; i++) {
                    g2f o;
                    o.pos = (i == 0) ? p0 : (i == 1 ? p1 : p2);
                    o.worldPos = mul(unity_ObjectToWorld, IN[i].vertex).xyz;
                    o.bary = dists[i];
                    triStream.Append(o);
                }
            }

            fixed4 frag (g2f i) : SV_Target {
                if (i.worldPos.x < _TopLeftAnchor.x || i.worldPos.y > _TopLeftAnchor.y ||
                    i.worldPos.x > _BottomRightAnchor.x || i.worldPos.y < _BottomRightAnchor.y) {
                    discard;
                }

                float minBary = min(min(i.bary.x, i.bary.y), i.bary.z);
                if (minBary > _WireThickness * 100.0) discard;

                return _WireColor;
            }
            ENDCG
        }
    }
}
