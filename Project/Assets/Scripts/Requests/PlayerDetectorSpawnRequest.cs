using TopView.Components;
using UnityEngine;

namespace TopView.Requests
{
    // 플레이어 감지기를 만들어 달라는 요청. 판을 준비할 때 Core가 Player Detector Authoring마다 채워 큐에 넣는다.
    public struct PlayerDetectorSpawnRequest
    {
        // 위치(월드)
        public Vector3 position;
        // 감지 범위
        public PlayerDetectorRangeComponent range;
        // 발동할 스포너의 연결 번호 목록. PlayerDetectorSpawnSystem이 엔티티의 배열로 복사한다
        public PlayerDetectorLinksComponent links;
    }
}
