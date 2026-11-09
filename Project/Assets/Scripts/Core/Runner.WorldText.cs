using TopView.Presentation.Views;
using UnityEngine;

namespace TopView.Core
{
    // 월드 글자: 프리팹 읽기
    public partial class Runner
    {
        // WorldTextView가 붙은 월드 글자 프리팹
        [SerializeField] private GameObject _worldTextPrefab;

        // 월드 글자 프리팹. 없으면 글자를 띄우지 않는다
        private WorldTextView _worldTextViewPrefab;

        // 월드 글자는 표시용이라 값이 빠져 있어도 판은 시작하고, 글자 없이 둔다.
        private void ReadWorldText(GameObject prefab)
        {
            _worldTextViewPrefab = null;

            if (prefab == null)
            {
                Debug.LogWarning("Runner >> Runner에 World Text Prefab이 연결되어 있지 않아 월드 글자 없이 진행한다.");
                return;
            }

            WorldTextView view = prefab.GetComponent<WorldTextView>();
            if (view == null)
            {
                Debug.LogWarning("Runner >> World Text Prefab에 WorldTextView가 붙어 있지 않아 월드 글자 없이 진행한다.");
                return;
            }

            if (view.lifetimeTicks <= 0)
            {
                Debug.LogWarning("Runner >> World Text View의 Lifetime Ticks가 0 이하라 월드 글자 없이 진행한다.");
                return;
            }

            if (string.IsNullOrEmpty(view.goldFormat) || string.IsNullOrEmpty(view.healthFormat) || string.IsNullOrEmpty(view.ammoFormat))
            {
                Debug.LogWarning("Runner >> World Text View의 전리품 글자 형식이 비어 있어 월드 글자 없이 진행한다.");
                return;
            }

            _worldTextViewPrefab = view;
        }
    }
}
