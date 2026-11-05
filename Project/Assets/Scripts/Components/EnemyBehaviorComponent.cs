using System;
using UnityEngine.Serialization;

namespace TopView.Components
{
    // 적의 이동, 회전 행동 값
    [Serializable]
    public struct EnemyBehaviorComponent
    {
        // 플레이어를 쫓을 때 남은 경로가 이 거리 안이면 멈춘다(유닛)
        [FormerlySerializedAs("attackDistance")]
        public float stopDistance;
        // 가속도(유닛/초²). 감속과 도착 감속에도 쓴다
        public float acceleration;
        // 걸을 때 진행 방향으로, 공격이 나가기 전에 플레이어 쪽으로 도는 최고 각속도(도/초)
        public float angularSpeed;
        // 사망 상태에 들어간 뒤 사라지기까지의 시간(틱)
        public int dieTicks;
        // 사망 상태가 끝나 사라질 때 게임을 끝내는지. 보스에 켠다
        public bool endsGameOnDeath;
        // 화면의 오버레이 체력 바에 체력을 보여 주는지. 보스에 켠다. 켜면 체력 게이지 프리팹이 없어도 경고하지 않는다
        public bool showsOverlayHealthBar;
    }
}
