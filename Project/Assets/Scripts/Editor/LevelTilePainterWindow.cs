using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TopView.Editor
{
    /// <summary>
    /// 레벨 칠하기 창 (Tools → Level Tile Painter). 에디터 전용.
    /// 타일 모드는 격자 칸에 타일 FBX·프리팹을 놓고, 소품 모드는 격자와 상관없이 마우스 위치에 소품을 놓고, 방 모드는 테두리와 길을 갖춘 방을 한 번에 놓는다. 타일과 방은 고른 층의 씬 루트 "Level Tiles (Painted) - N Layer", 소품은 "Level Props (Painted)" 아래에 둔다.
    /// 채우기 모드는 드래그한 범위에서 고른 층의 빈 칸을 찾아, 가장 적은 수의 타일을 늘려 빈틈과 겹침 없이 채운다.
    /// 층은 서로 독립이라, 칠하기의 교체와 지우기는 고른 층의 타일만 건드린다.
    /// 클릭·드래그로 칠하고, Shift+클릭·드래그로 지운다. 썸네일을 Ctrl+클릭해 여러 개 고르면 그중 하나를 무작위로 놓는다.
    /// ] / [로 타일을 90도씩 돌린다 (R은 Unity 크기 조절 도구 단축키라 쓰지 않는다). Esc로 칠하기를 끈다.
    /// 한 번의 드래그는 Undo 한 번으로 되돌아간다. Scene 뷰 입력은 SceneView.duringSceneGui로 받는다 (에디터 전용이라 이벤트 금지 규칙 밖).
    /// </summary>
    public class LevelTilePainterWindow : EditorWindow
    {
        [SerializeField] private LevelTileSet _tileSet;
        [SerializeField] private LevelPaintMode _mode;
        [SerializeField] private List<int> _selectedTileIndexes = new List<int>();
        [SerializeField] private List<int> _selectedPropIndexes = new List<int>();
        // 브러시 중심에서 가장자리까지의 칸 수 (0이면 1×1, 1이면 3×3)
        [SerializeField] private int _brushRadius;
        // 타일 모드와 방 모드에서 칠하고 지울 층 번호
        [SerializeField] private int _tileLayer;
        [SerializeField] private int _quarterTurns;
        [SerializeField] private bool _isRandomRotation;
        [SerializeField] private bool _isPainting;
        [SerializeField] private LevelRoomShape _roomShape = new LevelRoomShape { interiorWidth = 9, interiorDepth = 9, topRoadLength = 3, bottomRoadLength = 3, leftRoadLength = 3, rightRoadLength = 3 };
        // 고른 방 종류. LevelTileSet.roomTiles의 번호다
        [SerializeField] private int _roomTilesIndex;

        private Vector2 _scrollPosition;
        private bool _isStroking;
        private bool _isErasingStroke;
        private int _strokeUndoGroup;
        private bool _hasLastStroke;
        private Vector2Int _lastStrokeCell;
        private Vector3 _lastStrokePosition;
        // 채우기 드래그를 시작한 칸 (끝 칸은 _lastStrokeCell이다)
        private Vector2Int _fillStartCell;
        // 채울 사각형 (채울 때마다 다시 채운다)
        private readonly List<RectInt> _fillRectangles = new List<RectInt>();
        // 이번 드래그에서 무작위로 고를 프리팹 (드래그를 시작할 때 채운다)
        private readonly List<GameObject> _strokePrefabs = new List<GameObject>();
        // 씬에 칠해 둔 타일 (드래그를 시작할 때 채우고, 칠하고 지울 때마다 고친다)
        private readonly List<GameObject> _strokeTiles = new List<GameObject>();
        // 이번 드래그에서 칠한 타일. 드래그 중에는 이 타일을 덮어쓰지 않는다
        private readonly HashSet<GameObject> _strokePaintedTiles = new HashSet<GameObject>();
        // 새 타일과 겹쳐 지울 타일 (칠할 때마다 다시 채운다)
        private readonly List<GameObject> _overlappingTiles = new List<GameObject>();
        // 방 하나의 타일 배치 (찍거나 미리보기를 그릴 때마다 다시 채운다)
        private readonly List<LevelRoomPiece> _roomPieces = new List<LevelRoomPiece>();
        // 창 가장자리 여백 스타일 (GUI 스킨이 준비된 뒤 OnGUI에서 한 번 만든다)
        private GUIStyle _paddingStyle;
        // 썸네일 목록 안쪽 여백 스타일 (선택 테두리가 스크롤 영역 가장자리에서 잘리지 않게)
        private GUIStyle _thumbnailAreaStyle;
        // 마지막으로 정한 미리보기 캐시 크기 (값이 바뀔 때만 다시 정한다)
        private int _previewCacheSize;

        // 번호가 LevelPaintMode와 같다
        private static readonly string[] MODE_LABELS = { "타일", "소품", "방", "채우기" };
        // 번호가 _brushRadius와 같다
        private static readonly string[] BRUSH_LABELS = { "1×1", "3×3", "5×5", "7×7", "9×9" };
        // 번호가 _tileLayer와 같다
        private static readonly string[] TILE_LAYER_LABELS = { "0", "1", "2", "3" };
        private static readonly Color SELECTED_COLOR = new Color(0.25f, 0.6f, 1f);
        private static readonly Color TILE_COLOR = new Color(0.3f, 0.9f, 1f);
        private static readonly Color PROP_COLOR = new Color(0.4f, 1f, 0.4f);
        private static readonly Color ERASE_COLOR = new Color(1f, 0.2f, 0.2f);

        private const string WINDOW_TITLE = "Level Tile Painter";
        // 칠한 타일을 층별로 모아 두는 씬 루트 오브젝트 이름 (앞 + 층 번호 + 뒤)
        private const string TILE_ROOT_NAME_PREFIX = "Level Tiles (Painted) - ";
        private const string TILE_ROOT_NAME_SUFFIX = " Layer";
        // 놓은 소품을 모아 두는 씬 루트 오브젝트 이름
        private const string PROP_ROOT_NAME = "Level Props (Painted)";
        private const string TILE_CONTROLS_HELP =
            "클릭 / 드래그 : 칠하기 (고른 층에서 겹치는 타일은 교체, 이번 드래그에서 칠한 타일은 유지)\n" +
            "Shift + 클릭 / 드래그 : 고른 층의 타일 지우기\n" +
            "썸네일 Ctrl + 클릭 : 여러 개 고르기 (칸마다 무작위)\n" +
            "오른쪽 대괄호 키 ] : 시계 방향으로 90도 회전\n" +
            "왼쪽 대괄호 키 [ : 반시계 방향으로 90도 회전\n" +
            "Esc : 칠하기 끄기\n" +
            "Alt + 드래그, 우클릭 드래그 : Scene 뷰 카메라 조작 (그대로)";
        private const string PROP_CONTROLS_HELP =
            "클릭 / 드래그 : 놓기 (1×1은 마우스 위치, 큰 브러시는 원 안에 흩뿌리기)\n" +
            "회전과 크기는 무작위, 소품끼리 Prop Spacing보다 가까우면 놓지 않음\n" +
            "Shift + 클릭 / 드래그 : 원 안의 소품 지우기\n" +
            "썸네일 Ctrl + 클릭 : 여러 개 고르기 (소품마다 무작위)\n" +
            "Esc : 칠하기 끄기\n" +
            "Alt + 드래그, 우클릭 드래그 : Scene 뷰 카메라 조작 (그대로)";

        private const string ROOM_CONTROLS_HELP =
            "클릭 : 방 놓기 (고른 층에서 겹치는 타일은 지워짐)\n" +
            "Shift + 클릭 : 미리보기 범위에서 고른 층의 타일 지우기\n" +
            "Esc : 칠하기 끄기\n" +
            "Alt + 드래그, 우클릭 드래그 : Scene 뷰 카메라 조작 (그대로)";

        private const string FILL_CONTROLS_HELP =
            "드래그 : 범위 정하기, 놓으면 고른 층의 빈 칸을 가장 적은 수의 타일로 채움\n" +
            "썸네일 Ctrl + 클릭 : 여러 개 고르기 (사각형마다 무작위)\n" +
            "Esc : 칠하기 끄기 (드래그 중이면 채우지 않음)\n" +
            "Alt + 드래그, 우클릭 드래그 : Scene 뷰 카메라 조작 (그대로)";

        // 방 모드의 길 설정 칸 너비 (픽셀)
        private const float ROAD_CELL_WIDTH = 70f;
        // 방 모드의 가운데 네모 크기 (픽셀)
        private const float ROOM_BOX_WIDTH = 90f;
        private const float ROOM_BOX_HEIGHT = 60f;
        // 가운데 네모와 길 설정 칸 사이 간격 (픽셀)
        private const float ROOM_BOX_SPACING = 8f;
        private const float MODE_BUTTON_HEIGHT = 26f;
        private const float PAINT_BUTTON_HEIGHT = 28f;
        private const float SELECTED_BORDER = 3f;
        private const float THUMBNAIL_SIZE = 72f;
        private const float THUMBNAIL_SPACING = 10f;
        private const float THUMBNAIL_LABEL_HEIGHT = 16f;
        private const float PREVIEW_FILL_ALPHA = 0.15f;
        // 같은 방향으로 보는 회전 차이 (도)
        private const float SAME_ROTATION_DEGREES = 1f;
        // 가장자리만 맞닿은 타일을 겹친 것으로 보지 않기 위한 여유 (월드 유닛)
        private const float OVERLAP_TOLERANCE = 0.001f;
        // 창 가장자리 여백 (픽셀)
        private const int WINDOW_PADDING = 10;
        // 요소 사이 세로 간격 (픽셀)
        private const float ELEMENT_SPACING = 10f;
        // 썸네일 그림과 이름 사이 간격
        private const float THUMBNAIL_INNER_SPACING = 2f;
        // 세로 스크롤바 너비 (열 개수 계산에서 뺀다)
        private const float SCROLLBAR_WIDTH = 16f;
        // 썸네일 목록 안쪽 여백 (픽셀, 선택 테두리 SELECTED_BORDER보다 커야 잘리지 않는다)
        private const int THUMBNAIL_AREA_PADDING = 15;
        // 미리보기 캐시에 타일 개수보다 더 잡아 둘 칸 (캐시를 함께 쓰는 다른 창의 몫)
        private const int PREVIEW_CACHE_MARGIN = 50;

        /// <summary>
        /// 창을 연다.
        /// </summary>
        [MenuItem("Tools/Level Tile Painter")]
        public static void Open()
        {
            GetWindow<LevelTilePainterWindow>(WINDOW_TITLE);
        }

        private void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        private void OnGUI()
        {
            if (_paddingStyle == null)
            {
                _paddingStyle = new GUIStyle();
                _paddingStyle.padding = new RectOffset(WINDOW_PADDING, WINDOW_PADDING, WINDOW_PADDING, WINDOW_PADDING);
            }

            if (_thumbnailAreaStyle == null)
            {
                _thumbnailAreaStyle = new GUIStyle();
                _thumbnailAreaStyle.padding = new RectOffset(THUMBNAIL_AREA_PADDING, THUMBNAIL_AREA_PADDING, THUMBNAIL_AREA_PADDING, THUMBNAIL_AREA_PADDING);
            }

            EditorGUILayout.BeginVertical(_paddingStyle);
            DrawContent();
            EditorGUILayout.EndVertical();
        }

        private void DrawContent()
        {
            _tileSet = (LevelTileSet)EditorGUILayout.ObjectField("Level Tile Set", _tileSet, typeof(LevelTileSet), false);
            if (_tileSet == null)
            {
                GUILayout.Space(ELEMENT_SPACING);
                EditorGUILayout.HelpBox("격자 설정과 타일 목록을 담은 Level Tile Set 에셋을 넣는다.\n(Create → TopView → Level Tile Set)", MessageType.Info);
                SetPainting(false);
                return;
            }

            GUILayout.Space(ELEMENT_SPACING);
            LevelPaintMode mode = (LevelPaintMode)GUILayout.Toolbar((int)_mode, MODE_LABELS, GUILayout.Height(MODE_BUTTON_HEIGHT));
            if (mode != _mode)
            {
                _mode = mode;
                SetPainting(true);
            }

            GUILayout.Space(ELEMENT_SPACING);
            string paintLabel = _isPainting ? "칠하기 끄기 (Esc)" : "칠하기 켜기";
            if (GUILayout.Button(paintLabel, GUILayout.Height(PAINT_BUTTON_HEIGHT)))
            {
                SetPainting(!_isPainting);
            }

            if (_mode == LevelPaintMode.Tile || _mode == LevelPaintMode.Prop)
            {
                GUILayout.Space(ELEMENT_SPACING);
                EditorGUILayout.LabelField("브러시 크기", EditorStyles.boldLabel);
                _brushRadius = GUILayout.Toolbar(_brushRadius, BRUSH_LABELS);
            }

            if (_mode != LevelPaintMode.Prop)
            {
                GUILayout.Space(ELEMENT_SPACING);
                EditorGUILayout.LabelField("층", EditorStyles.boldLabel);
                _tileLayer = GUILayout.Toolbar(_tileLayer, TILE_LAYER_LABELS);
            }

            if (_mode == LevelPaintMode.Tile)
            {
                GUILayout.Space(ELEMENT_SPACING);
                _isRandomRotation = EditorGUILayout.Toggle("무작위 회전", _isRandomRotation);
                if (!_isRandomRotation)
                {
                    EditorGUILayout.LabelField("현재 회전", (_quarterTurns * 90) + "°", EditorStyles.boldLabel);
                }
            }

            GUILayout.Space(ELEMENT_SPACING);
            DrawSettings();

            GUILayout.Space(ELEMENT_SPACING);
            EditorGUILayout.HelpBox(GetControlsHelp(), MessageType.None);

            if (_mode != LevelPaintMode.Room)
            {
                GUILayout.Space(ELEMENT_SPACING);
                DrawThumbnails();
            }
        }

        private string GetControlsHelp()
        {
            switch (_mode)
            {
                case LevelPaintMode.Tile:
                    return TILE_CONTROLS_HELP;
                case LevelPaintMode.Prop:
                    return PROP_CONTROLS_HELP;
                case LevelPaintMode.Fill:
                    return FILL_CONTROLS_HELP;
                default:
                    return ROOM_CONTROLS_HELP;
            }
        }

        /// <summary>
        /// 현재 모드에 쓰는 Level Tile Set 값을 그린다. 고친 값은 에셋에 저장되고 Undo로 되돌릴 수 있다.
        /// </summary>
        private void DrawSettings()
        {
            SerializedObject settings = new SerializedObject(_tileSet);
            settings.Update();

            EditorGUILayout.PropertyField(settings.FindProperty(nameof(LevelTileSet.origin)));
            if (_mode == LevelPaintMode.Tile)
            {
                EditorGUILayout.PropertyField(settings.FindProperty(nameof(LevelTileSet.tileScale)));
            }
            else if (_mode == LevelPaintMode.Prop)
            {
                GUILayout.Space(ELEMENT_SPACING);
                EditorGUILayout.LabelField("Props", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(settings.FindProperty(nameof(LevelTileSet.propMinimumScale)));
                EditorGUILayout.PropertyField(settings.FindProperty(nameof(LevelTileSet.propMaximumScale)));
                EditorGUILayout.PropertyField(settings.FindProperty(nameof(LevelTileSet.propSpacing)));
                EditorGUILayout.PropertyField(settings.FindProperty(nameof(LevelTileSet.propScatterCount)));
            }
            else if (_mode == LevelPaintMode.Room)
            {
                DrawRoomSettings();
            }

            settings.ApplyModifiedProperties();
        }

        /// <summary>
        /// 방 크기와 길 낼 면을 그린다. 방을 찍을 수 없는 상태면 이유도 보여 준다.
        /// </summary>
        private void DrawRoomSettings()
        {
            GUILayout.Space(ELEMENT_SPACING);
            EditorGUILayout.LabelField("Room", EditorStyles.boldLabel);
            DrawRoomTilesToolbar();
            _roomShape.interiorWidth =Mathf.Max(LevelRoomLayout.MINIMUM_INTERIOR_SIZE, EditorGUILayout.IntField("안쪽 X 칸 수", _roomShape.interiorWidth));
            _roomShape.interiorDepth = Mathf.Max(LevelRoomLayout.MINIMUM_INTERIOR_SIZE, EditorGUILayout.IntField("안쪽 Z 칸 수", _roomShape.interiorDepth));
            GUILayout.Space(ELEMENT_SPACING);
            DrawRoadLayout();

            if (!HasRoomPrefabs())
            {
                EditorGUILayout.HelpBox("Level Tile Set 에셋의 Room Tiles에 방 종류를 넣고, 프리팹 4개를 모두 채워야 방을 놓을 수 있다.", MessageType.Warning);
            }
        }

        /// <summary>
        /// 방 종류를 고르는 버튼을 그린다. 이름이 비어 있으면 번호로 보여 준다.
        /// </summary>
        private void DrawRoomTilesToolbar()
        {
            LevelRoomTiles[] roomTiles = _tileSet.roomTiles;
            if (roomTiles == null || roomTiles.Length == 0)
            {
                return;
            }

            string[] labels = new string[roomTiles.Length];
            for (int index = 0; index < roomTiles.Length; index++)
            {
                labels[index] = string.IsNullOrEmpty(roomTiles[index].name) ? "Room " + (index + 1) : roomTiles[index].name;
            }
            _roomTilesIndex = GUILayout.Toolbar(Mathf.Clamp(_roomTilesIndex, 0, roomTiles.Length - 1), labels);
            GUILayout.Space(ELEMENT_SPACING);
        }

        /// <summary>
        /// 고른 방 종류를 구한다. 목록이 비어 있으면 프리팹이 없는 값이다.
        /// </summary>
        private LevelRoomTiles GetSelectedRoomTiles()
        {
            LevelRoomTiles[] roomTiles = _tileSet.roomTiles;
            if (roomTiles == null || roomTiles.Length == 0)
            {
                return default;
            }

            return roomTiles[Mathf.Clamp(_roomTilesIndex, 0, roomTiles.Length - 1)];
        }

        /// <summary>
        /// 가운데 네모를 방으로 삼아, 위에서 내려다본 방향대로 길 설정을 상하좌우에 그린다. 위가 +Z, 오른쪽이 +X다.
        /// </summary>
        private void DrawRoadLayout()
        {
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float lineSpacing = EditorGUIUtility.standardVerticalSpacing;
            float cellHeight = lineHeight * 2f + lineSpacing;
            float layoutHeight = cellHeight * 2f + ROOM_BOX_HEIGHT + ROOM_BOX_SPACING * 2f;
            Rect area = GUILayoutUtility.GetRect(0f, layoutHeight, GUILayout.ExpandWidth(true));

            Rect box = new Rect(area.center.x - ROOM_BOX_WIDTH * 0.5f, area.y + cellHeight + ROOM_BOX_SPACING, ROOM_BOX_WIDTH, ROOM_BOX_HEIGHT);
            GUI.Box(box, "방\n" + _roomShape.interiorWidth + " × " + _roomShape.interiorDepth);

            float centeredCellX = box.center.x - ROAD_CELL_WIDTH * 0.5f;
            float centeredCellY = box.center.y - cellHeight * 0.5f;
            Rect top = new Rect(centeredCellX, area.y, ROAD_CELL_WIDTH, cellHeight);
            Rect bottom = new Rect(centeredCellX, box.yMax + ROOM_BOX_SPACING, ROAD_CELL_WIDTH, cellHeight);
            Rect left = new Rect(box.x - ROOM_BOX_SPACING - ROAD_CELL_WIDTH, centeredCellY, ROAD_CELL_WIDTH, cellHeight);
            Rect right = new Rect(box.xMax + ROOM_BOX_SPACING, centeredCellY, ROAD_CELL_WIDTH, cellHeight);

            DrawRoadCell(top, "+Z", ref _roomShape.hasTopRoad, ref _roomShape.topRoadLength);
            DrawRoadCell(left, "-X", ref _roomShape.hasLeftRoad, ref _roomShape.leftRoadLength);
            DrawRoadCell(right, "+X", ref _roomShape.hasRightRoad, ref _roomShape.rightRoadLength);
            DrawRoadCell(bottom, "-Z", ref _roomShape.hasBottomRoad, ref _roomShape.bottomRoadLength);
        }

        /// <summary>
        /// 길 하나의 체크박스를 그리고, 길을 내는 방향이면 그 아래에 길이 입력칸을 그린다.
        /// </summary>
        private static void DrawRoadCell(Rect cell, string label, ref bool hasRoad, ref int roadLength)
        {
            float lineHeight = EditorGUIUtility.singleLineHeight;
            Rect toggleRect = new Rect(cell.x, cell.y, cell.width, lineHeight);
            Rect lengthRect = new Rect(cell.x, cell.yMax - lineHeight, cell.width, lineHeight);

            hasRoad = EditorGUI.ToggleLeft(toggleRect, label, hasRoad);
            if (hasRoad)
            {
                roadLength = EditorGUI.IntField(lengthRect, roadLength);
            }
            roadLength = Mathf.Max(LevelRoomLayout.MINIMUM_ROAD_LENGTH, roadLength);
        }

        private bool HasRoomPrefabs()
        {
            LevelRoomTiles tiles = GetSelectedRoomTiles();
            return tiles.openPrefab != null && tiles.sidePrefab != null && tiles.cornerPrefab != null && tiles.innerCornerPrefab != null;
        }

        private GameObject[] GetCurrentPrefabs()
        {
            return _mode == LevelPaintMode.Prop ? _tileSet.propPrefabs : _tileSet.prefabs;
        }

        private List<int> GetCurrentSelection()
        {
            return _mode == LevelPaintMode.Prop ? _selectedPropIndexes : _selectedTileIndexes;
        }

        /// <summary>
        /// 현재 모드의 목록을 썸네일 격자로 그린다. 썸네일을 불러오는 중이면 다시 그린다.
        /// </summary>
        private void DrawThumbnails()
        {
            GameObject[] prefabs = GetCurrentPrefabs();
            if (prefabs == null || prefabs.Length == 0)
            {
                string listName = _mode == LevelPaintMode.Prop ? "propPrefabs" : "prefabs";
                EditorGUILayout.HelpBox("Level Tile Set의 " + listName + "가 비어 있다. Project 창에서 FBX·프리팹을 끌어 넣는다.", MessageType.Warning);
                return;
            }

            // 캐시가 목록 개수보다 작으면 미리보기끼리 서로 밀어내 썸네일이 뜨지 않는다
            int previewCacheSize = prefabs.Length + PREVIEW_CACHE_MARGIN;
            if (previewCacheSize != _previewCacheSize)
            {
                AssetPreview.SetPreviewTextureCacheSize(previewCacheSize);
                _previewCacheSize = previewCacheSize;
            }

            NormalizeSelection(GetCurrentSelection(), prefabs.Length);
            float availableWidth = position.width - WINDOW_PADDING * 2f - SCROLLBAR_WIDTH - THUMBNAIL_AREA_PADDING * 2f;
            int columnCount = Mathf.Max(1, (int)((availableWidth + THUMBNAIL_SPACING) / (THUMBNAIL_SIZE + THUMBNAIL_SPACING)));
            int rowCount = (prefabs.Length + columnCount - 1) / columnCount;
            bool isLoading = false;

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            EditorGUILayout.BeginVertical(_thumbnailAreaStyle);
            for (int row = 0; row < rowCount; row++)
            {
                // 첫 줄이 아니면 위에 세로 간격 (마지막 줄 아래 간격은 여백이 맡는다)
                if (row > 0)
                {
                    GUILayout.Space(THUMBNAIL_SPACING);
                }
                EditorGUILayout.BeginHorizontal();
                for (int column = 0; column < columnCount; column++)
                {
                    int prefabIndex = row * columnCount + column;
                    if (prefabIndex >= prefabs.Length)
                    {
                        break;
                    }
                    // 첫 칸이 아니면 앞에 가로 간격
                    if (column > 0)
                    {
                        GUILayout.Space(THUMBNAIL_SPACING);
                    }
                    isLoading = DrawThumbnail(prefabs[prefabIndex], prefabIndex) || isLoading;
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();

            if (isLoading)
            {
                Repaint();
            }
        }

        /// <summary>
        /// 목록 범위를 벗어난 선택을 지운다. 선택이 없으면 첫 번째를 고른다.
        /// </summary>
        private static void NormalizeSelection(List<int> selection, int prefabCount)
        {
            for (int selectionIndex = selection.Count - 1; selectionIndex >= 0; selectionIndex--)
            {
                if (selection[selectionIndex] < 0 || selection[selectionIndex] >= prefabCount)
                {
                    selection.RemoveAt(selectionIndex);
                }
            }

            if (selection.Count == 0)
            {
                selection.Add(0);
            }
        }

        /// <summary>
        /// 썸네일 하나 (이미지, 이름). 고르면 칠하기를 켠다.
        /// 썸네일을 아직 불러오는 중이면 true.
        /// </summary>
        private bool DrawThumbnail(GameObject prefab, int prefabIndex)
        {
            Rect cellRect = GUILayoutUtility.GetRect(THUMBNAIL_SIZE, THUMBNAIL_SIZE + THUMBNAIL_INNER_SPACING + THUMBNAIL_LABEL_HEIGHT, GUILayout.Width(THUMBNAIL_SIZE));
            Rect imageRect = new Rect(cellRect.x, cellRect.y, THUMBNAIL_SIZE, THUMBNAIL_SIZE);
            Rect labelRect = new Rect(cellRect.x, imageRect.yMax + THUMBNAIL_INNER_SPACING, THUMBNAIL_SIZE, THUMBNAIL_LABEL_HEIGHT);

            // 고른 타일은 썸네일 둘레에만 테두리를 그린다 (이름 글자가 강조색에 묻히지 않게)
            if (GetCurrentSelection().Contains(prefabIndex))
            {
                EditorGUI.DrawRect(new Rect(imageRect.x - SELECTED_BORDER, imageRect.y - SELECTED_BORDER, imageRect.width + SELECTED_BORDER * 2f, imageRect.height + SELECTED_BORDER * 2f), SELECTED_COLOR);
            }

            bool isLoading = false;
            Texture2D preview = null;
            if (prefab != null)
            {
                preview = AssetPreview.GetAssetPreview(prefab);
                if (preview == null)
                {
                    isLoading = AssetPreview.IsLoadingAssetPreview(prefab.GetInstanceID());
                    preview = AssetPreview.GetMiniThumbnail(prefab);
                }
            }

            if (GUI.Button(imageRect, new GUIContent(preview, prefab != null ? prefab.name : "(비어 있음)")))
            {
                SelectThumbnail(prefabIndex);
            }
            GUI.Label(labelRect, prefab != null ? ShortenName(prefab.name) : "-", EditorStyles.centeredGreyMiniLabel);
            return isLoading;
        }

        /// <summary>
        /// 클릭은 그것 하나만 고르고, Ctrl+클릭은 선택에 넣거나 뺀다 (마지막 하나는 빼지 않는다).
        /// </summary>
        private void SelectThumbnail(int prefabIndex)
        {
            List<int> selection = GetCurrentSelection();
            if (!Event.current.control)
            {
                selection.Clear();
                selection.Add(prefabIndex);
            }
            else if (!selection.Contains(prefabIndex))
            {
                selection.Add(prefabIndex);
            }
            else if (selection.Count > 1)
            {
                selection.Remove(prefabIndex);
            }
            SetPainting(true);
        }

        /// <summary>
        /// Scene 뷰 입력을 받아 처리한다. 칠하기가 켜져 있는 동안 왼쪽 클릭이 오브젝트 선택 대신 쓰인다.
        /// </summary>
        private void OnSceneGUI(SceneView sceneView)
        {
            if (!_isPainting || _tileSet == null)
            {
                return;
            }

            Event current = Event.current;
            int controlIdentifier = GUIUtility.GetControlID(FocusType.Passive);
            if (current.type == EventType.Layout)
            {
                HandleUtility.AddDefaultControl(controlIdentifier);
                return;
            }

            Vector3 point;
            bool hasPoint = TryGetHoveredPoint(current.mousePosition, out point);
            switch (current.type)
            {
                case EventType.KeyDown:
                    HandleKey(current, sceneView);
                    break;
                case EventType.MouseDown:
                    if (current.button == 0 && !current.alt && hasPoint)
                    {
                        GUIUtility.hotControl = controlIdentifier;
                        BeginStroke(current.shift);
                        ApplyStroke(point);
                        current.Use();
                    }
                    break;
                case EventType.MouseDrag:
                    if (_isStroking && GUIUtility.hotControl == controlIdentifier)
                    {
                        if (hasPoint)
                        {
                            ApplyStroke(point);
                        }
                        current.Use();
                    }
                    sceneView.Repaint();
                    break;
                case EventType.MouseUp:
                    if (_isStroking && current.button == 0)
                    {
                        if (_mode == LevelPaintMode.Fill)
                        {
                            ApplyFill();
                        }
                        EndStroke();
                        GUIUtility.hotControl = 0;
                        current.Use();
                    }
                    break;
                case EventType.MouseMove:
                    sceneView.Repaint();
                    break;
                case EventType.Repaint:
                    if (hasPoint)
                    {
                        DrawBrushPreview(point, current.shift);
                    }
                    break;
            }
        }

        private void HandleKey(Event current, SceneView sceneView)
        {
            bool isRotateKey = current.keyCode == KeyCode.RightBracket || current.keyCode == KeyCode.LeftBracket;
            if (isRotateKey && _mode == LevelPaintMode.Tile)
            {
                _quarterTurns = (_quarterTurns + (current.keyCode == KeyCode.RightBracket ? 1 : 3)) % 4;
                current.Use();
                sceneView.Repaint();
                Repaint();
            }
            else if (current.keyCode == KeyCode.Escape)
            {
                SetPainting(false);
                current.Use();
            }
        }

        private void BeginStroke(bool isErasing)
        {
            Undo.IncrementCurrentGroup();
            _strokeUndoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("레벨 칠하기");
            _isStroking = true;
            _isErasingStroke = isErasing;
            _hasLastStroke = false;

            if (_mode != LevelPaintMode.Room)
            {
                CollectStrokePrefabs();
                if (!isErasing && _strokePrefabs.Count == 0)
                {
                    Debug.LogWarning("[LevelTilePainterWindow] 고른 프리팹이 없어 칠할 수 없다. Level Tile Set의 목록을 확인한다.");
                }
            }

            if (_mode != LevelPaintMode.Prop)
            {
                CollectStrokeTiles();
            }
        }

        private void CollectStrokePrefabs()
        {
            _strokePrefabs.Clear();
            GameObject[] prefabs = GetCurrentPrefabs();
            if (prefabs == null)
            {
                return;
            }

            List<int> selection = GetCurrentSelection();
            for (int selectionIndex = 0; selectionIndex < selection.Count; selectionIndex++)
            {
                int prefabIndex = selection[selectionIndex];
                if (prefabIndex >= 0 && prefabIndex < prefabs.Length && prefabs[prefabIndex] != null)
                {
                    _strokePrefabs.Add(prefabs[prefabIndex]);
                }
            }
        }

        private void CollectStrokeTiles()
        {
            _strokeTiles.Clear();
            _strokePaintedTiles.Clear();
            GameObject tileRoot = FindRootObject(GetTileRootName());
            if (tileRoot == null)
            {
                return;
            }

            Transform rootTransform = tileRoot.transform;
            for (int childIndex = 0; childIndex < rootTransform.childCount; childIndex++)
            {
                _strokeTiles.Add(rootTransform.GetChild(childIndex).gameObject);
            }
        }

        private void ApplyStroke(Vector3 point)
        {
            if (_mode == LevelPaintMode.Tile)
            {
                ApplyTileStroke(LevelGrid.GetCell(_tileSet, point));
            }
            else if (_mode == LevelPaintMode.Prop)
            {
                ApplyPropStroke(point);
            }
            else if (_mode == LevelPaintMode.Fill)
            {
                ApplyFillStroke(point);
            }
            else
            {
                ApplyRoomStroke(point);
            }
        }

        /// <summary>
        /// 드래그하는 동안 채울 범위의 시작 칸과 끝 칸을 기억한다. 실제로 채우는 일은 마우스를 놓을 때 한다.
        /// </summary>
        private void ApplyFillStroke(Vector3 point)
        {
            Vector2Int cell = GetUnitCell(point);
            if (!_hasLastStroke)
            {
                _fillStartCell = cell;
                _hasLastStroke = true;
            }
            _lastStrokeCell = cell;
        }

        /// <summary>
        /// 드래그한 범위에서 고른 층의 빈 칸을 찾아, 가장 적은 수의 사각형으로 나눠 타일을 놓는다.
        /// </summary>
        private void ApplyFill()
        {
            if (_strokePrefabs.Count == 0)
            {
                return;
            }

            int minimumColumn = Mathf.Min(_fillStartCell.x, _lastStrokeCell.x);
            int minimumRow = Mathf.Min(_fillStartCell.y, _lastStrokeCell.y);
            int width = Mathf.Abs(_fillStartCell.x - _lastStrokeCell.x) + 1;
            int depth = Mathf.Abs(_fillStartCell.y - _lastStrokeCell.y) + 1;

            bool[,] isEmptyCells = new bool[width, depth];
            for (int row = 0; row < depth; row++)
            {
                for (int column = 0; column < width; column++)
                {
                    isEmptyCells[column, row] = true;
                }
            }
            for (int tileIndex = 0; tileIndex < _strokeTiles.Count; tileIndex++)
            {
                MarkOccupiedCells(_strokeTiles[tileIndex].transform, isEmptyCells, minimumColumn, minimumRow);
            }

            LevelFillLayout.Build(isEmptyCells, _fillRectangles);
            if (_fillRectangles.Count == 0)
            {
                return;
            }

            Transform tileRoot = GetOrCreateRoot(GetTileRootName());
            for (int rectangleIndex = 0; rectangleIndex < _fillRectangles.Count; rectangleIndex++)
            {
                RectInt rectangle = _fillRectangles[rectangleIndex];
                LevelRoomPiece piece = new LevelRoomPiece
                {
                    prefab = _strokePrefabs[Random.Range(0, _strokePrefabs.Count)],
                    center = _tileSet.origin + new Vector3(
                        minimumColumn + rectangle.x + (rectangle.width - 1) * 0.5f,
                        0f,
                        minimumRow + rectangle.y + (rectangle.height - 1) * 0.5f),
                    width = rectangle.width,
                    depth = rectangle.height,
                    quarterTurns = 0,
                };
                PlaceRoomPiece(tileRoot, piece);
            }
        }

        /// <summary>
        /// 타일이 조금이라도 덮는 칸을 빈 칸에서 뺀다. 칸 경계에 맞지 않는 타일이 걸친 칸도 뺀다 (새 타일이 겹치지 않게).
        /// </summary>
        private void MarkOccupiedCells(Transform tile, bool[,] isEmptyCells, int minimumColumn, int minimumRow)
        {
            Vector3 size = tile.TransformVector(Vector3.one);
            float halfWidth = Mathf.Abs(size.x) * 0.5f;
            float halfDepth = Mathf.Abs(size.z) * 0.5f;
            float centerColumn = tile.position.x - _tileSet.origin.x;
            float centerRow = tile.position.z - _tileSet.origin.z;

            int firstColumn = Mathf.Max(Mathf.CeilToInt(centerColumn - halfWidth - 0.5f + OVERLAP_TOLERANCE), minimumColumn);
            int lastColumn = Mathf.Min(Mathf.FloorToInt(centerColumn + halfWidth + 0.5f - OVERLAP_TOLERANCE), minimumColumn + isEmptyCells.GetLength(0) - 1);
            int firstRow = Mathf.Max(Mathf.CeilToInt(centerRow - halfDepth - 0.5f + OVERLAP_TOLERANCE), minimumRow);
            int lastRow = Mathf.Min(Mathf.FloorToInt(centerRow + halfDepth + 0.5f - OVERLAP_TOLERANCE), minimumRow + isEmptyCells.GetLength(1) - 1);

            for (int row = firstRow; row <= lastRow; row++)
            {
                for (int column = firstColumn; column <= lastColumn; column++)
                {
                    isEmptyCells[column - minimumColumn, row - minimumRow] = false;
                }
            }
        }

        /// <summary>
        /// 월드 위치가 속한 1유닛 칸. origin이 칸 (0, 0)의 중심이고 Tile Scale과 상관없다.
        /// </summary>
        private Vector2Int GetUnitCell(Vector3 point)
        {
            return new Vector2Int(Mathf.RoundToInt(point.x - _tileSet.origin.x), Mathf.RoundToInt(point.z - _tileSet.origin.z));
        }

        /// <summary>
        /// 클릭 한 번에 방 하나를 놓거나 지운다. 드래그해도 다시 찍지 않는다.
        /// </summary>
        private void ApplyRoomStroke(Vector3 point)
        {
            if (_hasLastStroke)
            {
                return;
            }
            _hasLastStroke = true;

            if (!_isErasingStroke && !HasRoomPrefabs())
            {
                Debug.LogWarning("[LevelTilePainterWindow] 고른 방 종류의 프리팹이 비어 있어 방을 놓을 수 없다.");
                return;
            }

            LevelRoomTiles tiles = GetSelectedRoomTiles();
            LevelRoomLayout.Build(_tileSet.origin, in tiles, in _roomShape, point, _roomPieces);
            Transform tileRoot = _isErasingStroke ? null : GetOrCreateRoot(GetTileRootName());
            for (int pieceIndex = 0; pieceIndex < _roomPieces.Count; pieceIndex++)
            {
                LevelRoomPiece piece = _roomPieces[pieceIndex];
                EraseTilesInArea(piece.center, piece.width * 0.5f, piece.depth * 0.5f);
                if (!_isErasingStroke)
                {
                    PlaceRoomPiece(tileRoot, piece);
                }
            }
        }

        /// <summary>
        /// 방 타일 하나를 놓는다. 크기는 로컬 축 기준이라, 90도나 270도 돌리면 월드 X, Z 길이를 서로 바꿔 넣는다.
        /// </summary>
        private void PlaceRoomPiece(Transform tileRoot, in LevelRoomPiece piece)
        {
            Quaternion rotation = Quaternion.Euler(0f, piece.quarterTurns * 90f, 0f) * piece.prefab.transform.rotation;
            Vector3 localScale = piece.quarterTurns % 2 == 0 ? new Vector3(piece.width, 1f, piece.depth) : new Vector3(piece.depth, 1f, piece.width);

            GameObject tile = (GameObject)PrefabUtility.InstantiatePrefab(piece.prefab, tileRoot);
            Undo.RegisterCreatedObjectUndo(tile, "레벨 칠하기");
            tile.name = piece.prefab.name + " " + piece.center.x + "," + piece.center.z;
            tile.transform.SetPositionAndRotation(piece.center, rotation);
            tile.transform.localScale = Vector3.Scale(piece.prefab.transform.localScale, localScale);
            if (_tileSet.markTilesStatic)
            {
                MarkBatchingStatic(tile.transform);
            }
            _strokeTiles.Add(tile);
        }

        private void EndStroke()
        {
            Undo.CollapseUndoOperations(_strokeUndoGroup);
            _isStroking = false;
        }

        /// <summary>
        /// 중심 칸이 바뀔 때마다 한 번씩 브러시 범위의 칸을 칠하거나 지운다.
        /// </summary>
        private void ApplyTileStroke(Vector2Int centerCell)
        {
            if (_hasLastStroke && centerCell == _lastStrokeCell)
            {
                return;
            }
            _lastStrokeCell = centerCell;
            _hasLastStroke = true;

            if (_isErasingStroke)
            {
                EraseTiles(centerCell);
                return;
            }

            if (_strokePrefabs.Count == 0)
            {
                return;
            }

            Transform tileRoot = GetOrCreateRoot(GetTileRootName());
            for (int offsetZ = -_brushRadius; offsetZ <= _brushRadius; offsetZ++)
            {
                for (int offsetX = -_brushRadius; offsetX <= _brushRadius; offsetX++)
                {
                    PaintTile(tileRoot, GetBrushTileCenter(centerCell, offsetX, offsetZ));
                }
            }
        }

        /// <summary>
        /// 브러시 중심 칸에서 타일 몇 개만큼 떨어진 타일의 중심. 브러시 안의 타일은 빈틈없이 타일 크기만큼 떨어져 있다.
        /// </summary>
        private Vector3 GetBrushTileCenter(Vector2Int centerCell, int offsetX, int offsetZ)
        {
            return LevelGrid.GetCellCenter(_tileSet, centerCell) + new Vector3(offsetX * _tileSet.tileScale.x, 0f, offsetZ * _tileSet.tileScale.z);
        }

        /// <summary>
        /// 위치에 고른 타일을 놓고 겹치는 타일을 지운다. 이번 드래그에서 칠한 타일과 겹치면 놓지 않는다 (브러시가 1칸씩 움직여도 타일이 겹겹이 쌓이지 않게).
        /// 같은 자리에 고른 타일 중 하나가 같은 방향으로 이미 있으면 그대로 둔다 (무작위 회전이면 방향은 보지 않는다).
        /// </summary>
        private void PaintTile(Transform tileRoot, Vector3 center)
        {
            float halfWidth = _tileSet.tileScale.x * 0.5f;
            float halfDepth = _tileSet.tileScale.z * 0.5f;
            _overlappingTiles.Clear();
            for (int tileIndex = 0; tileIndex < _strokeTiles.Count; tileIndex++)
            {
                GameObject existing = _strokeTiles[tileIndex];
                if (!IsOverlapping(existing.transform, center, halfWidth, halfDepth))
                {
                    continue;
                }

                if (_strokePaintedTiles.Contains(existing) || (IsSameFootprint(existing.transform, center) && IsSelectedTile(existing)))
                {
                    return;
                }
                _overlappingTiles.Add(existing);
            }

            for (int tileIndex = 0; tileIndex < _overlappingTiles.Count; tileIndex++)
            {
                _strokeTiles.Remove(_overlappingTiles[tileIndex]);
                Undo.DestroyObjectImmediate(_overlappingTiles[tileIndex]);
            }

            GameObject prefab = _strokePrefabs[Random.Range(0, _strokePrefabs.Count)];
            int quarterTurns = _isRandomRotation ? Random.Range(0, 4) : _quarterTurns;
            Quaternion rotation = Quaternion.Euler(0f, quarterTurns * 90f, 0f) * prefab.transform.rotation;

            GameObject tile = (GameObject)PrefabUtility.InstantiatePrefab(prefab, tileRoot);
            Undo.RegisterCreatedObjectUndo(tile, "레벨 칠하기");
            tile.name = prefab.name + " " + center.x + "," + center.z;
            tile.transform.SetPositionAndRotation(center, rotation);
            tile.transform.localScale = Vector3.Scale(prefab.transform.localScale, GetLocalTileScale(quarterTurns));
            if (_tileSet.markTilesStatic)
            {
                MarkBatchingStatic(tile.transform);
            }
            _strokeTiles.Add(tile);
            _strokePaintedTiles.Add(tile);
        }

        /// <summary>
        /// 타일이 중심과 절반 크기로 정한 XZ 사각형과 겹치는지 본다. 가장자리만 맞닿으면 겹친 것이 아니다.
        /// </summary>
        private static bool IsOverlapping(Transform tile, Vector3 center, float halfWidth, float halfDepth)
        {
            Vector3 size = tile.TransformVector(Vector3.one);
            return Mathf.Abs(tile.position.x - center.x) < halfWidth + Mathf.Abs(size.x) * 0.5f - OVERLAP_TOLERANCE
                && Mathf.Abs(tile.position.z - center.z) < halfDepth + Mathf.Abs(size.z) * 0.5f - OVERLAP_TOLERANCE;
        }

        /// <summary>
        /// 타일이 그 위치에서 지금 Tile Scale 크기로 놓여 있는지 본다.
        /// </summary>
        private bool IsSameFootprint(Transform tile, Vector3 center)
        {
            Vector3 size = tile.TransformVector(Vector3.one);
            return Mathf.Abs(tile.position.x - center.x) < OVERLAP_TOLERANCE
                && Mathf.Abs(tile.position.z - center.z) < OVERLAP_TOLERANCE
                && Mathf.Abs(Mathf.Abs(size.x) - _tileSet.tileScale.x) < OVERLAP_TOLERANCE
                && Mathf.Abs(Mathf.Abs(size.z) - _tileSet.tileScale.z) < OVERLAP_TOLERANCE;
        }

        /// <summary>
        /// 회전한 타일의 로컬 축에 줄 크기. 90도나 270도 돌리면 로컬 x, z가 월드 z, x를 향하므로 서로 바꿔야 칸에 맞는다.
        /// </summary>
        private Vector3 GetLocalTileScale(int quarterTurns)
        {
            Vector3 tileScale = _tileSet.tileScale;
            return quarterTurns % 2 == 0 ? tileScale : new Vector3(tileScale.z, tileScale.y, tileScale.x);
        }

        private bool IsSelectedTile(GameObject tile)
        {
            GameObject source = PrefabUtility.GetCorrespondingObjectFromSource(tile);
            if (source == null || !_strokePrefabs.Contains(source))
            {
                return false;
            }
            if (_isRandomRotation)
            {
                return true;
            }

            Quaternion rotation = Quaternion.Euler(0f, _quarterTurns * 90f, 0f) * source.transform.rotation;
            return Quaternion.Angle(tile.transform.rotation, rotation) < SAME_ROTATION_DEGREES;
        }

        /// <summary>
        /// 브러시 전체 범위와 겹치는 타일을 지운다.
        /// </summary>
        private void EraseTiles(Vector2Int centerCell)
        {
            Vector3 center = LevelGrid.GetCellCenter(_tileSet, centerCell);
            float halfWidth = (_brushRadius + 0.5f) * _tileSet.tileScale.x;
            float halfDepth = (_brushRadius + 0.5f) * _tileSet.tileScale.z;
            EraseTilesInArea(center, halfWidth, halfDepth);
        }

        /// <summary>
        /// 중심과 절반 크기로 정한 XZ 사각형과 겹치는 타일을 지운다.
        /// </summary>
        private void EraseTilesInArea(Vector3 center, float halfWidth, float halfDepth)
        {
            for (int tileIndex = _strokeTiles.Count - 1; tileIndex >= 0; tileIndex--)
            {
                GameObject existing = _strokeTiles[tileIndex];
                if (IsOverlapping(existing.transform, center, halfWidth, halfDepth))
                {
                    _strokeTiles.RemoveAt(tileIndex);
                    Undo.DestroyObjectImmediate(existing);
                }
            }
        }

        /// <summary>
        /// 지우기는 드래그하는 동안 계속, 놓기는 마우스가 일정 거리를 움직일 때마다 한 번씩 한다 (제자리에서 소품이 쌓이지 않게).
        /// </summary>
        private void ApplyPropStroke(Vector3 point)
        {
            float brushRadius = GetPropBrushRadius();
            if (_isErasingStroke)
            {
                EraseProps(point, brushRadius);
                return;
            }

            if (_strokePrefabs.Count == 0)
            {
                return;
            }

            bool isSinglePlacement = _brushRadius == 0;
            float stampDistance = isSinglePlacement ? _tileSet.propSpacing : Mathf.Max(_tileSet.propSpacing, brushRadius);
            if (_hasLastStroke && GetSquaredDistanceOnGround(point, _lastStrokePosition) < stampDistance * stampDistance)
            {
                return;
            }
            _lastStrokePosition = point;
            _hasLastStroke = true;

            Transform propRoot = GetOrCreateRoot(PROP_ROOT_NAME);
            if (isSinglePlacement)
            {
                TryPlaceProp(propRoot, point);
                return;
            }

            for (int attemptIndex = 0; attemptIndex < _tileSet.propScatterCount; attemptIndex++)
            {
                Vector2 offset = Random.insideUnitCircle * brushRadius;
                TryPlaceProp(propRoot, point + new Vector3(offset.x, 0f, offset.y));
            }
        }

        /// <summary>
        /// 위치에 고른 소품 중 하나를 무작위 회전·크기로 놓는다. 다른 소품과 Prop Spacing보다 가까우면 놓지 않는다.
        /// </summary>
        private void TryPlaceProp(Transform propRoot, Vector3 point)
        {
            float squaredSpacing = _tileSet.propSpacing * _tileSet.propSpacing;
            for (int childIndex = 0; childIndex < propRoot.childCount; childIndex++)
            {
                if (GetSquaredDistanceOnGround(propRoot.GetChild(childIndex).position, point) < squaredSpacing)
                {
                    return;
                }
            }

            GameObject prefab = _strokePrefabs[Random.Range(0, _strokePrefabs.Count)];
            Quaternion rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f) * prefab.transform.rotation;

            GameObject prop = (GameObject)PrefabUtility.InstantiatePrefab(prefab, propRoot);
            Undo.RegisterCreatedObjectUndo(prop, "레벨 칠하기");
            prop.transform.SetPositionAndRotation(point, rotation);
            prop.transform.localScale = prefab.transform.localScale * Random.Range(_tileSet.propMinimumScale, _tileSet.propMaximumScale);
            if (_tileSet.markTilesStatic)
            {
                MarkBatchingStatic(prop.transform);
            }
        }

        private void EraseProps(Vector3 point, float radius)
        {
            GameObject propRoot = FindRootObject(PROP_ROOT_NAME);
            if (propRoot == null)
            {
                return;
            }

            Transform rootTransform = propRoot.transform;
            float squaredRadius = radius * radius;
            for (int childIndex = rootTransform.childCount - 1; childIndex >= 0; childIndex--)
            {
                Transform child = rootTransform.GetChild(childIndex);
                if (GetSquaredDistanceOnGround(child.position, point) < squaredRadius)
                {
                    Undo.DestroyObjectImmediate(child.gameObject);
                }
            }
        }

        /// <summary>
        /// 소품 브러시의 반지름 (월드 유닛). Tile Scale과 상관없다.
        /// </summary>
        private float GetPropBrushRadius()
        {
            return _brushRadius + 0.5f;
        }

        private static float GetSquaredDistanceOnGround(Vector3 first, Vector3 second)
        {
            Vector3 offset = first - second;
            offset.y = 0f;
            return offset.sqrMagnitude;
        }

        /// <summary>
        /// 마우스가 가리키는 바닥(origin 높이의 수평면) 위 지점을 구한다.
        /// </summary>
        private bool TryGetHoveredPoint(Vector2 mousePosition, out Vector3 point)
        {
            point = Vector3.zero;
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
            Plane ground = new Plane(Vector3.up, new Vector3(0f, _tileSet.origin.y, 0f));
            float distance;
            if (!ground.Raycast(ray, out distance))
            {
                return false;
            }

            point = ray.GetPoint(distance);
            return true;
        }

        private void DrawBrushPreview(Vector3 point, bool isErasing)
        {
            if (_mode == LevelPaintMode.Tile)
            {
                DrawTileBrushPreview(LevelGrid.GetCell(_tileSet, point), isErasing);
            }
            else if (_mode == LevelPaintMode.Prop)
            {
                DrawPropBrushPreview(point, isErasing);
            }
            else if (_mode == LevelPaintMode.Fill)
            {
                DrawFillPreview(point);
            }
            else
            {
                DrawRoomPreview(point, isErasing);
            }
        }

        /// <summary>
        /// 채울 범위를 네모로 그린다. 드래그 전에는 마우스가 가리키는 칸 하나다.
        /// </summary>
        private void DrawFillPreview(Vector3 point)
        {
            Vector2Int endCell = GetUnitCell(point);
            Vector2Int startCell = _isStroking ? _fillStartCell : endCell;
            Vector3 minimumCorner = _tileSet.origin + new Vector3(Mathf.Min(startCell.x, endCell.x) - 0.5f, 0f, Mathf.Min(startCell.y, endCell.y) - 0.5f);
            Vector3 maximumCorner = _tileSet.origin + new Vector3(Mathf.Max(startCell.x, endCell.x) + 0.5f, 0f, Mathf.Max(startCell.y, endCell.y) + 0.5f);

            Vector3[] corners = new Vector3[4];
            corners[0] = minimumCorner;
            corners[1] = new Vector3(minimumCorner.x, minimumCorner.y, maximumCorner.z);
            corners[2] = maximumCorner;
            corners[3] = new Vector3(maximumCorner.x, minimumCorner.y, minimumCorner.z);
            Handles.DrawSolidRectangleWithOutline(corners, new Color(TILE_COLOR.r, TILE_COLOR.g, TILE_COLOR.b, PREVIEW_FILL_ALPHA), TILE_COLOR);
        }

        /// <summary>
        /// 방을 이루는 타일마다 네모를 그린다. 지우기면 빨간색이다.
        /// </summary>
        private void DrawRoomPreview(Vector3 point, bool isErasing)
        {
            Color outline = isErasing ? ERASE_COLOR : TILE_COLOR;
            Color fill = new Color(outline.r, outline.g, outline.b, PREVIEW_FILL_ALPHA);
            Vector3[] corners = new Vector3[4];
            LevelRoomTiles tiles = GetSelectedRoomTiles();
            LevelRoomLayout.Build(_tileSet.origin, in tiles, in _roomShape, point, _roomPieces);
            for (int pieceIndex = 0; pieceIndex < _roomPieces.Count; pieceIndex++)
            {
                LevelRoomPiece piece = _roomPieces[pieceIndex];
                float halfWidth = piece.width * 0.5f;
                float halfDepth = piece.depth * 0.5f;
                corners[0] = piece.center + new Vector3(-halfWidth, 0f, -halfDepth);
                corners[1] = piece.center + new Vector3(-halfWidth, 0f, halfDepth);
                corners[2] = piece.center + new Vector3(halfWidth, 0f, halfDepth);
                corners[3] = piece.center + new Vector3(halfWidth, 0f, -halfDepth);
                Handles.DrawSolidRectangleWithOutline(corners, fill, outline);
            }
        }

        /// <summary>
        /// 브러시가 덮는 칸마다 네모를 그린다. 지우기면 빨간색, 무작위 회전이 아니면 회전 방향(타일의 +Z 쪽) 선도 그린다.
        /// </summary>
        private void DrawTileBrushPreview(Vector2Int centerCell, bool isErasing)
        {
            float halfWidth = _tileSet.tileScale.x * 0.5f;
            float halfDepth = _tileSet.tileScale.z * 0.5f;
            Color outline = isErasing ? ERASE_COLOR : TILE_COLOR;
            Color fill = new Color(outline.r, outline.g, outline.b, PREVIEW_FILL_ALPHA);
            Vector3[] corners = new Vector3[4];
            for (int offsetZ = -_brushRadius; offsetZ <= _brushRadius; offsetZ++)
            {
                for (int offsetX = -_brushRadius; offsetX <= _brushRadius; offsetX++)
                {
                    Vector3 cellCenter = GetBrushTileCenter(centerCell, offsetX, offsetZ);
                    corners[0] = cellCenter + new Vector3(-halfWidth, 0f, -halfDepth);
                    corners[1] = cellCenter + new Vector3(-halfWidth, 0f, halfDepth);
                    corners[2] = cellCenter + new Vector3(halfWidth, 0f, halfDepth);
                    corners[3] = cellCenter + new Vector3(halfWidth, 0f, -halfDepth);
                    Handles.DrawSolidRectangleWithOutline(corners, fill, outline);
                }
            }

            if (!isErasing && !_isRandomRotation)
            {
                Vector3 center = LevelGrid.GetCellCenter(_tileSet, centerCell);
                Vector3 forward = Quaternion.Euler(0f, _quarterTurns * 90f, 0f) * Vector3.forward;
                Handles.color = outline;
                Handles.DrawLine(center, center + Vector3.Scale(forward, _tileSet.tileScale) * (_brushRadius + 0.5f));
            }
        }

        /// <summary>
        /// 소품 브러시 범위를 원으로 그린다. 지우기면 빨간색이다.
        /// </summary>
        private void DrawPropBrushPreview(Vector3 point, bool isErasing)
        {
            float radius = GetPropBrushRadius();
            Color outline = isErasing ? ERASE_COLOR : PROP_COLOR;
            Handles.color = new Color(outline.r, outline.g, outline.b, PREVIEW_FILL_ALPHA);
            Handles.DrawSolidDisc(point, Vector3.up, radius);
            Handles.color = outline;
            Handles.DrawWireDisc(point, Vector3.up, radius);
        }

        private void SetPainting(bool isPainting)
        {
            if (_isPainting == isPainting)
            {
                return;
            }

            _isPainting = isPainting;
            if (!isPainting && _isStroking)
            {
                EndStroke();
            }
            SceneView.RepaintAll();
            Repaint();
        }

        /// <summary>
        /// 고른 층의 씬 루트 이름.
        /// </summary>
        private string GetTileRootName()
        {
            return TILE_ROOT_NAME_PREFIX + _tileLayer + TILE_ROOT_NAME_SUFFIX;
        }

        private static GameObject FindRootObject(string objectName)
        {
            GameObject[] rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
            for (int rootIndex = 0; rootIndex < rootObjects.Length; rootIndex++)
            {
                if (rootObjects[rootIndex].name == objectName)
                {
                    return rootObjects[rootIndex];
                }
            }
            return null;
        }

        private static Transform GetOrCreateRoot(string rootName)
        {
            GameObject root = FindRootObject(rootName);
            if (root == null)
            {
                root = new GameObject(rootName);
                Undo.RegisterCreatedObjectUndo(root, "레벨 칠하기");
            }
            return root.transform;
        }

        private static void MarkBatchingStatic(Transform target)
        {
            Transform[] transforms = target.GetComponentsInChildren<Transform>(true);
            for (int transformIndex = 0; transformIndex < transforms.Length; transformIndex++)
            {
                GameObject targetObject = transforms[transformIndex].gameObject;
                GameObjectUtility.SetStaticEditorFlags(targetObject, GameObjectUtility.GetStaticEditorFlags(targetObject) | StaticEditorFlags.BatchingStatic);
            }
        }

        /// <summary>
        /// 썸네일 아래 이름에서 "ground_" 같은 앞부분을 떼어 짧게 보여 준다.
        /// </summary>
        private static string ShortenName(string prefabName)
        {
            int separatorIndex = prefabName.IndexOf('_');
            return separatorIndex >= 0 && separatorIndex < prefabName.Length - 1 ? prefabName.Substring(separatorIndex + 1) : prefabName;
        }
    }
}
