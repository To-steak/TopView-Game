using System;
using TopView.Components;
using UnityEngine;

namespace TopView.Definitions
{
    // 적 공격 하나의 Logic 값과 View용 프리팹 묶음
    [Serializable]
    public struct EnemyAttackEntry
    {
        // 공격 값
        public EnemyAttackComponent attack;
        // 공격 상태에 들어갈 때 재생할 애니메이션
        public EnemyAttackAnimation animation;
        // 공격이 나갈 때 재생할 소리. Body는 판정이 시작될 때, 투사체 공격은 한 발이 나갈 때마다 재생한다. 없으면 null
        public AudioClip attackClip;
        // ProjectileView가 붙은 투사체 프리팹. Body 공격은 쓰지 않는다
        public GameObject projectilePrefab;
        // 투사체가 맞을 때 재생할 ParticleSystem 프리팹. 없으면 null이고, Body 공격은 쓰지 않는다
        public GameObject hitEffectPrefab;
    }
}
