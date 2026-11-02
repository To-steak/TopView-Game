using TopView.UserInterface;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace TopView.Editor
{
    // 인스펙터에서 UserInterfaceView와 그 자식 클래스를 그린다. 기본 필드 아래에 레이아웃 칸을 붙인다.
    // Content가 연결되어 있지 않으면 너비·높이와 Header/Body/Footer 높이를 입력받고, 생성 버튼으로 계층을 만들어 필드에 연결한다.
    // Content가 연결되어 있으면 Root의 너비·높이와 각 영역의 Layout Element 높이를 직접 읽고 고친다.
    [CustomEditor(typeof(UserInterfaceView), true)]
    public class UserInterfaceViewEditor : UnityEditor.Editor
    {
        // 생성 전 입력값. 생성 후에는 쓰지 않고 실제 컴포넌트 값을 읽는다.
        private float _width = DEFAULT_WIDTH;
        private float _height = DEFAULT_HEADER_HEIGHT + DEFAULT_BODY_HEIGHT + DEFAULT_FOOTER_HEIGHT;
        private float _headerHeight = DEFAULT_HEADER_HEIGHT;
        private float _bodyHeight = DEFAULT_BODY_HEIGHT;
        private float _footerHeight = DEFAULT_FOOTER_HEIGHT;

        // 생성 전 입력값의 처음 값(픽셀)
        private const float DEFAULT_WIDTH = 400f;
        private const float DEFAULT_HEADER_HEIGHT = 60f;
        private const float DEFAULT_BODY_HEIGHT = 180f;
        private const float DEFAULT_FOOTER_HEIGHT = 60f;
        private const string CONTENT_NAME = "Content";
        private const string HEADER_NAME = "Header";
        private const string BODY_NAME = "Body";
        private const string FOOTER_NAME = "Footer";
        private const string GENERATE_UNDO_NAME = "Generate User Interface View";
        private const string RESIZE_UNDO_NAME = "Resize User Interface View";

        /// <summary>
        /// 기본 필드를 그리고, 그 아래에 계층 생성 칸이나 크기 조절 칸을 그린다.
        /// </summary>
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Layout", EditorStyles.boldLabel);

            UserInterfaceView view = (UserInterfaceView)target;
            RectTransform root = view.transform as RectTransform;
            if (root == null)
            {
                EditorGUILayout.HelpBox("RectTransform이 없습니다. Canvas 아래의 UI 오브젝트에 붙여 주세요.", MessageType.Warning);
                return;
            }

            if (view.content == null)
            {
                DrawGenerateFields(root);
            }
            else
            {
                DrawResizeFields(view, root);
            }
        }

        // 생성 전 입력 칸과 생성 버튼을 그린다.
        private void DrawGenerateFields(RectTransform root)
        {
            _width = Mathf.Max(0f, EditorGUILayout.FloatField("Width", _width));
            _height = Mathf.Max(0f, EditorGUILayout.FloatField("Height", _height));
            _headerHeight = Mathf.Max(0f, EditorGUILayout.FloatField("Header Height", _headerHeight));
            _bodyHeight = Mathf.Max(0f, EditorGUILayout.FloatField("Body Height", _bodyHeight));
            _footerHeight = Mathf.Max(0f, EditorGUILayout.FloatField("Footer Height", _footerHeight));
            GetFlexibleHeights(_headerHeight, _bodyHeight, _footerHeight, out int headerFlexible, out int bodyFlexible, out int footerFlexible);
            DrawFlexibleHeights(headerFlexible, bodyFlexible, footerFlexible);

            if (root.Find(CONTENT_NAME) != null)
            {
                EditorGUILayout.HelpBox($"이미 {CONTENT_NAME} 자식이 있습니다. 생성하면 하나 더 만들어집니다. 기존 것을 쓰려면 필드에 직접 연결해 주세요.", MessageType.Info);
            }

            if (GUILayout.Button("Generate"))
            {
                Generate(root);
            }
        }

        // 생성된 계층의 Root 너비·높이와 영역 높이를 그리고, 고치면 실제 컴포넌트에 바로 반영한다.
        private void DrawResizeFields(UserInterfaceView view, RectTransform root)
        {
            EditorGUI.BeginChangeCheck();
            float width = Mathf.Max(0f, EditorGUILayout.FloatField("Width", root.rect.width));
            float height = Mathf.Max(0f, EditorGUILayout.FloatField("Height", root.rect.height));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(root, RESIZE_UNDO_NAME);
                root.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
                root.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            }

            LayoutElement header = GetAreaLayoutElement(HEADER_NAME, view.header);
            LayoutElement body = GetAreaLayoutElement(BODY_NAME, view.body);
            LayoutElement footer = GetAreaLayoutElement(FOOTER_NAME, view.footer);
            if (header == null || body == null || footer == null)
            {
                return;
            }

            EditorGUI.BeginChangeCheck();
            float headerHeight = Mathf.Max(0f, EditorGUILayout.FloatField("Header Height", header.preferredHeight));
            float bodyHeight = Mathf.Max(0f, EditorGUILayout.FloatField("Body Height", body.preferredHeight));
            float footerHeight = Mathf.Max(0f, EditorGUILayout.FloatField("Footer Height", footer.preferredHeight));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObjects(new Object[] { header, body, footer }, RESIZE_UNDO_NAME);
                header.preferredHeight = headerHeight;
                body.preferredHeight = bodyHeight;
                footer.preferredHeight = footerHeight;
                GetFlexibleHeights(headerHeight, bodyHeight, footerHeight, out int headerFlexible, out int bodyFlexible, out int footerFlexible);
                header.flexibleHeight = headerFlexible;
                body.flexibleHeight = bodyFlexible;
                footer.flexibleHeight = footerFlexible;
            }

            DrawFlexibleHeights(header.flexibleHeight, body.flexibleHeight, footer.flexibleHeight);
        }

        // 영역의 Layout Element를 찾는다. 영역이나 Layout Element가 없으면 경고하고 null을 돌려준다.
        private static LayoutElement GetAreaLayoutElement(string areaName, RectTransform area)
        {
            if (area == null)
            {
                EditorGUILayout.HelpBox($"{areaName}: 영역이 연결되어 있지 않습니다.", MessageType.Warning);
                return null;
            }

            LayoutElement layoutElement = area.GetComponent<LayoutElement>();
            if (layoutElement == null)
            {
                EditorGUILayout.HelpBox($"{areaName}: {area.name}에 Layout Element가 없습니다.", MessageType.Warning);
            }

            return layoutElement;
        }

        // 영역별 Flexible Height 비율을 읽기 전용으로 보여 준다.
        private static void DrawFlexibleHeights(float headerFlexible, float bodyFlexible, float footerFlexible)
        {
            EditorGUILayout.LabelField("Flexible Height", $"{headerFlexible} : {bodyFlexible} : {footerFlexible}");
        }

        // 세 높이를 정수로 반올림하고 최대공약수로 나눠 가장 작은 정수 비율을 만든다. 예: 60, 180, 60 → 1 : 3 : 1.
        // Root가 선호 높이 합과 달라도 이 비율로 늘고 줄어 세 영역의 비율이 유지된다. 모두 0이면 모두 0이다.
        private static void GetFlexibleHeights(float headerHeight, float bodyHeight, float footerHeight, out int headerFlexible, out int bodyFlexible, out int footerFlexible)
        {
            headerFlexible = Mathf.RoundToInt(headerHeight);
            bodyFlexible = Mathf.RoundToInt(bodyHeight);
            footerFlexible = Mathf.RoundToInt(footerHeight);
            int divisor = GetGreatestCommonDivisor(GetGreatestCommonDivisor(headerFlexible, bodyFlexible), footerFlexible);
            if (divisor == 0)
            {
                return;
            }

            headerFlexible /= divisor;
            bodyFlexible /= divisor;
            footerFlexible /= divisor;
        }

        // 0 이상의 두 정수의 최대공약수. 둘 다 0이면 0이다.
        private static int GetGreatestCommonDivisor(int first, int second)
        {
            while (second != 0)
            {
                int remainder = first % second;
                first = second;
                second = remainder;
            }

            return first;
        }

        // Root 크기를 입력값으로 맞추고 Content/Header/Body/Footer를 만들어 View의 필드에 연결한다. 한 번의 실행 취소로 되돌린다.
        private void Generate(RectTransform root)
        {
            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName(GENERATE_UNDO_NAME);

            Undo.RecordObject(root, GENERATE_UNDO_NAME);
            root.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _width);
            root.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _height);

            GameObject contentObject = new GameObject(CONTENT_NAME, typeof(RectTransform), typeof(CanvasGroup), typeof(VerticalLayoutGroup));
            RectTransform content = (RectTransform)contentObject.transform;
            SetParent(content, root);
            content.anchorMin = Vector2.zero;
            content.anchorMax = Vector2.one;
            content.offsetMin = Vector2.zero;
            content.offsetMax = Vector2.zero;

            VerticalLayoutGroup layoutGroup = contentObject.GetComponent<VerticalLayoutGroup>();
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = true;
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.childForceExpandHeight = false;
            Undo.RegisterCreatedObjectUndo(contentObject, GENERATE_UNDO_NAME);

            GetFlexibleHeights(_headerHeight, _bodyHeight, _footerHeight, out int headerFlexible, out int bodyFlexible, out int footerFlexible);
            RectTransform header = CreateArea(HEADER_NAME, content, _headerHeight, headerFlexible);
            RectTransform body = CreateArea(BODY_NAME, content, _bodyHeight, bodyFlexible);
            RectTransform footer = CreateArea(FOOTER_NAME, content, _footerHeight, footerFlexible);

            serializedObject.Update();
            serializedObject.FindProperty(nameof(UserInterfaceView.content)).objectReferenceValue = content;
            serializedObject.FindProperty(nameof(UserInterfaceView.header)).objectReferenceValue = header;
            serializedObject.FindProperty(nameof(UserInterfaceView.body)).objectReferenceValue = body;
            serializedObject.FindProperty(nameof(UserInterfaceView.footer)).objectReferenceValue = footer;
            serializedObject.ApplyModifiedProperties();

            Undo.CollapseUndoOperations(undoGroup);
        }

        // Layout Element의 선호 높이와 Flexible Height를 정한 영역 오브젝트를 Content 아래에 만든다.
        private static RectTransform CreateArea(string name, RectTransform content, float height, int flexibleHeight)
        {
            GameObject areaObject = new GameObject(name, typeof(RectTransform), typeof(LayoutElement));
            RectTransform area = (RectTransform)areaObject.transform;
            SetParent(area, content);
            LayoutElement layoutElement = areaObject.GetComponent<LayoutElement>();
            layoutElement.preferredHeight = height;
            layoutElement.flexibleHeight = flexibleHeight;
            Undo.RegisterCreatedObjectUndo(areaObject, GENERATE_UNDO_NAME);
            return area;
        }

        // 부모 밑에 붙이고 부모의 레이어를 따른다.
        private static void SetParent(RectTransform child, RectTransform parent)
        {
            child.SetParent(parent, false);
            child.gameObject.layer = parent.gameObject.layer;
        }
    }
}
