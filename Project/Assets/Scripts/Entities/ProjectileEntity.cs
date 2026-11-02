using TopView.Components;
using UnityEngine;

namespace TopView.Entities
{
    // 투사체 하나의 데이터
    public struct ProjectileEntity
    {
        // 살아 있는 칸인지. false면 빈 칸이다
        public bool isAlive;
        // 이 칸이 채워진 횟수
        public int generation;

        // 투사체 값. 생성 때 무기나 공격의 값에서 복사한다
        public ProjectileComponent projectile;
        // 쏜 쪽의 진영. 진영이 다른 대상만 맞힌다
        public Faction faction;
        // 맞을 때의 피해량
        public float damage;

        // 직전 틱의 위치(월드). 판정 선분의 시작이자 View 보간에 쓴다
        public Vector3 previousPosition;
        // 이번 틱의 위치(월드)
        public Vector3 currentPosition;
        // 회전(월드). 날아가는 동안 바뀌지 않는다
        public Quaternion rotation;
        // 속도(월드, 유닛/초)
        public Vector3 velocity;
        // 생긴 뒤 지난 시간(틱)
        public int elapsedTicks;
        // 땅이나 벽에 닿아 멈췄는지. Grenade만 쓴다
        public bool isResting;
    }
}
