using UnityEditor;
using UnityEngine;

namespace TopView.Editor
{
    // Definition 에셋을 탭별로 모아 보여 주고, 고른 에셋의 인스펙터를 오른쪽에 그리는 편집 창
    public class DefinitionEditorWindow : EditorWindow
    {
        private DefinitionEditorTab _currentTab = DefinitionEditorTab.GameSettings;
        private ScriptableObject[] _assets;
        private ScriptableObject _selectedAsset;
        private UnityEditor.Editor _assetEditor;
        private Vector2 _listScrollPosition;
        private Vector2 _detailScrollPosition;
        // 번호가 DefinitionEditorTab과 같다
        private static readonly string[] TAB_LABELS = { "Game Settings", "Weapons", "Player", "Enemy", "Boss" };
        private static readonly string[] TAB_FILTERS = { "t:GameSettingsDefinition", "t:WeaponDefinition", "t:PlayerDefinition", "t:EnemyDefinition", "t:BossDefinition" };

        private const string WINDOW_TITLE = "Definition Editor";
        private const string BOX_STYLE = "box";
        private const float TAB_HEIGHT = 30f;
        private const float LIST_WIDTH = 250f;
        private const float BUTTON_HEIGHT = 25f;

        /// <summary>
        /// 편집 창을 연다.
        /// </summary>
        [MenuItem("Tools/Definition Editor")]
        public static void ShowWindow()
        {
            GetWindow<DefinitionEditorWindow>(WINDOW_TITLE);
        }

        private void OnEnable()
        {
            LoadAssets();
        }

        private void LoadAssets()
        {
            string[] assetIdentifiers = AssetDatabase.FindAssets(TAB_FILTERS[(int)_currentTab]);
            _assets = new ScriptableObject[assetIdentifiers.Length];

            for (int assetIndex = 0; assetIndex < assetIdentifiers.Length; assetIndex++)
            {
                string path = AssetDatabase.GUIDToAssetPath(assetIdentifiers[assetIndex]);
                _assets[assetIndex] = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
            }
        }

        private void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            _currentTab = (DefinitionEditorTab)GUILayout.Toolbar((int)_currentTab, TAB_LABELS, GUILayout.Height(TAB_HEIGHT));

            if (EditorGUI.EndChangeCheck())
            {
                _selectedAsset = null;
                if (_assetEditor != null)
                {
                    DestroyImmediate(_assetEditor);
                }
                LoadAssets();
            }

            EditorGUILayout.Space();

            // 왼쪽 목록과 오른쪽 인스펙터로 나눈다
            EditorGUILayout.BeginHorizontal();
            DrawListPanel();
            DrawDetailPanel();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawListPanel()
        {
            EditorGUILayout.BeginVertical(BOX_STYLE, GUILayout.Width(LIST_WIDTH), GUILayout.ExpandHeight(true));
            _listScrollPosition = EditorGUILayout.BeginScrollView(_listScrollPosition);

            if (GUILayout.Button("Refresh", GUILayout.Height(BUTTON_HEIGHT)))
            {
                LoadAssets();
            }

            EditorGUILayout.Space();

            if (_assets != null)
            {
                foreach (ScriptableObject asset in _assets)
                {
                    if (asset == null)
                    {
                        continue;
                    }

                    GUI.backgroundColor = (_selectedAsset == asset) ? Color.cyan : Color.white;

                    if (GUILayout.Button(asset.name, GUILayout.Height(BUTTON_HEIGHT)))
                    {
                        _selectedAsset = asset;

                        if (_assetEditor != null)
                        {
                            DestroyImmediate(_assetEditor);
                        }
                        _assetEditor = UnityEditor.Editor.CreateEditor(_selectedAsset);
                    }
                    GUI.backgroundColor = Color.white;
                }
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawDetailPanel()
        {
            EditorGUILayout.BeginVertical(BOX_STYLE, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            _detailScrollPosition = EditorGUILayout.BeginScrollView(_detailScrollPosition);

            if (_selectedAsset != null && _assetEditor != null)
            {
                EditorGUILayout.LabelField($"{_selectedAsset.name}", EditorStyles.boldLabel);
                EditorGUILayout.Space();

                _assetEditor.OnInspectorGUI();
            }
            else
            {
                EditorGUILayout.HelpBox("Select an asset from the list on the left.", MessageType.Info);
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }
    }
}
