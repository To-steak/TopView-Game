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
        // 플레이어 카메라가 환경 카메라보다 앞서도록 주는 Cinemachine 우선순위
        public int cameraPriority;
        // 플레이어 카메라의 Yaw 회전 속도(도/초)
        public float cameraYawDegreesPerSecond = 90f;
        // 원거리 레이어 가중치가 0에서 1로 올라오는 시간(틱). 0이면 바로 1이 된다
        public int rangedLayerBlendInTicks;
        // 투척 미리보기 원이 바닥과 겹쳐 깜빡이지 않도록 폭발 위치보다 띄우는 높이(유닛)
        public float throwPreviewCircleHeightOffset = 0.02f;
    }
}
