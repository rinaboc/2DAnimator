Shader "Unlit/S_Mesh"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _TopLeftAnchor ("Top Left Anchor", Vector) = (-10, 10, 0)
        _BottomRightAnchor ("Bottom Right Anchor", Vector) = (10, -10, 0)
        _Color ("Tint Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry"}
        Blend One OneMinusSrcAlpha
        LOD 100

        Pass
        {
            Name "BASE"
            ZWrite On
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.0
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                fixed4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float3 _TopLeftAnchor;
            float3 _BottomRightAnchor;
            fixed4 _Color;

            v2f vert (appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                fixed4 col = tex2D(_MainTex, i.uv);
                col = col * i.color;
                col.rgb *= col.a;

                if (i.worldPos.x < _TopLeftAnchor.x || 
                    i.worldPos.y > _TopLeftAnchor.y ||
                    i.worldPos.x > _BottomRightAnchor.x || 
                    i.worldPos.y < _BottomRightAnchor.y) {
                        discard;
                    }

                return col;
            }
            ENDCG
        }
    }
}