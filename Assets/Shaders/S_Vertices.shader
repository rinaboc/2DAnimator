Shader "Unlit/S_Vertices"
{
    Properties
    {
        _vertexSize ("Vertex Size", Range(0, 0.1)) = 0.02
        _TopLeftAnchor ("Top Left Anchor", Vector) = (-10, 10, 0)
        _BottomRightAnchor ("Bottom Right Anchor", Vector) = (10, -10, 0)
    }
    SubShader
    {
        Pass
        {
            Name "POINTS"
            Tags { "RenderType"="Opaque" }
            LOD 100

            Offset 2, 2
            ZWrite Off
            ZTest Always
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag

            float3 _TopLeftAnchor;
            float3 _BottomRightAnchor;
            float _vertexSize;

            struct appdata {
                float4 vertex : POSITION;
            };

            struct v2g {
                float4 pos : POSITION;
                float3 worldPos : TEXCOORD0;
            };

            struct g2f {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD1;
            };

            v2g vert(appdata v) {
                v2g o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            [maxvertexcount(3)]
            void geom(point v2g input[1], inout TriangleStream<g2f> stream)
            {
                float size = _vertexSize;
                float4 p = input[0].pos;
                float3 worldPos = input[0].worldPos;
                float aspectRatio = _ScreenParams.x / _ScreenParams.y;

                float2 offsets[3] = {
                    float2(0, size * aspectRatio),
                    float2(-0.866 * size, -0.5 * size * aspectRatio),
                    float2(0.866 * size, -0.5 * size * aspectRatio)
                };
                
                for (int i = 0; i < 3; i++)
                {
                    g2f o;
                    o.pos = p + float4(offsets[i], 0, 0);
                    o.worldPos = worldPos;
                    stream.Append(o);
                }
            }

            float4 frag(g2f i) : SV_Target {
                if (i.worldPos.x < _TopLeftAnchor.x || i.worldPos.y > _TopLeftAnchor.y ||
                    i.worldPos.x > _BottomRightAnchor.x || i.worldPos.y < _BottomRightAnchor.y) {
                    discard;
                }

                return float4(0,0,0,1);
            }

            ENDCG
        }
    }
}
