using TopView.Components;
using UnityEngine;

namespace TopView.Definitions
{
    // 전리품의 규칙과 종류별 프리팹
    [CreateAssetMenu(fileName = "Loot Definition", menuName = "Definitions/Loot Definition")]
    public class LootDefinition : ScriptableObject
    {
        // 전리품 규칙 값
        public LootRuleComponent rule;
        // LootView가 붙은 골드 프리팹
        public GameObject goldViewPrefab;
        // LootView가 붙은 회복 아이템 프리팹
        public GameObject healthViewPrefab;
        // LootView가 붙은 총알 프리팹
        public GameObject ammoViewPrefab;
    }
}
