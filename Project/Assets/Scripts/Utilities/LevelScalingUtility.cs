using TopView.Components;

namespace TopView.Utilities
{
    // 레벨에 따라 커지는 값을 계산하는 도구.
    public static class LevelScalingUtility
    {
        /// <summary>
        /// 그 레벨에서의 값을 구한다. baseValue + 레벨 × incrementPerLevel이다.
        /// </summary>
        /// <param name="scaling">레벨에 따라 커지는 값</param>
        /// <param name="level">레벨</param>
        /// <returns>그 레벨에서의 값</returns>
        public static float GetValue(in LevelScalingComponent scaling, int level)
        {
            return scaling.baseValue + level * scaling.incrementPerLevel;
        }
    }
}
