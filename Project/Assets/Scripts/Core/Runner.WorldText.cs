using TopView.Components;
using TopView.Definitions;
using TopView.Presentation.Views;
using UnityEngine;

namespace TopView.Core
{
    // 월드 글자: Definition 읽기
    public partial class Runner
    {
        // WorldTextView가 붙은 프리팹. 없으면 글자를 띄우지 않는다
        private GameObject _worldTextPrefab;
        // 월드 글자의 표시 값
        private WorldTextRuleComponent _worldTextRule;

        // 월드 글자는 표시용이라 값이 빠져 있어도 판은 시작하고, 글자 없이 둔다.
        private void ReadWorldText(WorldTextDefinition definition)
        {
            _worldTextPrefab = null;

            if (definition == null)
            {
                Debug.LogWarning("Runner >> Definition Catalog에 World Text Definition이 연결되어 있지 않아 월드 글자 없이 진행한다.");
                return;
            }

            if (definition.viewPrefab == null || definition.viewPrefab.GetComponent<WorldTextView>() == null)
            {
                Debug.LogWarning("Runner >> World Text Definition의 View Prefab이 없거나 WorldTextView가 붙어 있지 않아 월드 글자 없이 진행한다.");
                return;
            }

            if (definition.rule.lifetimeTicks <= 0)
            {
                Debug.LogWarning("Runner >> World Text Definition의 Lifetime Ticks가 0 이하라 월드 글자 없이 진행한다.");
                return;
            }

            if (string.IsNullOrEmpty(definition.rule.goldFormat) || string.IsNullOrEmpty(definition.rule.healthFormat) || string.IsNullOrEmpty(definition.rule.ammoFormat))
            {
                Debug.LogWarning("Runner >> World Text Definition의 전리품 글자 형식이 비어 있어 월드 글자 없이 진행한다.");
                return;
            }

            _worldTextPrefab = definition.viewPrefab;
            _worldTextRule = definition.rule;
        }
    }
}
