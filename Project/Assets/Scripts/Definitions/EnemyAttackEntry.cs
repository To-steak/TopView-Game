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
        // ProjectileView가 붙은 투사체 프리팹. Body 공격은 쓰지 않는다
        public GameObject projectilePrefab;
        // 투사체가 맞을 때 재생할 ParticleSystem 프리팹. 없으면 null이고, Body 공격은 쓰지 않는다
        public GameObject hitEffectPrefab;
    }
}
