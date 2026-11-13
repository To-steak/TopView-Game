using TopView.Components;
using TopView.Definitions;
using TopView.Presentation.Views;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TopView.Editor
{
    // 무기 모델의 WeaponView.muzzle 위치를 플레이어 루트 기준 로컬 위치로 바꿔 무기 Shots 칸의 Spawn Offset을 채우는 인스펙터 도구.
    // 플레이어 프리팹을 미리보기 씬에 만들고 게임과 같은 방식으로 무기 모델을 무기 소켓에 붙인 뒤, AnimationPoseSampler로 그 칸의 클립을 그 칸의 Start Ticks 시점으로 샘플링해 위치를 잰다.
    public static class MuzzleOffsetBaker
    {
        /// <summary>
        /// 굽기 버튼의 높이.
        /// </summary>
        public static float Height => AnimationTickConverter.LineHeight;

        private const string BAKE_LABEL = "Bake Spawn Offset";

        /// <summary>
        /// 굽기 버튼을 그린다. 버튼을 누르면 그 칸의 Spawn Offset을 채운다. 클립이 없으면 프리팹의 자세 그대로 잰다.
        /// </summary>
        /// <param name="rect">그릴 영역(Height 높이)</param>
        /// <param name="shot">구울 Shots 칸. WeaponDefinition 안의 칸이어야 한다</param>
        /// <param name="clip">샘플링할 클립. 없으면 null</param>
        public static void Draw(Rect rect, SerializedProperty shot, AnimationClip clip)
        {
            Rect line = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
            WeaponDefinition definition = shot.serializedObject.targetObject as WeaponDefinition;
            if (definition == null)
            {
                EditorGUI.HelpBox(line, $"{nameof(WeaponDefinition)} 안의 칸에서만 굽는다.", MessageType.None);
                return;
            }

            Rect buttonRect = EditorGUI.IndentedRect(line);
            SerializedProperty startTicks = shot.FindPropertyRelative(nameof(ProjectileShotComponent.startTicks));
            if (GUI.Button(buttonRect, BAKE_LABEL) && TryBake(definition, clip, startTicks.intValue, out Vector3 offset))
            {
                shot.FindPropertyRelative(nameof(ProjectileShotComponent.spawnOffset)).vector3Value = offset;
            }
        }

        // 미리보기 씬에서 플레이어와 무기 모델을 만들어 총구 위치를 잰다. 필요한 프리팹이나 참조가 없으면 경고를 남기고 false
        private static bool TryBake(WeaponDefinition definition, AnimationClip clip, int attackStartTicks, out Vector3 offset)
        {
            offset = Vector3.zero;
            if (definition.modelPrefab == null)
            {
                Debug.LogWarning($"MuzzleOffsetBaker >> {definition.name}에 모델 프리팹이 없어 총구 위치를 굽지 않았다.");
                return false;
            }

            WeaponView weaponViewPrefab = definition.modelPrefab.GetComponentInChildren<WeaponView>(true);
            if (weaponViewPrefab == null || weaponViewPrefab.muzzle == null)
            {
                Debug.LogWarning($"MuzzleOffsetBaker >> {definition.modelPrefab.name}에 WeaponView나 muzzle이 없어 총구 위치를 굽지 않았다.");
                return false;
            }

            if (!TryFindPlayerViewPrefab(definition, out GameObject playerViewPrefab))
            {
                return false;
            }

            Scene previewScene = EditorSceneManager.NewPreviewScene();
            try
            {
                GameObject playerObject = (GameObject)PrefabUtility.InstantiatePrefab(playerViewPrefab, previewScene);
                Transform root = playerObject.transform;
                root.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

                PlayerView playerView = playerObject.GetComponent<PlayerView>();
                if (playerView == null || playerView.weaponSocket == null)
                {
                    Debug.LogWarning($"MuzzleOffsetBaker >> {playerViewPrefab.name}에 PlayerView나 weaponSocket이 없어 총구 위치를 굽지 않았다.");
                    return false;
                }

                // PlayerViewSystem이 무기 모델을 붙이는 방식과 같게 붙인다
                GameObject model = Object.Instantiate(definition.modelPrefab, playerView.weaponSocket);
                model.transform.localPosition = Vector3.zero;
                model.transform.localRotation = Quaternion.identity;
                Transform muzzle = model.GetComponentInChildren<WeaponView>(true).muzzle;

                return AnimationPoseSampler.TryMeasureLocalPosition(root, playerView.animator, clip, attackStartTicks, muzzle, nameof(MuzzleOffsetBaker), playerViewPrefab.name, out offset);
            }
            finally
            {
                EditorSceneManager.ClosePreviewScene(previewScene);
            }
        }

        // 이 무기를 무기 목록에 가진 PlayerDefinition을 찾아 그 플레이어 프리팹을 돌려준다. 여럿이면 처음 찾은 것을 쓴다
        private static bool TryFindPlayerViewPrefab(WeaponDefinition definition, out GameObject playerViewPrefab)
        {
            string[] guids = AssetDatabase.FindAssets($"t:{nameof(PlayerDefinition)}");
            foreach (string guid in guids)
            {
                PlayerDefinition player = AssetDatabase.LoadAssetAtPath<PlayerDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                if (player == null || player.weapons == null || player.viewPrefab == null)
                {
                    continue;
                }

                foreach (WeaponDefinition weapon in player.weapons)
                {
                    if (weapon == definition)
                    {
                        playerViewPrefab = player.viewPrefab;
                        return true;
                    }
                }
            }

            Debug.LogWarning($"MuzzleOffsetBaker >> {definition.name}을 무기 목록에 가지고 플레이어 프리팹이 있는 PlayerDefinition이 없어 총구 위치를 굽지 않았다.");
            playerViewPrefab = null;
            return false;
        }
    }
}
