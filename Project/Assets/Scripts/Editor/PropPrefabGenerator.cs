using UnityEditor;
using UnityEngine;

namespace TopView.Editor
{
    // 소품 FBX마다 Prefab Variant를 만든다. 막는 소품은 Prop 레이어로, 통과하는 소품은 Collider 없이 저장한다. FBX 머티리얼은 이름별 공용 머티리얼로 바꾼다.
    public static class PropPrefabGenerator
    {
        // 지나갈 수 있는 소품의 FBX 이름. Collider를 빼고 레이어를 바꾸지 않는다
        private static readonly string[] PASSABLE_MODEL_NAMES =
        {
            "Grass",
            "Grass_2",
            "Grass_Short",
            "Flowers",
            "Wheat",
            "Lilypad",
            "Corn_1",
            "Corn_2",
            "Plant_1",
            "Plant_2",
            "Plant_3",
            "Plant_4",
            "Plant_5",
        };

        private const string MODEL_FOLDER = "Assets/Local Assets/Ultimate Nature Pack by Quaternius/FBX";
        private const string PREFAB_FOLDER = "Assets/Prefabs/Prop";
        private const string MATERIAL_FOLDER = "Assets/Materials/Prop";
        private const string MATERIAL_NAME_PREFIX = "Prop_";
        private const string BLOCKING_LAYER_NAME = "Prop";

        /// <summary>
        /// 소품 FBX 폴더의 모델마다 프리팹 폴더에 Prefab Variant를 만든다. 같은 이름의 프리팹은 GUID를 유지한 채 덮어쓴다.
        /// 공용 머티리얼은 없을 때만 만들어, 다시 실행해도 에디터에서 바꾼 값이 유지된다.
        /// </summary>
        [MenuItem("Tools/Generate Prop Prefabs")]
        public static void Generate()
        {
            int blockingLayer = LayerMask.NameToLayer(BLOCKING_LAYER_NAME);
            if (blockingLayer < 0)
            {
                Debug.LogError($"PropPrefabGenerator >> {BLOCKING_LAYER_NAME} 레이어가 없다.");
                return;
            }

            if (!AssetDatabase.IsValidFolder(MODEL_FOLDER))
            {
                Debug.LogError($"PropPrefabGenerator >> 모델 폴더가 없다: {MODEL_FOLDER}");
                return;
            }

            if (!AssetDatabase.IsValidFolder(PREFAB_FOLDER))
            {
                Debug.LogError($"PropPrefabGenerator >> 프리팹 폴더가 없다: {PREFAB_FOLDER}");
                return;
            }

            if (!AssetDatabase.IsValidFolder(MATERIAL_FOLDER))
            {
                Debug.LogError($"PropPrefabGenerator >> 머티리얼 폴더가 없다: {MATERIAL_FOLDER}");
                return;
            }

            string[] modelGuids = AssetDatabase.FindAssets("t:Model", new[] { MODEL_FOLDER });
            int blockingCount = 0;
            int passableCount = 0;
            int failedCount = 0;

            for (int index = 0; index < modelGuids.Length; index++)
            {
                string modelPath = AssetDatabase.GUIDToAssetPath(modelGuids[index]);
                GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
                if (model == null)
                {
                    Debug.LogWarning($"PropPrefabGenerator >> 모델을 읽지 못했다: {modelPath}");
                    failedCount++;
                    continue;
                }

                bool isPassable = System.Array.IndexOf(PASSABLE_MODEL_NAMES, model.name) >= 0;
                if (!SavePrefab(model, isPassable, blockingLayer))
                {
                    failedCount++;
                    continue;
                }

                if (isPassable)
                {
                    passableCount++;
                }
                else
                {
                    blockingCount++;
                }
            }

            Debug.Log($"PropPrefabGenerator >> 막는 소품 {blockingCount}개, 통과하는 소품 {passableCount}개를 만들었다. 실패 {failedCount}개.");
        }

        private static bool SavePrefab(GameObject model, bool isPassable, int blockingLayer)
        {
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(model);
            try
            {
                if (isPassable)
                {
                    RemoveColliders(instance);
                }
                else
                {
                    if (instance.GetComponentInChildren<Collider>(true) == null)
                    {
                        Debug.LogWarning($"PropPrefabGenerator >> 막는 소품에 Collider가 없다. Generate Colliders를 확인한다: {model.name}");
                    }

                    SetLayer(instance, blockingLayer);
                }

                ReplaceMaterials(instance, model.name);

                string prefabPath = $"{PREFAB_FOLDER}/{model.name}.prefab";
                PrefabUtility.SaveAsPrefabAsset(instance, prefabPath, out bool isSuccess);
                if (!isSuccess)
                {
                    Debug.LogWarning($"PropPrefabGenerator >> 프리팹을 저장하지 못했다: {prefabPath}");
                }

                return isSuccess;
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }
        }

        private static void RemoveColliders(GameObject root)
        {
            Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
            for (int index = 0; index < colliders.Length; index++)
            {
                Object.DestroyImmediate(colliders[index]);
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
                        Debug.LogWarning($"PropPrefabGenerator >> 머티리얼 칸이 비어 있다: {modelName}");
                        continue;
                    }

                    materials[materialIndex] = GetSharedMaterial(materials[materialIndex]);
                }

                renderers[rendererIndex].sharedMaterials = materials;
            }
        }

        // 같은 이름의 공용 머티리얼을 돌려준다. 없으면 FBX 머티리얼을 복사해 만든다. Wood.002처럼 번호가 붙은 이름은 번호를 뗀 머티리얼로 합친다.
        private static Material GetSharedMaterial(Material source)
        {
            int suffixIndex = source.name.IndexOf('.');
            string baseName = suffixIndex < 0 ? source.name : source.name.Substring(0, suffixIndex);
            string materialPath = $"{MATERIAL_FOLDER}/{MATERIAL_NAME_PREFIX}{baseName}.mat";

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
