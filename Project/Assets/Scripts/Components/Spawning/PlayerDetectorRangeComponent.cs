using System;

namespace TopView.Components
{
    // 플레이어 감지기의 범위 설정값. Player Detector Authoring에서 정한다.
    [Serializable]
    public struct PlayerDetectorRangeComponent
    {
        // XZ 평면의 감지 반경(유닛). 플레이어 캡슐 가장자리가 이 원에 닿으면 감지한다
        public float detectionRadius;
    }
}
