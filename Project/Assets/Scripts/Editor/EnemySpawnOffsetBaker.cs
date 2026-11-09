using System.Collections.Generic;
using TopView.Components;
using TopView.Definitions;
using TopView.Presentation.Views;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

namespace TopView.Editor
{
    // 적 프리팹의 EnemyView.muzzles 중 하나의 위치를 적 루트 기준 로컬 위치로 바꿔 Shots 칸의 Spawn Offset을 채우는 인스펙터 도구.
    // 적 프리팹을 미리보기 씬에 만들고, 그 칸의 클립을 그 칸의 Start Ticks 시점으로 샘플링해 위치를 잰다.
    // 고른 Muzzle 번호는 에셋에 저장하지 않고, 에디터가 켜져 있는 동안만 칸마다 기억한다. 처음에는 칸 번호와 같은 번호다.
    public static class EnemySpawnOffsetBaker
    {
        private static readonly Dictionary<string, int> MUZZLE_INDICES = new Dictionary<string, int>();

        private const int LINE_COUNT = 2;
        private const string MUZZLE_LABEL = "Muzzle";
        private const string BAKE_LABEL = "Bake Spawn Offset";
        private const string GRAPH_NAME = "Enemy Spawn Offset Bake";
        private const string MISSING_MUZZLE_NAME = "(None)";

        /// <summary>
        /// Muzzle 드롭다운과 굽기 버튼의 높이.
        /// </summary>
        public static float Height => AnimationTickConverter.LineHeight * LINE_COUNT;

        /// <summary>
        /// Muzzle 드롭다운과 굽기 버튼을 그린다. 버튼을 누르면 그 칸의 Spawn Offset을 채운다. 클립이 없으면 프리팹의 자세 그대로 잰다.
        /// </summary>
        /// <param name="rect">그릴 영역(Height 높이)</param>
        /// <param name="shot">구울 Shots 칸</param>
        /// <param name="clip">샘플링할 클립. 없으면 null</param>
        public static void Draw(Rect rect, SerializedProperty shot, AnimationClip clip)
        {
            Rect line = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
            EnemyDefinition definition = shot.serializedObject.targetObject as EnemyDefinition;
            if (definition == null)
            {
                EditorGUI.HelpBox(line, $"{nameof(EnemyDefinition)} 안의 칸에서만 굽는다.", MessageType.None);
                return;
            }

            EnemyView enemyViewPrefab = definition.viewPrefab != null ? definition.viewPrefab.GetComponent<EnemyView>() : null;
            Transform[] muzzles = enemyViewPrefab != null && enemyViewPrefab.muzzles != null ? enemyViewPrefab.muzzles : new Transform[0];

            string key = $"{definition.GetInstanceID()}:{shot.propertyPath}";
            if (!MUZZLE_INDICES.TryGetValue(key, out int muzzleIndex))
            {
                muzzleIndex = GetArrayIndex(shot.propertyPath);
            }
            muzzleIndex = Mathf.Clamp(muzzleIndex, 0, Mathf.Max(0, muzzles.Length - 1));

            string[] muzzleNames = new string[muzzles.Length];
            for (int index = 0; index < muzzles.Length; index++)
            {
                muzzleNames[index] = $"{index}: {(muzzles[index] != null ? muzzles[index].name : MISSING_MUZZLE_NAME)}";
            }

            using (new EditorGUI.DisabledScope(muzzles.Length == 0))
            {
                muzzleIndex = EditorGUI.Popup(line, MUZZLE_LABEL, muzzleIndex, muzzleNames);
            }
            MUZZLE_INDICES[key] = muzzleIndex;

            line.y += AnimationTickConverter.LineHeight;
            Rect buttonRect = EditorGUI.IndentedRect(line);
            SerializedProperty startTicks = shot.FindPropertyRelative(nameof(ProjectileShotComponent.startTicks));
            if (GUI.Button(buttonRect, BAKE_LABEL) && TryBake(definition, muzzleIndex, clip, startTicks.intValue, out Vector3 offset))
            {
                shot.FindPropertyRelative(nameof(ProjectileShotComponent.spawnOffset)).vector3Value = offset;
            }
        }

        // 미리보기 씬에서 적 프리팹을 만들어 고른 Muzzle의 위치를 잰다. 필요한 프리팹이나 참조가 없으면 경고를 남기고 false
        private static bool TryBake(EnemyDefinition definition, int muzzleIndex, AnimationClip clip, int startTicks, out Vector3 offset)
        {
            offset = Vector3.zero;
            if (definition.viewPrefab == null)
            {
                Debug.LogWarning($"EnemySpawnOffsetBaker >> {definition.name}에 View Prefab이 없어 발사 위치를 굽지 않았다.");
                return false;
            }

            EnemyView enemyViewPrefab = definition.viewPrefab.GetComponent<EnemyView>();
            if (enemyViewPrefab == null || enemyViewPrefab.muzzles == null || muzzleIndex >= enemyViewPrefab.muzzles.Length || enemyViewPrefab.muzzles[muzzleIndex] == null)
            {
                Debug.LogWarning($"EnemySpawnOffsetBaker >> {definition.viewPrefab.name}에 EnemyView가 없거나 Muzzles {muzzleIndex}번이 비어 있어 발사 위치를 굽지 않았다.");
                return false;
            }

            Scene previewScene = EditorSceneManager.NewPreviewScene();
            PlayableGraph graph = default;
            try
            {
                GameObject enemyObject = (GameObject)PrefabUtility.InstantiatePrefab(definition.viewPrefab, previewScene);
                Transform root = enemyObject.transform;
                root.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

                EnemyView enemyView = enemyObject.GetComponent<EnemyView>();
                Transform muzzle = enemyView.muzzles[muzzleIndex];

                if (clip != null)
                {
                    if (enemyView.animator == null)
                    {
                        Debug.LogWarning($"EnemySpawnOffsetBaker >> {definition.viewPrefab.name}에 Animator가 없어 클립을 샘플링할 수 없어 발사 위치를 굽지 않았다.");
                        return false;
                    }

                    float time = startTicks * Time.fixedDeltaTime;
                    if (time > clip.length)
                    {
                        Debug.LogWarning($"EnemySpawnOffsetBaker >> Start Ticks({startTicks}) 시점이 {clip.name}의 길이({clip.length}초)보다 길어 클립 끝에서 샘플링했다.");
                        time = clip.length;
                    }

                    enemyView.animator.applyRootMotion = false;
                    graph = PlayableGraph.Create(GRAPH_NAME);
                    graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
                    AnimationPlayableOutput output = AnimationPlayableOutput.Create(graph, GRAPH_NAME, enemyView.animator);
                    AnimationClipPlayable playable = AnimationClipPlayable.Create(graph, clip);
                    output.SetSourcePlayable(playable);
                    playable.SetTime(time);
                    graph.Evaluate();
                }

                // 게임에서 rotation * spawnOffset으로 되돌리므로 루트의 크기는 반영하지 않는다
                offset = Quaternion.Inverse(root.rotation) * (muzzle.position - root.position);
                return true;
            }
            finally
            {
                if (graph.IsValid())
                {
                    graph.Destroy();
                }
                EditorSceneManager.ClosePreviewScene(previewScene);
            }
        }

        // 칸 경로의 마지막 배열 번호를 돌려준다. 예: attacks.Array.data[1].attack.shots.Array.data[0] → 0
        private static int GetArrayIndex(string propertyPath)
        {
            int openIndex = propertyPath.LastIndexOf('[');
            int closeIndex = propertyPath.LastIndexOf(']');
            if (openIndex < 0 || closeIndex <= openIndex || !int.TryParse(propertyPath.Substring(openIndex + 1, closeIndex - openIndex - 1), out int arrayIndex))
            {
                return 0;
            }

            return arrayIndex;
        }
    }
}
