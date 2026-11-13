using TopView.Components;

namespace TopView.Utilities
{
    // 적 공격의 판정 도구.
    public static class EnemyAttackUtility
    {
        /// <summary>
        /// 공격 한 번에 내보내는 발 수를 돌려준다. Body는 근접 공격, Projectile은 직선 투사체 공격의 발 수다.
        /// </summary>
        /// <param name="attack">공격 값</param>
        /// <returns>내보내는 발 수</returns>
        public static int GetShotCount(in EnemyAttackComponent attack)
        {
            if (attack.kind == EnemyAttackKind.Body)
            {
                return AttackUtility.GetShotCount(in attack.melee);
            }

            return AttackUtility.GetShotCount(in attack.straight);
        }

        /// <summary>
        /// 공격 상태가 끝났는지 돌려준다. 모든 발을 내보낸 뒤 durationTicks까지 기다린다.
        /// </summary>
        /// <param name="attack">공격 값</param>
        /// <param name="stateElapsedTicks">공격 상태에 들어간 뒤 지난 시간(틱)</param>
        /// <param name="committedShotCount">이번 공격 상태에서 내보낸 발 수</param>
        /// <returns>공격 상태가 끝났으면 true</returns>
        public static bool IsAttackFinished(in EnemyAttackComponent attack, int stateElapsedTicks, int committedShotCount)
        {
            return committedShotCount >= GetShotCount(in attack) && stateElapsedTicks >= attack.durationTicks;
        }
    }
}
