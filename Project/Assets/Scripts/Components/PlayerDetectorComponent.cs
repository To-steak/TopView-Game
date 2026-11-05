using UnityEngine;

namespace TopView.Components
{
    // 플레이어 감지기. 판 시작 때 Core가 Player Detector Authoring에서 채우고, 게임 중에는 바꾸지 않는다.
    public struct PlayerDetectorComponent
    {
        // 위치(월드)
        public Vector3 position;
        // 범위 설정값
        public PlayerDetectorRangeComponent range;
        // 플레이어를 감지하면 발동할 적 스포너 번호 목록
        public int[] spawnerIndices;
    }
}
