using TopView.Components;
using TopView.Definitions;
using TopView.Presentation.Views;
using UnityEngine;

namespace TopView.Core
{
    // 피해량 글자: Definition 읽기
    public partial class Runner
    {
        // DamageTextView가 붙은 프리팹. 없으면 글자를 띄우지 않는다
        private GameObject _damageTextPrefab;
        // 피해량 글자의 표시 값
        private DamageTextRuleComponent _damageTextRule;

        // 피해량 글자는 표시용이라 값이 빠져 있어도 판은 시작하고, 글자 없이 둔다.
        private void ReadDamageText(DamageTextDefinition definition)
        {
            _damageTextPrefab = null;

            if (definition == null)
            {
                Debug.LogWarning("Runner >> Definition Catalog에 Damage Text Definition이 연결되어 있지 않아 피해량 글자 없이 진행한다.");
                return;
            }

            if (definition.viewPrefab == null || definition.viewPrefab.GetComponent<WorldTextView>() == null)
            {
                Debug.LogWarning("Runner >> Damage Text Definition의 View Prefab이 없거나 DamageTextView가 붙어 있지 않아 피해량 글자 없이 진행한다.");
                return;
            }

            if (definition.rule.lifetimeTicks <= 0)
            {
                Debug.LogWarning("Runner >> Damage Text Definition의 Lifetime Ticks가 0 이하라 피해량 글자 없이 진행한다.");
                return;
            }

            _damageTextPrefab = definition.viewPrefab;
            _damageTextRule = definition.rule;
        }
    }
}
