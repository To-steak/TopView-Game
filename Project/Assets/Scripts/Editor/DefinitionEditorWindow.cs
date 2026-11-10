using UnityEditor;
using UnityEngine;

namespace TopView.Editor
{
    // Definition 에셋을 탭별로 모아 보여 주고, 고른 에셋의 인스펙터를 오른쪽에 그리는 편집 창
    public class DefinitionEditorWindow : EditorWindow
    {
        private DefinitionEditorTab _currentTab = DefinitionEditorTab.Settings;
        // 현재 탭의 묶음별 에셋. 번호가 TAB_GROUP_FILTERS의 묶음 번호와 같다
        private ScriptableObject[][] _groupAssets;
        private ScriptableObject _selectedAsset;
        private UnityEditor.Editor _assetEditor;
        private Vector2 _listScrollPosition;
        private Vector2 _detailScrollPosition;
        // 번호가 DefinitionEditorTab과 같다
        private static readonly string[] TAB_LABELS = { "Settings", "Weapons", "Player", "Enemy", "Loot" };
        // 탭마다 묶음별 검색 필터. 바깥 번호가 DefinitionEditorTab과 같다
        private static readonly string[][] TAB_GROUP_FILTERS =
        {
            new string[] { "t:GameSettingsDefinition", "t:MenuSettingsDefinition", "t:AudioSettingsDefinition", "t:MusicDefinition", "t:UserInterfaceSoundDefinition" },
            new string[] { "t:WeaponDefinition" },
            new string[] { "t:PlayerDefinition" },
            new string[] { "t:EnemyDefinition" },
            new string[] { "t:LootDefinition" }
        };
        // 묶음 머리글. 번호가 TAB_GROUP_FILTERS와 같고, 묶음이 둘 이상인 탭에서만 그린다
        private static readonly string[][] TAB_GROUP_LABELS =
        {
            new string[] { "Game Settings", "Menu Settings", "Audio Settings", "Music", "User Interface Sound" },
            new string[] { "Weapons" },
            new string[] { "Player" },
            new string[] { "Enemy" },
            new string[] { "Loot" }
        };

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
            string[] groupFilters = TAB_GROUP_FILTERS[(int)_currentTab];
            _groupAssets = new ScriptableObject[groupFilters.Length][];

            for (int groupIndex = 0; groupIndex < groupFilters.Length; groupIndex++)
            {
                string[] assetIdentifiers = AssetDatabase.FindAssets(groupFilters[groupIndex]);
                ScriptableObject[] assets = new ScriptableObject[assetIdentifiers.Length];

                for (int assetIndex = 0; assetIndex < assetIdentifiers.Length; assetIndex++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(assetIdentifiers[assetIndex]);
                    assets[assetIndex] = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                }

                _groupAssets[groupIndex] = assets;
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

            if (_groupAssets != null)
            {
                string[] groupLabels = TAB_GROUP_LABELS[(int)_currentTab];
                bool isGrouped = _groupAssets.Length > 1;

                for (int groupIndex = 0; groupIndex < _groupAssets.Length; groupIndex++)
                {
                    if (isGrouped)
                    {
                        if (groupIndex > 0)
                        {
                            EditorGUILayout.Space();
                        }
                        EditorGUILayout.LabelField(groupLabels[groupIndex], EditorStyles.boldLabel);
                    }

                    DrawAssetButtons(_groupAssets[groupIndex]);
                }
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawAssetButtons(ScriptableObject[] assets)
        {
            foreach (ScriptableObject asset in assets)
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
