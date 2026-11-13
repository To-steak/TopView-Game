using TopView.Components;
using UnityEngine;

namespace TopView.Requests
{
    // 플레이어를 만들어 달라는 요청. 판을 준비할 때 Core가 Authoring과 Definition 값으로 채워 큐에 넣는다.
    public struct PlayerSpawnRequest
    {
        // 위치(월드)
        public Vector3 position;
        // 회전(월드)
        public Quaternion rotation;
        // 이동 값
        public PlayerMovementComponent movement;
        // 체력과 마나 값
        public PlayerVitalsComponent vitals;
        // 행동 상태의 길이
        public PlayerActionComponent action;
        // 몸체 크기
        public BodyComponent body;
        // 사망 상태의 길이
        public DeathComponent death;
        // 강화 값
        public PlayerUpgradeComponent upgrade;
        // 무기 생성 요청 목록. owner와 slotIndex는 비어 있고, PlayerSpawnSystem이 채워 무기마다 넘긴다
        public WeaponSpawnRequest[] weapons;
    }
}
