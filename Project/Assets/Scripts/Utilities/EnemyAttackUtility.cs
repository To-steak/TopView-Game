using TopView.Components;

namespace TopView.Systems
{
    // 적 공격의 판정 도구.
    public static class EnemyAttackUtility
    {
        /// <summary>
        /// 공격 상태가 끝났는지 돌려준다. 공격을 내보낸 뒤 durationTicks까지 기다린다.
        /// </summary>
        /// <param name="attack">공격 값</param>
        /// <param name="stateElapsedTicks">공격 상태에 들어간 뒤 지난 시간(틱)</param>
        /// <param name="isAttackCommitted">이번 공격 상태에서 공격을 내보냈는지</param>
        /// <returns>공격 상태가 끝났으면 true</returns>
        public static bool IsAttackFinished(in EnemyAttackComponent attack, int stateElapsedTicks, bool isAttackCommitted)
        {
            return isAttackCommitted && stateElapsedTicks >= attack.durationTicks;
        }
    }
}
