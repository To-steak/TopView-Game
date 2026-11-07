using TopView.Components;
using UnityEngine;

namespace TopView.Definitions
{
    // 메뉴의 흐름 값
    [CreateAssetMenu(fileName = "Menu Settings Definition", menuName = "Definitions/Menu Settings Definition")]
    public class MenuSettingsDefinition : ScriptableObject
    {
        // 메뉴 흐름 값
        public MenuRuleComponent rule = new MenuRuleComponent
        {
            worldSceneName = "World",
            minimumLoadingTicks = 100
        };
    }
}
