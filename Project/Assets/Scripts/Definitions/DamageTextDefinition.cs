using TopView.Components;
using UnityEngine;

namespace TopView.Definitions
{
    // 적이 받은 피해량 글자의 프리팹과 표시 값
    [CreateAssetMenu(fileName = "Damage Text Definition", menuName = "Definitions/Damage Text Definition")]
    public class DamageTextDefinition : ScriptableObject
    {
        // 표시 값
        public DamageTextRuleComponent rule = new DamageTextRuleComponent { lifetimeTicks = 40, riseDistance = 100f, spreadRadius = 0.2f };
        // DamageTextView가 붙은 프리팹
        public GameObject viewPrefab;
    }
}
