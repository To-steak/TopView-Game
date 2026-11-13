using TopView.Components;

namespace TopView.Systems
{
    // 판이 진행된 시간에 따라 스포너에서 생기는 적의 레벨을 올린다. 이미 생긴 적의 레벨은 바꾸지 않는다.
    public static class EnemyLevelSystem
    {
        /// <summary>
        /// 다음 상승까지 남은 시간을 1틱 줄이고, 0이 되면 적 레벨을 1 올린 뒤 간격을 다시 채운다.
        /// </summary>
        /// <param name="enemyLevelUpIntervalTicks">적 레벨이 1 오르는 간격(틱)</param>
        /// <param name="progress">한 판의 진행 상태</param>
        public static void Tick(int enemyLevelUpIntervalTicks, ref GameProgressComponent progress)
        {
            progress.enemyLevelUpRemainingTicks--;
            if (progress.enemyLevelUpRemainingTicks > 0)
            {
                return;
            }

            progress.enemyLevel++;
            progress.enemyLevelUpRemainingTicks = enemyLevelUpIntervalTicks;
        }
    }
}
