using TopView.Components;
using UnityEditor;

namespace TopView.Editor
{
    // DeathComponent의 사망 시간 칸에 클립 칸을 붙인다.
    [CustomPropertyDrawer(typeof(DeathComponent))]
    public class DeathComponentDrawer : AnimationTicksDrawer
    {
        protected override bool IsLengthTicks(string propertyName)
        {
            return propertyName == nameof(DeathComponent.dieTicks);
        }
    }
}
