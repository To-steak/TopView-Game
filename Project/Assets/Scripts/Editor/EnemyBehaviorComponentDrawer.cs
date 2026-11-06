using TopView.Components;
using UnityEditor;

namespace TopView.Editor
{
    // EnemyBehaviorComponent의 도발 시간과 사망 시간 칸에 클립 칸을 붙인다.
    [CustomPropertyDrawer(typeof(EnemyBehaviorComponent))]
    public class EnemyBehaviorComponentDrawer : AnimationTicksDrawer
    {
        protected override bool IsLengthTicks(string propertyName)
        {
            return propertyName == nameof(EnemyBehaviorComponent.tauntTicks)
                || propertyName == nameof(EnemyBehaviorComponent.dieTicks);
        }
    }
}
