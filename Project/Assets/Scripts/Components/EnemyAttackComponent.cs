using System;
using UnityEngine;

namespace TopView.Components
{
    // 적·보스 공격 하나의 값
    [Serializable]
    public struct EnemyAttackComponent
    {
        // 공격 종류
        public EnemyAttackKind kind;
        // 공격 상태에 들어간 뒤 공격이 나가기까지의 시간(틱)
        public int commitTicks;
        // 공격 상태에 들어간 뒤 공격 상태가 끝나기까지의 시간(틱)
        public int durationTicks;
        // 공격력에 곱하는 배율
        public float damageMultiplier;
        // Body 판정 구의 반지름(유닛). Body만 쓴다
        public float bodyRadius;
        // Body 판정 구 중심을 적 앞쪽으로 띄우는 거리(유닛). Body만 쓴다
        public float bodyOffset;
        // 내보내는 투사체. Body가 아닌 공격이 쓴다
        public ProjectileComponent projectile;
        // 투사체가 생기는 위치. 공격하는 쪽의 회전 기준 로컬 위치. BossMissile에서는 짝수 번째 발사 위치다
        public Vector3 spawnOffset;
        // BossMissile의 홀수 번째 발사 위치. 공격하는 쪽의 회전 기준 로컬 위치
        public Vector3 alternateSpawnOffset;
    }
}
