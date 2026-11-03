using TopView.Components;
using UnityEngine;

namespace TopView.Definitions
{
    // 적이 받은 피해량 글자의 프리팹과 표시 값
    [CreateAssetMenu(fileName = "World Text Definition", menuName = "Definitions/World Text Definition")]
    public class WorldTextDefinition : ScriptableObject
    {
        // 표시 값
        public WorldTextRuleComponent rule = new WorldTextRuleComponent { lifetimeTicks = 40, riseDistance = 100f, spreadRadius = 0.2f };
        // DamageTextView가 붙은 프리팹
        public GameObject viewPrefab;
    }
}
