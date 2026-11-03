// Ground가 맨 앞인 픽셀에서는 깊이를 무시하고, 나머지 픽셀에서는 깊이를 지켜 그린다.
// URP가 두 패스를 모두 실행하도록 LightMode 태그를 서로 다르게 둔다.
Shader "Custom/WorldUI"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags
        {
            "Queue"="Overlay"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        CGINCLUDE
        #include "UnityCG.cginc"

        struct appdata_t
        {
            float4 vertex   : POSITION;
            float4 color    : COLOR;
            float2 texcoord : TEXCOORD0;
        };

        struct v2f
        {
            float4 vertex   : SV_POSITION;
            fixed4 color    : COLOR;
            float2 texcoord : TEXCOORD0;
        };

        sampler2D _MainTex;
        fixed4 _Color;

        v2f vert(appdata_t IN)
        {
            v2f OUT;
            OUT.vertex = UnityObjectToClipPos(IN.vertex);
            OUT.texcoord = IN.texcoord;
            OUT.color = IN.color * _Color;
            return OUT;
        }

        fixed4 frag(v2f IN) : SV_Target
        {
            return tex2D(_MainTex, IN.texcoord) * IN.color;
        }
        ENDCG

        // Ground가 맨 앞인 픽셀: 땅을 뚫고 그린다
        Pass
        {
            Tags { "LightMode"="SRPDefaultUnlit" }
            ZTest Always

            Stencil
            {
                Ref 1
                Comp Equal
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDCG
        }

        // 나머지 픽셀: 몬스터와 벽의 깊이를 지킨다
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            ZTest LEqual

            Stencil
            {
                Ref 1
                Comp NotEqual
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDCG
        }
    }
}
