using System;

namespace TopView.Components
{
    // 레벨에 따라 커지는 값 하나. 값은 baseValue + 레벨 × incrementPerLevel이다
    [Serializable]
    public struct LevelScalingComponent
    {
        // 레벨 0일 때의 값
        public float baseValue;
        // 레벨 1마다 더하는 값
        public float incrementPerLevel;
    }
}
