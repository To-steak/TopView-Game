using UnityEditor;
using UnityEngine;

public class SOEditorWindow : EditorWindow
{
    private enum EditorTab { GameSettings, Weapons, Player, Enemy, EnemyAttack }
    private EditorTab _currentTab = EditorTab.GameSettings;
    private ScriptableObject[] _assets;
    private ScriptableObject _selectedAsset;
    private Editor _assetEditor;

    private Vector2 _scrollPosList;
    private Vector2 _scrollPosDetails;

    [MenuItem("Tools/Player Data Editor")]
    public static void ShowWindow()
    {
        GetWindow<SOEditorWindow>("플레이어 데이터 센터");
    }

    private void OnEnable()
    {
        LoadAssets();
    }

    private void LoadAssets()
    {
        string filter = "";
        switch (_currentTab)
        {
            case EditorTab.GameSettings: filter = "t:GameSettingsSO"; break;
            case EditorTab.Weapons: filter = "t:PlayerWeaponSO"; break;
            case EditorTab.Player: filter = "t:PlayerSO"; break;
            case EditorTab.Enemy: filter = "t:EnemySO "; break;
            case EditorTab.EnemyAttack: filter = "t:EnemyAttackSO"; break;
        }

        string[] guids = AssetDatabase.FindAssets(filter);
        _assets = new ScriptableObject[guids.Length];

        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            _assets[i] = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
        }
    }

    private void OnGUI()
    {
        EditorGUI.BeginChangeCheck();
        _currentTab = (EditorTab)GUILayout.Toolbar((int)_currentTab, new string[] { "게임 설정", "무기 설정", "플레이어", "몬스터", "몬스터 공격" }, GUILayout.Height(30));

        if (EditorGUI.EndChangeCheck())
        {
            _selectedAsset = null;
            if (_assetEditor != null) DestroyImmediate(_assetEditor);
            LoadAssets();
        }

        EditorGUILayout.Space();

        // 메인 화면 분할 레이아웃
        EditorGUILayout.BeginHorizontal();
        DrawLeftPanel();
        DrawRightPanel();
        EditorGUILayout.EndHorizontal();
    }

    private void DrawLeftPanel()
    {
        EditorGUILayout.BeginVertical("box", GUILayout.Width(250), GUILayout.ExpandHeight(true));
        _scrollPosList = EditorGUILayout.BeginScrollView(_scrollPosList);

        if (GUILayout.Button("Refresh", GUILayout.Height(25)))
        {
            LoadAssets();
        }

        EditorGUILayout.Space();

        if (_assets != null)
        {
            foreach (var asset in _assets)
            {
                if (asset == null) continue;

                GUI.backgroundColor = (_selectedAsset == asset) ? Color.cyan : Color.white;

                if (GUILayout.Button(asset.name, GUILayout.Height(25)))
                {
                    _selectedAsset = asset;

                    if (_assetEditor != null) DestroyImmediate(_assetEditor);
                    _assetEditor = Editor.CreateEditor(_selectedAsset);
                }
                GUI.backgroundColor = Color.white;
            }
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void DrawRightPanel()
    {
        EditorGUILayout.BeginVertical("box", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        _scrollPosDetails = EditorGUILayout.BeginScrollView(_scrollPosDetails);

        if (_selectedAsset != null && _assetEditor != null)
        {
            EditorGUILayout.LabelField($"{_selectedAsset.name}", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            _assetEditor.OnInspectorGUI();
        }
        else
        {
            EditorGUILayout.HelpBox("Select SO.asset", MessageType.Info);
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }
}