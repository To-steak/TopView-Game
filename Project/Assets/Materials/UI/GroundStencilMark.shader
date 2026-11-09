// Ground 레이어가 화면 맨 앞인 픽셀의 스텐실에 1을 기록한다. 색과 깊이는 쓰지 않는다.
// URP Render Objects의 Override Material로만 쓴다.
Shader "Custom/GroundStencilMark"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            ColorMask 0
            ZWrite Off
            ZTest LEqual
            // 같은 메시를 다시 그릴 때 깊이가 미세하게 어긋나 표시가 끊기지 않게 살짝 당긴다
            Offset -1, -1

            Stencil
            {
                Ref 1
                Comp Always
                Pass Replace
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 vert(float4 vertex : POSITION) : SV_POSITION
            {
                return UnityObjectToClipPos(vertex);
            }

            fixed4 frag() : SV_Target
            {
                return 0;
            }
            ENDCG
        }
    }
}
