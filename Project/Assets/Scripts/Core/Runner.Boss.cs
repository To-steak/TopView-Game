using TopView.Entities;
using UnityEngine;

namespace TopView.Core
{
    // 보스: 저장소 용량과 Gizmo. 지금은 보스를 만드는 곳이 없다
    public partial class Runner
    {
        // 보스를 만드는 곳이 없어 비어 있다. BossViewSystem은 살아 있는 보스가 있을 때만 읽는다
        private GameObject _bossViewPrefab = null;

        private const int BOSS_CAPACITY = 1;

#if UNITY_EDITOR
        // 살아 있는 보스의 피격 판정 원(청록)과 Body 공격 판정 원(빨강)을 Logic 위치에 그린다.
        private void DrawBossGizmos()
        {
            BossEntity[] bosses = _world.bosses.entities;
            for (int bossIndex = 0; bossIndex < bosses.Length; bossIndex++)
            {
                ref readonly BossEntity boss = ref bosses[bossIndex];
                if (boss.isAlive)
                {
                    Gizmos.color = Color.cyan;
                    DrawCircleGizmo(boss.position.current, boss.stats.hitRadius);
                    DrawBodyAttackGizmos(boss.attacks, boss.position.current, boss.rotation.current);
                }
            }
        }
#endif
    }
}
