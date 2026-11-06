using TopView.Components;
using UnityEditor;

namespace TopView.Editor
{
    // PlayerDetectorRangeComponent를 펼쳐 그리면서, 범위 모양이 쓰지 않는 칸을 숨긴다. Circle은 크기 칸, Rectangle은 반경 칸이다.
    [CustomPropertyDrawer(typeof(PlayerDetectorRangeComponent))]
    public class PlayerDetectorRangeComponentDrawer : AnimationTicksDrawer
    {
        protected override bool IsLengthTicks(string propertyName)
        {
            return false;
        }

        protected override bool IsHidden(SerializedProperty property, string childName)
        {
            PlayerDetectorShapeKind shape = (PlayerDetectorShapeKind)property.FindPropertyRelative(nameof(PlayerDetectorRangeComponent.shape)).intValue;
            if (shape == PlayerDetectorShapeKind.Rectangle)
            {
                return childName == nameof(PlayerDetectorRangeComponent.detectionRadius);
            }

            return childName == nameof(PlayerDetectorRangeComponent.detectionSize);
        }
    }
}
