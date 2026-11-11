using UnityEditor;
using UnityEngine;

namespace TopView.Editor
{
    // 지형 FBX마다 Ground 레이어의 Prefab Variant를 만들고, FBX 머티리얼을 이름별 공용 머티리얼로 바꾼다. 지형 접두사로 시작하지 않는 FBX는 건너뛴다.
    public static class TilePrefabGenerator
    {
        // 지형으로 쓸 FBX 이름의 접두사
        private static readonly string[] TILE_MODEL_PREFIXES =
        {
            "cliff_",
            "ground_",
            "bridge_",
            "path_",
            "platform_",
        };

        private const string MODEL_FOLDER = "Assets/CC0/Nature Kit (2.1)/Models";
        private const string PREFAB_FOLDER = "Assets/Prefabs/Tile";
        private const string MATERIAL_FOLDER = "Assets/Materials";
        private const string MATERIAL_NAME_PREFIX = "Tile_";
        private const string TILE_LAYER_NAME = "Ground";

        /// <summary>
        /// 지형 FBX 폴더의 지형 모델마다 프리팹 폴더에 Prefab Variant를 만든다. 같은 이름의 프리팹은 GUID를 유지한 채 덮어쓴다.
        /// 공용 머티리얼은 없을 때만 만들어, 다시 실행해도 에디터에서 바꾼 값이 유지된다.
        /// </summary>
        [MenuItem("Tools/Generate Tile Prefabs")]
        public static void Generate()
        {
            int tileLayer = LayerMask.NameToLayer(TILE_LAYER_NAME);
            if (tileLayer < 0)
            {
                Debug.LogError($"TilePrefabGenerator >> {TILE_LAYER_NAME} 레이어가 없다.");
                return;
            }

            if (!AssetDatabase.IsValidFolder(MODEL_FOLDER))
            {
                Debug.LogError($"TilePrefabGenerator >> 모델 폴더가 없다: {MODEL_FOLDER}");
                return;
            }

            if (!AssetDatabase.IsValidFolder(PREFAB_FOLDER))
            {
                Debug.LogError($"TilePrefabGenerator >> 프리팹 폴더가 없다: {PREFAB_FOLDER}");
                return;
            }

            if (!AssetDatabase.IsValidFolder(MATERIAL_FOLDER))
            {
                Debug.LogError($"TilePrefabGenerator >> 머티리얼 폴더가 없다: {MATERIAL_FOLDER}");
                return;
            }

            string[] modelGuids = AssetDatabase.FindAssets("t:Model", new[] { MODEL_FOLDER });
            int createdCount = 0;
            int skippedCount = 0;
            int failedCount = 0;

            for (int index = 0; index < modelGuids.Length; index++)
            {
                string modelPath = AssetDatabase.GUIDToAssetPath(modelGuids[index]);
                GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
                if (model == null)
                {
                    Debug.LogWarning($"TilePrefabGenerator >> 모델을 읽지 못했다: {modelPath}");
                    failedCount++;
                    continue;
                }

                if (!IsTileModel(model.name))
                {
                    skippedCount++;
                    continue;
                }

                if (SavePrefab(model, tileLayer))
                {
                    createdCount++;
                }
                else
                {
                    failedCount++;
                }
            }

            Debug.Log($"TilePrefabGenerator >> 지형 {createdCount}개를 만들었다. 제외 {skippedCount}개, 실패 {failedCount}개.");
        }

        private static bool IsTileModel(string modelName)
        {
            for (int index = 0; index < TILE_MODEL_PREFIXES.Length; index++)
            {
                if (modelName.StartsWith(TILE_MODEL_PREFIXES[index], System.StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool SavePrefab(GameObject model, int tileLayer)
        {
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(model);
            try
            {
                if (instance.GetComponentInChildren<Collider>(true) == null)
                {
                    Debug.LogWarning($"TilePrefabGenerator >> 지형에 Collider가 없다. Generate Colliders를 확인한다: {model.name}");
                }

                SetLayer(instance, tileLayer);
                ReplaceMaterials(instance, model.name);

                string prefabPath = $"{PREFAB_FOLDER}/{model.name}.prefab";
                PrefabUtility.SaveAsPrefabAsset(instance, prefabPath, out bool isSuccess);
                if (!isSuccess)
                {
                    Debug.LogWarning($"TilePrefabGenerator >> 프리팹을 저장하지 못했다: {prefabPath}");
                }

                return isSuccess;
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }
        }

        private static void SetLayer(GameObject root, int layer)
        {
            Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
            for (int index = 0; index < transforms.Length; index++)
            {
                transforms[index].gameObject.layer = layer;
            }
        }

        private static void ReplaceMaterials(GameObject root, string modelName)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            for (int rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
            {
                Material[] materials = renderers[rendererIndex].sharedMaterials;
                for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                {
                    if (materials[materialIndex] == null)
                    {
                        Debug.LogWarning($"TilePrefabGenerator >> 머티리얼 칸이 비어 있다: {modelName}");
                        continue;
                    }

                    materials[materialIndex] = GetSharedMaterial(materials[materialIndex]);
                }

                renderers[rendererIndex].sharedMaterials = materials;
            }
        }

        // 같은 이름의 공용 머티리얼을 돌려준다. 없으면 FBX 머티리얼을 복사해 만든다.
        private static Material GetSharedMaterial(Material source)
        {
            string materialPath = $"{MATERIAL_FOLDER}/{MATERIAL_NAME_PREFIX}{source.name.TrimStart('_')}.mat";
            Material shared = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (shared == null)
            {
                shared = new Material(source);
                shared.hideFlags = HideFlags.None;
                AssetDatabase.CreateAsset(shared, materialPath);
            }

            return shared;
        }
    }
}
