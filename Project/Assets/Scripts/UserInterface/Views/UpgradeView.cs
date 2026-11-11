using System;
using System.Collections.Generic;
using TopView.Commands;
using TopView.Components;
using UnityEngine;
using UnityEngine.UI;

namespace TopView.UserInterface.Views
{
    // 강화 화면. 강화 항목(UpgradeEntry) 프리팹을 배열 순서대로 만들어 담고, 항목의 버튼과 닫기 버튼 입력을 이벤트로 알린다.
    // 각 항목이 무엇을 강화하는지는 프리팹이 정한다. 배열에 없는 항목(예: 수류탄)은 화면에 없고 강화할 수 없다.
    public class UpgradeView : UserInterfaceView
    {
        // 강화 버튼을 눌렀다. 두 번째 값은 Weapon일 때의 무기 번호이며, 다른 항목은 -1이다
        public event Action<PlayerUpgradeCommandType, int> UpgradeClicked;
        // 닫기 버튼을 눌렀다
        public event Action CloseClicked;

        [SerializeField] private Button _closeButton;
        // 항목을 담을 부모. Content 안의 Layout Group이 붙은 오브젝트(또는 Scroll View의 Content)를 연결한다. Content 밖이면 화면을 닫아도 항목이 남아 보인다
        [SerializeField] private RectTransform _entryParent;
        // 화면에 둘 항목 프리팹. 배열 순서가 화면 순서다
        [SerializeField] private UpgradeEntry[] _entryPrefabs;

        private readonly List<UpgradeEntry> _entries = new List<UpgradeEntry>();
        private bool _isBuilt;

        /// <summary>
        /// 항목 프리팹을 배열 순서대로 한 번만 만든다. 판을 준비할 때 Presenter가 부른다.
        /// </summary>
        public void BuildEntries()
        {
            if (_isBuilt)
            {
                return;
            }

            for (int prefabIndex = 0; prefabIndex < _entryPrefabs.Length; prefabIndex++)
            {
                UpgradeEntry prefab = _entryPrefabs[prefabIndex];
                if (prefab == null)
                {
                    Debug.LogError($"UpgradeView >> Entry Prefabs의 {prefabIndex}번 칸이 비어 있어 항목을 만들지 않았다.");
                    continue;
                }

                UpgradeEntry entry = Instantiate(prefab, _entryParent);
                entry.Initialize(this);
                _entries.Add(entry);
            }
            WarnDuplicateEntries();
            _isBuilt = true;
        }

        /// <summary>
        /// Weapon이 아닌 항목의 레벨, 강화 비용, 버튼을 누를 수 있는지, 지금과 다음 레벨의 수치를 보여 준다. 그 항목의 Entry가 없으면 보여 주지 않는다.
        /// </summary>
        /// <param name="type">강화 항목</param>
        /// <param name="level">현재 레벨</param>
        /// <param name="maxLevel">최대 레벨</param>
        /// <param name="cost">다음 레벨로 강화하는 비용</param>
        /// <param name="canUpgrade">버튼을 누를 수 있는지</param>
        /// <param name="value">지금 레벨의 수치</param>
        /// <param name="nextValue">다음 레벨의 수치</param>
        public void SetStat(PlayerUpgradeCommandType type, int level, int maxLevel, int cost, bool canUpgrade, float value, float nextValue)
        {
            for (int entryIndex = 0; entryIndex < _entries.Count; entryIndex++)
            {
                UpgradeEntry entry = _entries[entryIndex];
                if (entry.Type == type)
                {
                    entry.SetStat(level, maxLevel, cost, canUpgrade, value, nextValue);
                }
            }
        }

        /// <summary>
        /// 무기 하나의 레벨, 강화 비용, 버튼을 누를 수 있는지, 지금과 다음 레벨의 무기 수치를 보여 준다. 그 무기의 Entry가 없으면 보여 주지 않는다.
        /// </summary>
        /// <param name="weaponIndex">무기 번호</param>
        /// <param name="level">현재 레벨</param>
        /// <param name="maxLevel">최대 레벨</param>
        /// <param name="cost">다음 레벨로 강화하는 비용</param>
        /// <param name="canUpgrade">버튼을 누를 수 있는지</param>
        /// <param name="effect">지금과 다음 레벨의 무기 수치</param>
        public void SetWeapon(int weaponIndex, int level, int maxLevel, int cost, bool canUpgrade, in WeaponUpgradeEffectComponent effect)
        {
            for (int entryIndex = 0; entryIndex < _entries.Count; entryIndex++)
            {
                UpgradeEntry entry = _entries[entryIndex];
                if (entry.Type == PlayerUpgradeCommandType.Weapon && entry.WeaponIndex == weaponIndex)
                {
                    entry.SetWeapon(level, maxLevel, cost, canUpgrade, in effect);
                }
            }
        }

        /// <summary>
        /// 항목의 버튼 입력을 강화 항목과 무기 번호로 알린다. UpgradeEntry가 부른다.
        /// </summary>
        /// <param name="type">강화 항목</param>
        /// <param name="weaponIndex">Weapon일 때의 무기 번호</param>
        public void ClickEntry(PlayerUpgradeCommandType type, int weaponIndex)
        {
            UpgradeClicked?.Invoke(type, type == PlayerUpgradeCommandType.Weapon ? weaponIndex : -1);
        }

        private void OnEnable()
        {
            _closeButton.onClick.AddListener(HandleCloseButtonClicked);
        }

        private void OnDisable()
        {
            _closeButton.onClick.RemoveListener(HandleCloseButtonClicked);
        }

        private void HandleCloseButtonClicked()
        {
            CloseClicked?.Invoke();
        }

        // 같은 항목(Weapon은 같은 무기 번호)을 강화하는 Entry가 둘 이상이면 프리팹 설정 실수로 보고 한 번 경고한다.
        // 배열에 없는 항목은 강화하지 않는다는 뜻이라 경고하지 않는다
        private void WarnDuplicateEntries()
        {
            for (int entryIndex = 0; entryIndex < _entries.Count; entryIndex++)
            {
                UpgradeEntry entry = _entries[entryIndex];
                for (int otherIndex = entryIndex + 1; otherIndex < _entries.Count; otherIndex++)
                {
                    UpgradeEntry other = _entries[otherIndex];
                    bool isSameWeapon = entry.Type != PlayerUpgradeCommandType.Weapon || entry.WeaponIndex == other.WeaponIndex;
                    if (entry.Type == other.Type && isSameWeapon)
                    {
                        Debug.LogWarning($"UpgradeView >> Entry Prefabs의 {entry.name}과 {other.name}이 같은 항목({entry.Type}, 무기 번호 {entry.WeaponIndex})을 강화한다. 프리팹의 Type과 Weapon Index를 확인한다.");
                    }
                }
            }
        }
    }
}
