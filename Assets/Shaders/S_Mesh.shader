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
        Tags { "RenderType"="Opaque" }
        Blend One OneMinusSrcAlpha
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float3 _TopLeftAnchor;
            float3 _BottomRightAnchor;
            fixed4 _Color;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR; 
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
                fixed4 color : COLOR;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                
                // tint color
                col = col * i.color;

                // Premultiply
                col.rgb *= col.a;

                // clip to viewport
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
