using TopView.Components;
using UnityEditor;

namespace TopView.Editor
{
    // PlayerActionComponent의 상태 길이 칸마다 클립 칸을 붙인다. 클립을 넣으면 클립 길이 전체가 틱으로 채워진다.
    [CustomPropertyDrawer(typeof(PlayerActionComponent))]
    public class PlayerActionComponentDrawer : AnimationTicksDrawer
    {
        protected override bool IsLengthTicks(string propertyName)
        {
            return propertyName == nameof(PlayerActionComponent.dodgeTicks)
                || propertyName == nameof(PlayerActionComponent.landTicks)
                || propertyName == nameof(PlayerActionComponent.swapTicks)
                || propertyName == nameof(PlayerActionComponent.reloadTicks)
                || propertyName == nameof(PlayerActionComponent.dieTicks);
        }
    }
}
