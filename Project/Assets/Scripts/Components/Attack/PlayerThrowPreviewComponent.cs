using UnityEngine;

namespace TopView.Components
{
    // 런타임 값. 투척 충전 중에 지금 던지면 수류탄이 지나갈 궤도와 폭발할 자리. View가 읽어 그린다.
    public struct PlayerThrowPreviewComponent
    {
        // 미리보기를 보여 줄지. 투척 무기로 충전 중일 때만 true다
        public bool isVisible;
        // 궤도 위의 점(월드). 0번은 던지는 위치이고 1틱마다 하나씩이다. 재사용하는 배열이며 길이는 들고 있는 투척 무기 중 가장 긴 flightTicks + 1이다
        public Vector3[] points;
        // points에서 이번 틱에 채운 점 수
        public int pointCount;
        // 폭발할 자리(월드). 착탄점이거나, 가는 길에 땅이나 벽에 닿은 자리다
        public Vector3 impactPosition;
        // 폭발 반지름(유닛, XZ)
        public float explosionRadius;
    }
}
