using UnityEditor;
using UnityEngine;

namespace TopView.Editor
{
    // ~Component를 펼쳐 그리면서, 애니메이션 길이를 뜻하는 ~Ticks 칸 옆에 클립 칸을 붙이는 PropertyDrawer의 기반.
    // 자식 클래스는 어떤 칸이 애니메이션 길이인지, 어떤 칸을 숨길지, 아래에 무엇을 더 그릴지 정한다.
    public abstract class AnimationTicksDrawer : PropertyDrawer
    {
        /// <summary>
        /// 접힌 줄과, 펼쳤을 때 보이는 칸과 덧붙이는 칸의 높이를 더한다.
        /// </summary>
        /// <param name="property">그릴 Component 칸</param>
        /// <param name="label">이름</param>
        /// <returns>높이</returns>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight;
            if (!property.isExpanded)
            {
                return height;
            }

            SerializedProperty child = property.Copy();
            SerializedProperty end = property.GetEndProperty();
            bool isEntering = true;
            while (child.NextVisible(isEntering) && !SerializedProperty.EqualContents(child, end))
            {
                isEntering = false;
                if (IsHidden(property, child.name))
                {
                    continue;
                }

                height += EditorGUIUtility.standardVerticalSpacing + GetChildHeight(child);
            }

            return height + GetExtraHeight(property);
        }

        /// <summary>
        /// 접는 줄을 그리고, 펼쳐져 있으면 칸을 하나씩 그린 뒤 덧붙이는 칸을 그린다.
        /// </summary>
        /// <param name="position">그릴 영역</param>
        /// <param name="property">그릴 Component 칸</param>
        /// <param name="label">이름</param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            Rect line = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.Foldout(line, property.isExpanded, label, true);
            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;
                float y = line.yMax;

                SerializedProperty child = property.Copy();
                SerializedProperty end = property.GetEndProperty();
                bool isEntering = true;
                while (child.NextVisible(isEntering) && !SerializedProperty.EqualContents(child, end))
                {
                    isEntering = false;
                    if (IsHidden(property, child.name))
                    {
                        continue;
                    }

                    y += EditorGUIUtility.standardVerticalSpacing;
                    float childHeight = GetChildHeight(child);
                    Rect childRect = new Rect(position.x, y, position.width, childHeight);
                    if (IsLengthTicks(child.name))
                    {
                        AnimationTickConverter.DrawLengthTicks(childRect, child);
                    }
                    else
                    {
                        EditorGUI.PropertyField(childRect, child, true);
                    }
                    y += childHeight;
                }

                DrawExtra(new Rect(position.x, y, position.width, GetExtraHeight(property)), property);
                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        // 클립 길이 전체로 채우는 ~Ticks 칸인지
        protected abstract bool IsLengthTicks(string propertyName);

        // 그리지 않을 칸인지. 다른 칸의 값(종류 등)에 따라 정할 수 있다
        protected virtual bool IsHidden(SerializedProperty property, string childName)
        {
            return false;
        }

        // 칸들 아래에 덧붙이는 영역의 높이
        protected virtual float GetExtraHeight(SerializedProperty property)
        {
            return 0f;
        }

        // 칸들 아래에 덧붙이는 영역을 그린다
        protected virtual void DrawExtra(Rect rect, SerializedProperty property)
        {
        }

        private float GetChildHeight(SerializedProperty child)
        {
            return IsLengthTicks(child.name) ? EditorGUIUtility.singleLineHeight : EditorGUI.GetPropertyHeight(child, true);
        }
    }
}
