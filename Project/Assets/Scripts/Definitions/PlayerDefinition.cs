using TopView.Components;
using UnityEngine;

namespace TopView.Definitions
{
    // 플레이어의 값과 무기 구성
    [CreateAssetMenu(fileName = "Player Definition", menuName = "Definitions/Player Definition")]
    public class PlayerDefinition : ScriptableObject
    {
        // 이동 값
        public PlayerMovementComponent movement;
        // 체력과 마나 값
        public PlayerVitalsComponent vitals;
        // 행동 상태의 길이
        public PlayerActionComponent action;
        // 무기 목록. 번호 순서가 교체 키(1, 2, 3 …) 순서다
        public WeaponDefinition[] weapons;
        // PlayerView가 붙은 프리팹
        public GameObject viewPrefab;
        // 마나 게이지를 플레이어 위치에서 띄울 거리(월드)
        public Vector3 manaGaugeOffset;
        // 한 번의 근접 공격에서 맞힌 적을 기록할 수 있는 최대 수
        public int meleeHitCapacity;
        // 플레이어 카메라가 환경 카메라보다 앞서도록 주는 Cinemachine 우선순위
        public int cameraPriority;
    }
}
