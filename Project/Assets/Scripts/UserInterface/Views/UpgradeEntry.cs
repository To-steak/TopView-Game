using TMPro;
using TopView.Commands;
using UnityEngine;
using UnityEngine.UI;

namespace TopView.UserInterface.Views
{
    // 강화 항목 하나. 항목마다 프리팹을 따로 두고, 무엇을 강화하는지와 아이콘(Image)을 프리팹에 정해 둔다.
    // UpgradeView가 프리팹에서 만들어 소유하며, 현재 레벨·강화 비용을 보여 주고 버튼 입력을 UpgradeView에 알린다.
    public class UpgradeEntry : MonoBehaviour
    {
        // 강화 항목
        [SerializeField] private PlayerUpgradeCommandType _type;
        // Weapon일 때 강화할 무기 번호. Player Definition의 무기 목록 번호(교체 키 - 1)다. 다른 항목은 쓰지 않는다
        [SerializeField] private int _weaponIndex;
        [SerializeField] private TMP_Text _text;
        [SerializeField] private Button _button;

        private UpgradeView _owner;
        private bool _hasPresented;
        private int _presentedLevel;
        private int _presentedMaxLevel;
        private int _presentedCost;
        private bool _presentedCanUpgrade;

        private const string MAX_LEVEL_COST_TEXT = "MAX";

        /// <summary>
        /// 강화 항목.
        /// </summary>
        public PlayerUpgradeCommandType Type => _type;

        /// <summary>
        /// Weapon일 때 강화할 무기 번호.
        /// </summary>
        public int WeaponIndex => _weaponIndex;

        /// <summary>
        /// 소유한 View를 정한다. UpgradeView가 항목을 만들 때 부른다.
        /// </summary>
        /// <param name="owner">이 항목을 소유한 View</param>
        public void Initialize(UpgradeView owner)
        {
            _owner = owner;
            _hasPresented = false;
        }

        /// <summary>
        /// 값이 바뀌었을 때만 현재 레벨과 강화 비용, 버튼을 누를 수 있는지를 보여 준다. 최대 레벨이면 비용 대신 MAX를 보여 준다.
        /// </summary>
        /// <param name="level">현재 레벨</param>
        /// <param name="maxLevel">최대 레벨</param>
        /// <param name="cost">다음 레벨로 강화하는 비용</param>
        /// <param name="canUpgrade">버튼을 누를 수 있는지</param>
        public void Set(int level, int maxLevel, int cost, bool canUpgrade)
        {
            if (_hasPresented && level == _presentedLevel && maxLevel == _presentedMaxLevel && cost == _presentedCost && canUpgrade == _presentedCanUpgrade)
            {
                return;
            }

            string costText = level >= maxLevel ? MAX_LEVEL_COST_TEXT : $"{cost:N0} G";
            _text.text = $"Lv {level}/{maxLevel}\n{costText}";
            _button.interactable = canUpgrade;
            _presentedLevel = level;
            _presentedMaxLevel = maxLevel;
            _presentedCost = cost;
            _presentedCanUpgrade = canUpgrade;
            _hasPresented = true;
        }

        private void OnEnable()
        {
            _button.onClick.AddListener(HandleButtonClicked);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(HandleButtonClicked);
        }

        private void HandleButtonClicked()
        {
            if (_owner == null)
            {
                Debug.LogWarning($"UpgradeEntry >> 소유한 UpgradeView가 정해지지 않아 {_type} 항목의 버튼 입력을 버렸다.");
                return;
            }

            _owner.ClickEntry(_type, _weaponIndex);
        }
    }
}
