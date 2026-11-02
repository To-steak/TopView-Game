using TopView.Components;
using UnityEditor;

namespace TopView.Editor
{
    // BossBehaviorComponent의 도발·사망 시간 칸에 클립 칸을 붙인다. 공격 대기 시간은 애니메이션과 무관해 그대로 둔다.
    [CustomPropertyDrawer(typeof(BossBehaviorComponent))]
    public class BossBehaviorComponentDrawer : AnimationTicksDrawer
    {
        protected override bool IsLengthTicks(string propertyName)
        {
            return propertyName == nameof(BossBehaviorComponent.tauntTicks)
                || propertyName == nameof(BossBehaviorComponent.dieTicks);
        }
    }
}
