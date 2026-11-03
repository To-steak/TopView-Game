using TopView.Components;
using UnityEngine;

namespace TopView.Definitions
{
    // 월드 글자의 프리팹과 표시 값
    [CreateAssetMenu(fileName = "World Text Definition", menuName = "Definitions/World Text Definition")]
    public class WorldTextDefinition : ScriptableObject
    {
        // 표시 값
        public WorldTextRuleComponent rule = new WorldTextRuleComponent
        {
            lifetimeTicks = 40,
            riseDistance = 100f,
            spreadRadius = 0.2f,
            damageColor = Color.white,
            lootHeightOffset = 1f,
            goldColor = Color.yellow,
            goldFormat = "+{0} G",
            healthColor = Color.green,
            healthFormat = "+{0} HP",
            ammoColor = Color.cyan,
            ammoFormat = "+{0}"
        };
        // WorldTextView가 붙은 프리팹
        public GameObject viewPrefab;
    }
}
