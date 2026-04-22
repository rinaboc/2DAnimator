Shader "Unlit/S_Clip"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _TopLeftAnchor ("Top Left Anchor", Vector) = (-10, 10, 0)
        _BottomRightAnchor ("Bottom Right Anchor", Vector) = (10, -10, 0)
    }
    SubShader
    {
        Blend One OneMinusSrcAlpha
        LOD 100

        Pass
        {
            ZWrite Off
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
            };

            fixed4 _Color;
            float3 _TopLeftAnchor;
            float3 _BottomRightAnchor;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                if (i.worldPos.x < _TopLeftAnchor.x || i.worldPos.y > _TopLeftAnchor.y ||
                    i.worldPos.x > _BottomRightAnchor.x || i.worldPos.y < _BottomRightAnchor.y) {
                    discard;
                }

                return _Color;
            }
            ENDCG
        }
    }
}
