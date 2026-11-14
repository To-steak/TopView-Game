using System.Collections;
using NUnit.Framework;
using TMPro;
using TopView.UserInterface.Presenters;
using TopView.UserInterface.Views;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace TopView.Tests.PlayMode
{
    // 마나가 줄었다가 회복으로 돌아설 때 게이지가 이어지게 움직이는지 확인한다. 게이지 트윈이 흘러야 해서 실제 시간을 기다린다.
    public class PlayerManaPresenterTests
    {
        private PlayerManaPresenter _presenter;
        private RectMask2D _mask;

        private const float MASK_WIDTH = 100f;
        private const float MAX_MANA = 100f;
        // View의 게이지 트윈 시간(틱). PlayerManaView의 기본값이다
        private const int TWEEN_TICKS = 15;
        // 마나가 준 뒤 회복이 시작되기까지의 틱. 게이지 트윈보다 짧은 회복 지연이다
        private const int SHORT_RECOVERY_DELAY_TICKS = 5;
        // 한 번의 회복으로 게이지가 움직여도 되는 비율. 한 틱 회복량(1)보다 넉넉히 둔다
        private const float MAX_RATIO_STEP = 0.05f;

        /// <summary>
        /// 마나 View를 붙인 Presenter를 만들고 꽉 찬 마나를 바로 보여 준다.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            PlayerManaView view = new GameObject("Test Player Mana View", typeof(RectTransform)).AddComponent<PlayerManaView>();
            view.content = new GameObject("Content", typeof(RectTransform)).GetComponent<RectTransform>();
            view.content.SetParent(view.transform, false);
            TMP_Text manaText = new GameObject("Mana Text", typeof(RectTransform)).AddComponent<TextMeshProUGUI>();
            manaText.transform.SetParent(view.content, false);
            _mask = new GameObject("Mana Mask", typeof(RectTransform)).AddComponent<RectMask2D>();
            _mask.transform.SetParent(view.content, false);
            _mask.rectTransform.sizeDelta = new Vector2(MASK_WIDTH, 10f);
            PrivateFieldAccess.Write(view, "_manaText", manaText);
            PrivateFieldAccess.Write(view, "_manaMask", _mask);
            PrivateFieldAccess.Write(view, "_tweenTicks", TWEEN_TICKS);

            _presenter = view.gameObject.AddComponent<PlayerManaPresenter>();
            PrivateFieldAccess.Write(_presenter, "_view", view);
            _presenter.Initialize();
            _presenter.Present(true, MAX_MANA, MAX_MANA);
        }

        /// <summary>
        /// 만든 View와 Presenter를 지운다.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            Object.Destroy(_presenter.gameObject);
        }

        /// <summary>
        /// 줄어드는 게이지가 다 내려가기 전에 회복이 시작되어도, 게이지가 회복 첫 틱에 한 번에 건너뛰지 않는다.
        /// </summary>
        /// <returns>테스트 코루틴</returns>
        [UnityTest]
        public IEnumerator GaugeDoesNotJumpWhenRecoveryStartsDuringDecrease()
        {
            _presenter.Present(true, 70f, MAX_MANA);
            yield return new WaitForSeconds(SHORT_RECOVERY_DELAY_TICKS * Time.fixedDeltaTime);
            float ratioBeforeRecovery = GetDisplayedRatio();
            Assert.Greater(ratioBeforeRecovery, 0.7f + MAX_RATIO_STEP, "회복 전에 게이지가 아직 내려가는 중이어야 이 테스트가 뜻이 있다.");

            _presenter.Present(true, 71f, MAX_MANA);
            float ratioAfterRecovery = GetDisplayedRatio();

            Assert.LessOrEqual(Mathf.Abs(ratioAfterRecovery - ratioBeforeRecovery), MAX_RATIO_STEP, $"회복 첫 틱에 게이지가 {ratioBeforeRecovery:F2}에서 {ratioAfterRecovery:F2}로 한 번에 건너뛰었다.");
        }

        /// <summary>
        /// 줄어드는 게이지가 다 내려간 뒤에 회복이 시작되면 게이지는 회복량만큼만 움직인다.
        /// </summary>
        /// <returns>테스트 코루틴</returns>
        [UnityTest]
        public IEnumerator GaugeMovesByRecoveryAfterDecreaseEnds()
        {
            _presenter.Present(true, 70f, MAX_MANA);
            yield return new WaitForSeconds((TWEEN_TICKS + 5) * Time.fixedDeltaTime);
            float ratioBeforeRecovery = GetDisplayedRatio();

            _presenter.Present(true, 71f, MAX_MANA);
            float ratioAfterRecovery = GetDisplayedRatio();

            Assert.LessOrEqual(Mathf.Abs(ratioAfterRecovery - ratioBeforeRecovery), MAX_RATIO_STEP);
        }

        // 마스크가 가린 오른쪽 여백에서 지금 보이는 게이지 비율을 구한다.
        private float GetDisplayedRatio()
        {
            return 1f - _mask.padding.z / _mask.rectTransform.rect.width;
        }
    }
}
