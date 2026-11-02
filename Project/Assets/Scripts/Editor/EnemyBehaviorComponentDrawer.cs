using TopView.Components;
using UnityEditor;

namespace TopView.Editor
{
    // EnemyBehaviorComponent의 사망 시간 칸에 클립 칸을 붙인다. 배회 간격은 애니메이션과 무관해 그대로 둔다.
    [CustomPropertyDrawer(typeof(EnemyBehaviorComponent))]
    public class EnemyBehaviorComponentDrawer : AnimationTicksDrawer
    {
        protected override bool IsLengthTicks(string propertyName)
        {
            return propertyName == nameof(EnemyBehaviorComponent.dieTicks);
        }
    }
}
