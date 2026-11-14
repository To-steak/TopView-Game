using System.Text.RegularExpressions;
using NUnit.Framework;
using TMPro;
using TopView.UserInterface.Views;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace TopView.PlayModeTests
{
    // 플레이어 마나 HUD가 받은 마나로 게이지 마스크를 맞게 맞추는지 확인한다.
    public class PlayerManaViewTests
    {
        private PlayerManaView _view;
        private RectMask2D _mask;

        private const float MASK_WIDTH = 100f;

        /// <summary>
        /// 글자와 마스크만 붙인 마나 View를 만든다.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            _view = new GameObject("Test Player Mana View", typeof(RectTransform)).AddComponent<PlayerManaView>();
            TMP_Text manaText = new GameObject("Mana Text", typeof(RectTransform)).AddComponent<TextMeshProUGUI>();
            manaText.transform.SetParent(_view.transform, false);
            _mask = new GameObject("Mana Mask", typeof(RectTransform)).AddComponent<RectMask2D>();
            _mask.transform.SetParent(_view.transform, false);
            _mask.rectTransform.sizeDelta = new Vector2(MASK_WIDTH, 10f);
            SerializedFieldWriter.Write(_view, "_manaText", manaText);
            SerializedFieldWriter.Write(_view, "_manaMask", _mask);
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
        /// 최대 마나가 0이어도 마스크 여백에 NaN이 들어가지 않는다. 잘못된 값이므로 경고를 남긴다.
        /// </summary>
        [Test]
        public void ZeroMaxManaDoesNotPutNaNIntoMask()
        {
            _view.SetMana(0f, 0f, true);

            LogAssert.Expect(LogType.Warning, new Regex("최대 마나가 0"));

            Assert.IsFalse(float.IsNaN(_mask.padding.z), "최대 마나가 0이라 마스크 오른쪽 여백이 NaN이 되었다.");
        }

        /// <summary>
        /// 마나가 절반이면 마스크가 게이지의 오른쪽 절반을 가린다.
        /// </summary>
        [Test]
        public void HalfManaMasksHalfGauge()
        {
            _view.SetMana(50f, 100f, true);

            Assert.AreEqual(MASK_WIDTH * 0.5f, _mask.padding.z, 0.001f);
        }
    }
}
