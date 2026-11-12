#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace TopView.Core
{
    // 에디터 부트스트랩: 에디터에서 App이 없는 씬(World 등)부터 Play해도 App이 생기도록, 씬을 불러온 직후 App 프리팹을 찾아 만든다.
    // 빌드는 App을 배치한 Menu 씬에서 시작하므로 이 파일이 필요 없다.
    public partial class App
    {
        // App 프리팹을 찾을 때 쓰는 검색어. 프리팹 파일 이름에 이 글자가 들어 있어야 한다
        private const string EDITOR_PREFAB_SEARCH_FILTER = "App t:Prefab";

        // 씬 오브젝트의 Awake 뒤, Start 전에 불린다. World와 MenuFlow는 Start에서 등록하므로 여기서 만든 App에도 등록된다
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreateForEditor()
        {
            if (Instance != null)
            {
                return;
            }

            App prefab = null;
            int prefabCount = 0;
            string[] guids = AssetDatabase.FindAssets(EDITOR_PREFAB_SEARCH_FILTER);
            for (int guidIndex = 0; guidIndex < guids.Length; guidIndex++)
            {
                App candidate = AssetDatabase.LoadAssetAtPath<App>(AssetDatabase.GUIDToAssetPath(guids[guidIndex]));
                if (candidate == null)
                {
                    continue;
                }

                prefab = candidate;
                prefabCount++;
            }

            if (prefab == null)
            {
                Debug.LogError("App >> 씬에 App이 없고 App 컴포넌트가 붙은 프리팹(파일 이름에 App이 들어간)도 찾지 못해 App을 만들지 않았다. 볼륨, 설정, 씬 전환, 배경음악, 버튼 소리가 동작하지 않는다.");
                return;
            }

            if (prefabCount > 1)
            {
                Debug.LogWarning($"App >> App 컴포넌트가 붙은 프리팹이 {prefabCount}개라 {AssetDatabase.GetAssetPath(prefab)}로 App을 만들었다.");
            }

            Instantiate(prefab);
        }
    }
}
#endif
