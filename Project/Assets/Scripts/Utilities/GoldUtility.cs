using TopView.Components;

namespace TopView.Utilities
{
    // 골드를 더하는 계산. 전리품과 디버그 명령이 같은 방식으로 더하도록 한곳에 둔다.
    public static class GoldUtility
    {
        /// <summary>
        /// 골드를 더한다. int 범위를 넘으면 음수가 되지 않도록 최댓값에서 멈추고, 0보다 작아지면 0에서 멈춘다.
        /// </summary>
        /// <param name="progress">한 판의 진행 상태. 골드를 고친다</param>
        /// <param name="amount">더할 골드. 음수면 뺀다</param>
        /// <returns>최댓값을 넘어 멈췄으면 true</returns>
        public static bool Add(ref GameProgressComponent progress, int amount)
        {
            long gold = (long)progress.gold + amount;
            bool isOverflowed = gold > int.MaxValue;
            if (isOverflowed)
            {
                gold = int.MaxValue;
            }
            else if (gold < 0)
            {
                gold = 0;
            }

            progress.gold = (int)gold;
            return isOverflowed;
        }
    }
}
