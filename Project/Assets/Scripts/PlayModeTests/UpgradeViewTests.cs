using NUnit.Framework;
using TMPro;
using TopView.Commands;
using TopView.Components;
using TopView.UserInterface.Views;
using UnityEngine;
using UnityEngine.UI;

namespace TopView.PlayModeTests
{
    // 강화 화면의 항목이 받은 값대로 버튼을 누를 수 있게 하는지 확인한다.
    public class UpgradeViewTests
    {
        private GameObject _prefabHolder;
        private UpgradeView _view;
        private RectTransform _entryParent;

        // 플레이어가 가진 무기 수. 프리팹에는 이보다 큰 번호의 무기 항목이 하나 남아 있다
        private const int WEAPON_COUNT = 2;
        private const int STALE_WEAPON_INDEX = 2;

        /// <summary>
        /// 무기 0번과 2번 항목 프리팹을 가진 강화 화면을 만들고 항목을 만든다.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            // 프리팹처럼 쓸 항목이 장면에서 스스로 돌지 않도록 꺼 둔 부모 아래에 둔다
            _prefabHolder = new GameObject("Test Entry Prefabs");
            _prefabHolder.SetActive(false);
            UpgradeEntry[] entryPrefabs = new UpgradeEntry[] { CreateWeaponEntryPrefab(0), CreateWeaponEntryPrefab(STALE_WEAPON_INDEX) };

            GameObject root = new GameObject("Test Upgrade View", typeof(RectTransform));
            root.SetActive(false);
            _view = root.AddComponent<UpgradeView>();
            Button closeButton = new GameObject("Close Button", typeof(RectTransform)).AddComponent<Button>();
            closeButton.transform.SetParent(root.transform, false);
            _entryParent = new GameObject("Entry Parent", typeof(RectTransform)).GetComponent<RectTransform>();
            _entryParent.SetParent(root.transform, false);
            SerializedFieldWriter.Write(_view, "_closeButton", closeButton);
            SerializedFieldWriter.Write(_view, "_entryParent", _entryParent);
            SerializedFieldWriter.Write(_view, "_entryPrefabs", entryPrefabs);
            root.SetActive(true);
            _view.BuildEntries();
        }

        /// <summary>
        /// 만든 화면과 프리팹을 지운다.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            Object.Destroy(_view.gameObject);
            Object.Destroy(_prefabHolder);
        }

        /// <summary>
        /// World가 값을 넘기지 않은 항목(플레이어에게 없는 무기)은 버튼을 누를 수 없다.
        /// </summary>
        [Test]
        public void EntryWithoutValuesCannotBeClicked()
        {
            PresentAllWeapons();

            Assert.IsFalse(FindEntry(STALE_WEAPON_INDEX).GetComponentInChildren<Button>().interactable, $"플레이어에게 없는 무기 {STALE_WEAPON_INDEX}번 항목의 버튼을 누를 수 있다.");
        }

        /// <summary>
        /// 값을 받은 항목은 받은 대로 버튼을 누를 수 있다.
        /// </summary>
        [Test]
        public void EntryWithValuesFollowsCanUpgrade()
        {
            PresentAllWeapons();

            Assert.IsTrue(FindEntry(0).GetComponentInChildren<Button>().interactable);
        }

        // World처럼 플레이어가 가진 무기마다 강화할 수 있는 값을 넘긴다.
        private void PresentAllWeapons()
        {
            WeaponUpgradeEffectComponent effect = new WeaponUpgradeEffectComponent { currentDamage = 10f, nextDamage = 12f };
            for (int weaponIndex = 0; weaponIndex < WEAPON_COUNT; weaponIndex++)
            {
                _view.SetWeapon(weaponIndex, 0, 3, 100, true, in effect);
            }
        }

        private UpgradeEntry FindEntry(int weaponIndex)
        {
            UpgradeEntry[] entries = _entryParent.GetComponentsInChildren<UpgradeEntry>(true);
            for (int entryIndex = 0; entryIndex < entries.Length; entryIndex++)
            {
                if (entries[entryIndex].WeaponIndex == weaponIndex)
                {
                    return entries[entryIndex];
                }
            }

            Assert.Fail($"무기 {weaponIndex}번 항목이 만들어지지 않았다.");
            return null;
        }

        private UpgradeEntry CreateWeaponEntryPrefab(int weaponIndex)
        {
            GameObject prefab = new GameObject($"Test Weapon {weaponIndex} Entry", typeof(RectTransform));
            prefab.transform.SetParent(_prefabHolder.transform, false);
            UpgradeEntry entry = prefab.AddComponent<UpgradeEntry>();
            SerializedFieldWriter.Write(entry, "_type", PlayerUpgradeCommandType.Weapon);
            SerializedFieldWriter.Write(entry, "_weaponIndex", weaponIndex);
            SerializedFieldWriter.Write(entry, "_levelText", CreateText(prefab.transform, "Level Text"));
            SerializedFieldWriter.Write(entry, "_costText", CreateText(prefab.transform, "Cost Text"));
            SerializedFieldWriter.Write(entry, "_descriptionText", CreateText(prefab.transform, "Description Text"));
            Button button = new GameObject("Button", typeof(RectTransform)).AddComponent<Button>();
            button.transform.SetParent(prefab.transform, false);
            SerializedFieldWriter.Write(entry, "_button", button);
            return entry;
        }

        private static TMP_Text CreateText(Transform parent, string name)
        {
            TMP_Text text = new GameObject(name, typeof(RectTransform)).AddComponent<TextMeshProUGUI>();
            text.transform.SetParent(parent, false);
            return text;
        }
    }
}
