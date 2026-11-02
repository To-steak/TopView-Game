using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TopView.Editor
{
    /// <summary>
    /// 레벨 칠하기 창 (Tools → Level Tile Painter). 에디터 전용.
    /// 타일 모드는 격자 칸에 타일 FBX·프리팹을 놓고, 소품 모드는 격자와 상관없이 마우스 위치에 소품을 놓는다. 타일은 씬 루트의 "Level Tiles (Painted)", 소품은 "Level Props (Painted)" 아래에 둔다.
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
        [SerializeField] private int _quarterTurns;
        [SerializeField] private bool _isRandomRotation;
        [SerializeField] private bool _isPainting;

        private Vector2 _scrollPosition;
        private bool _isStroking;
        private bool _isErasingStroke;
        private int _strokeUndoGroup;
        private bool _hasLastStroke;
        private Vector2Int _lastStrokeCell;
        private Vector3 _lastStrokePosition;
        // 이번 드래그에서 무작위로 고를 프리팹 (드래그를 시작할 때 채운다)
        private readonly List<GameObject> _strokePrefabs = new List<GameObject>();
        // 칸별로 칠해 둔 타일 (드래그를 시작할 때 채운다. 칠할 때마다 자식을 전부 훑지 않으려고 둔다)
        private readonly Dictionary<Vector2Int, GameObject> _strokeTiles = new Dictionary<Vector2Int, GameObject>();
        // 창 가장자리 여백 스타일 (GUI 스킨이 준비된 뒤 OnGUI에서 한 번 만든다)
        private GUIStyle _paddingStyle;
        // 썸네일 목록 안쪽 여백 스타일 (선택 테두리가 스크롤 영역 가장자리에서 잘리지 않게)
        private GUIStyle _thumbnailAreaStyle;
        // 마지막으로 정한 미리보기 캐시 크기 (값이 바뀔 때만 다시 정한다)
        private int _previewCacheSize;

        // 번호가 LevelPaintMode와 같다
        private static readonly string[] MODE_LABELS = { "타일", "소품" };
        // 번호가 _brushRadius와 같다
        private static readonly string[] BRUSH_LABELS = { "1×1", "3×3", "5×5", "7×7", "9×9" };
        private static readonly Color SELECTED_COLOR = new Color(0.25f, 0.6f, 1f);
        private static readonly Color TILE_COLOR = new Color(0.3f, 0.9f, 1f);
        private static readonly Color PROP_COLOR = new Color(0.4f, 1f, 0.4f);
        private static readonly Color ERASE_COLOR = new Color(1f, 0.2f, 0.2f);

        private const string WINDOW_TITLE = "Level Tile Painter";
        // 칠한 타일을 모아 두는 씬 루트 오브젝트 이름
        private const string TILE_ROOT_NAME = "Level Tiles (Painted)";
        // 놓은 소품을 모아 두는 씬 루트 오브젝트 이름
        private const string PROP_ROOT_NAME = "Level Props (Painted)";
        private const string TILE_CONTROLS_HELP =
            "클릭 / 드래그 : 칠하기 (같은 칸은 교체)\n" +
            "Shift + 클릭 / 드래그 : 지우기\n" +
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

        private const float MODE_BUTTON_HEIGHT = 26f;
        private const float PAINT_BUTTON_HEIGHT = 28f;
        private const float SELECTED_BORDER = 3f;
        private const float THUMBNAIL_SIZE = 72f;
        private const float THUMBNAIL_SPACING = 10f;
        private const float THUMBNAIL_LABEL_HEIGHT = 16f;
        private const float PREVIEW_FILL_ALPHA = 0.15f;
        // 같은 방향으로 보는 회전 차이 (도)
        private const float SAME_ROTATION_DEGREES = 1f;
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

            GUILayout.Space(ELEMENT_SPACING);
            EditorGUILayout.LabelField("브러시 크기", EditorStyles.boldLabel);
            _brushRadius = GUILayout.Toolbar(_brushRadius, BRUSH_LABELS);

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
            EditorGUILayout.HelpBox(_mode == LevelPaintMode.Tile ? TILE_CONTROLS_HELP : PROP_CONTROLS_HELP, MessageType.None);

            GUILayout.Space(ELEMENT_SPACING);
            DrawThumbnails();
        }

        private GameObject[] GetCurrentPrefabs()
        {
            return _mode == LevelPaintMode.Tile ? _tileSet.prefabs : _tileSet.propPrefabs;
        }

        private List<int> GetCurrentSelection()
        {
            return _mode == LevelPaintMode.Tile ? _selectedTileIndexes : _selectedPropIndexes;
        }

        /// <summary>
        /// 현재 모드의 목록을 썸네일 격자로 그린다. 썸네일을 불러오는 중이면 다시 그린다.
        /// </summary>
        private void DrawThumbnails()
        {
            GameObject[] prefabs = GetCurrentPrefabs();
            if (prefabs == null || prefabs.Length == 0)
            {
                string listName = _mode == LevelPaintMode.Tile ? "prefabs" : "propPrefabs";
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

            CollectStrokePrefabs();
            if (!isErasing && _strokePrefabs.Count == 0)
            {
                Debug.LogWarning("[LevelTilePainterWindow] 고른 프리팹이 없어 칠할 수 없다. Level Tile Set의 목록을 확인한다.");
            }

            if (_mode == LevelPaintMode.Tile)
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
            GameObject tileRoot = FindRootObject(TILE_ROOT_NAME);
            if (tileRoot == null)
            {
                return;
            }

            Transform rootTransform = tileRoot.transform;
            for (int childIndex = 0; childIndex < rootTransform.childCount; childIndex++)
            {
                Transform child = rootTransform.GetChild(childIndex);
                _strokeTiles[LevelGrid.GetCell(_tileSet, child.position)] = child.gameObject;
            }
        }

        private void ApplyStroke(Vector3 point)
        {
            if (_mode == LevelPaintMode.Tile)
            {
                ApplyTileStroke(LevelGrid.GetCell(_tileSet, point));
            }
            else
            {
                ApplyPropStroke(point);
            }
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

            if (!_isErasingStroke && _strokePrefabs.Count == 0)
            {
                return;
            }

            Transform tileRoot = _isErasingStroke ? null : GetOrCreateRoot(TILE_ROOT_NAME);
            for (int offsetZ = -_brushRadius; offsetZ <= _brushRadius; offsetZ++)
            {
                for (int offsetX = -_brushRadius; offsetX <= _brushRadius; offsetX++)
                {
                    Vector2Int cell = centerCell + new Vector2Int(offsetX, offsetZ);
                    if (_isErasingStroke)
                    {
                        EraseTile(cell);
                    }
                    else
                    {
                        PaintTile(tileRoot, cell);
                    }
                }
            }
        }

        /// <summary>
        /// 칸에 고른 타일을 놓는다. 고른 타일 중 하나가 같은 방향으로 이미 있으면 그대로 두고 (무작위 회전이면 방향은 보지 않는다), 그 밖의 타일이 있으면 교체한다.
        /// </summary>
        private void PaintTile(Transform tileRoot, Vector2Int cell)
        {
            GameObject existing;
            if (_strokeTiles.TryGetValue(cell, out existing))
            {
                if (IsSelectedTile(existing))
                {
                    return;
                }
                Undo.DestroyObjectImmediate(existing);
            }

            GameObject prefab = _strokePrefabs[Random.Range(0, _strokePrefabs.Count)];
            int quarterTurns = _isRandomRotation ? Random.Range(0, 4) : _quarterTurns;
            Quaternion rotation = Quaternion.Euler(0f, quarterTurns * 90f, 0f) * prefab.transform.rotation;

            GameObject tile = (GameObject)PrefabUtility.InstantiatePrefab(prefab, tileRoot);
            Undo.RegisterCreatedObjectUndo(tile, "레벨 칠하기");
            tile.name = prefab.name + " " + cell.x + "," + cell.y;
            tile.transform.SetPositionAndRotation(LevelGrid.GetCellCenter(_tileSet, cell), rotation);
            tile.transform.localScale = prefab.transform.localScale * _tileSet.tileScale;
            if (_tileSet.markTilesStatic)
            {
                MarkBatchingStatic(tile.transform);
            }
            _strokeTiles[cell] = tile;
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

        private void EraseTile(Vector2Int cell)
        {
            GameObject existing;
            if (_strokeTiles.TryGetValue(cell, out existing))
            {
                Undo.DestroyObjectImmediate(existing);
                _strokeTiles.Remove(cell);
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
        /// 소품 브러시의 반지름 (월드 유닛). 같은 크기의 타일 브러시 한 변의 절반이다.
        /// </summary>
        private float GetPropBrushRadius()
        {
            return (_brushRadius + 0.5f) * _tileSet.cellSize;
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
            else
            {
                DrawPropBrushPreview(point, isErasing);
            }
        }

        /// <summary>
        /// 브러시가 덮는 칸 테두리를 그린다. 지우기면 빨간색, 무작위 회전이 아니면 회전 방향(타일의 +Z 쪽) 선도 그린다.
        /// </summary>
        private void DrawTileBrushPreview(Vector2Int centerCell, bool isErasing)
        {
            Vector3 center = LevelGrid.GetCellCenter(_tileSet, centerCell);
            float half = (_brushRadius + 0.5f) * _tileSet.cellSize;
            Vector3[] corners =
            {
                center + new Vector3(-half, 0f, -half),
                center + new Vector3(-half, 0f, half),
                center + new Vector3(half, 0f, half),
                center + new Vector3(half, 0f, -half),
            };
            Color outline = isErasing ? ERASE_COLOR : TILE_COLOR;
            Color fill = new Color(outline.r, outline.g, outline.b, PREVIEW_FILL_ALPHA);
            Handles.DrawSolidRectangleWithOutline(corners, fill, outline);

            if (!isErasing && !_isRandomRotation)
            {
                Vector3 forward = Quaternion.Euler(0f, _quarterTurns * 90f, 0f) * Vector3.forward;
                Handles.color = outline;
                Handles.DrawLine(center, center + forward * half);
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
