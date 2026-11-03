using UnityEngine;

namespace TopView.Presentation.ViewContexts
{
    // 이번 프레임에 띄울 월드 글자 하나
    public struct WorldTextEntry
    {
        // 글자가 시작할 위치(월드)
        public Vector3 position;
        // 표시할 문자열
        public string text;
        // 글자 색
        public Color color;
    }
}
