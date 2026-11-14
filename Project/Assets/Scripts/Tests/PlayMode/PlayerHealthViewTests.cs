using NUnit.Framework;
using TMPro;
using TopView.UserInterface.Views;
using UnityEngine;
using UnityEngine.UI;

namespace TopView.Tests.PlayMode
{
    // 플레이어 체력 HUD가 받은 체력을 글자로 맞게 보여 주는지 확인한다.
    public class PlayerHealthViewTests
    {
        private PlayerHealthView _view;
        private TMP_Text _healthText;

        private const float MAX_HEALTH = 145f;

        /// <summary>
        /// 글자와 마스크만 붙인 체력 View를 만든다.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            _view = new GameObject("Test Player Health View", typeof(RectTransform)).AddComponent<PlayerHealthView>();
            _healthText = new GameObject("Health Text", typeof(RectTransform)).AddComponent<TextMeshProUGUI>();
            _healthText.transform.SetParent(_view.transform, false);
            RectMask2D mask = new GameObject("Health Mask", typeof(RectTransform)).AddComponent<RectMask2D>();
            mask.transform.SetParent(_view.transform, false);
            mask.rectTransform.sizeDelta = new Vector2(100f, 10f);
            SerializedFieldWriter.Write(_view, "_healthText", _healthText);
            SerializedFieldWriter.Write(_view, "_healthMask", mask);
        }

        /// <summary>
        /// 만든 View를 지운다.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            Object.Destroy(_view.gameObject);
        }

        /// <summary>
        /// 체력이 0보다 크면 살아 있으므로, 1 미만으로 남아도 글자가 0으로 보이지 않는다. 사망 판정은 체력 0 이하다.
        /// </summary>
        [Test]
        public void AliveHealthBelowOneIsNotShownAsZero()
        {
            _view.SetHealth(0.4f, MAX_HEALTH, true);

            Assert.AreNotEqual($"0 / {MAX_HEALTH:F0}", _healthText.text, "체력 0.4가 남아 살아 있는 플레이어의 체력이 0으로 보인다.");
        }

        /// <summary>
        /// 체력이 0이면 0으로 보인다.
        /// </summary>
        [Test]
        public void ZeroHealthIsShownAsZero()
        {
            _view.SetHealth(0f, MAX_HEALTH, true);

            Assert.AreEqual($"0 / {MAX_HEALTH:F0}", _healthText.text);
        }
    }
}
