using TopView.Components;
using UnityEngine;

namespace TopView.Definitions
{
    // 씬 흐름 값. App 프리팹이 가리킨다
    [CreateAssetMenu(fileName = "Menu Settings Definition", menuName = "Definitions/Menu Settings Definition")]
    public class MenuSettingsDefinition : ScriptableObject
    {
        // 메뉴 흐름 값
        public MenuRuleComponent rule = new MenuRuleComponent
        {
            worldSceneName = "World",
            menuSceneName = "Menu",
            minimumLoadingTicks = 100
        };
    }
}
