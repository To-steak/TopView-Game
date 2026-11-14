using System.Collections.Generic;
using TopView.Commands;
using TopView.Components;
using TopView.Entities;
using TopView.Utilities;
using UnityEngine;

namespace TopView.Systems
{
    // 강화 명령을 받아 골드를 쓰고 플레이어의 강화 레벨을 올린다.
    public static class PlayerUpgradeSystem
    {
        // 탄창이 있어 강화로 늘어난 탄약을 바로 넣는 무기가 가진 컴포넌트
        private const ComponentMask MAGAZINE_MASK = ComponentMask.Magazine | ComponentMask.MagazineState | ComponentMask.WeaponUpgrade;

        /// <summary>
        /// 강화 명령을 모두 소비한다. 강화 화면이 열려 있을 때만 살아 있는 첫 플레이어를 강화한다. 일시정지 중에도 돌도록 PauseSystem 바로 뒤에서 부른다.
        /// </summary>
        /// <param name="storage">엔티티 저장소. 살아 있는(Die가 아닌) 첫 플레이어(PlayerActionState)의 health, mana, playerUpgradeLevels를 고치고 playerVitals, playerUpgrade, weaponSlots를 읽는다. 무기 강화면 그 무기의 upgradeLevel과 탄창이 있는 무기(Magazine, MagazineState, WeaponUpgrade)의 magazineState를 고친다</param>
        /// <param name="progress">한 판의 진행 상태. 골드를 쓴다</param>
        /// <param name="playerUpgradeCommands">플레이어 강화 명령 목록</param>
        public static void Tick(EntityStorage storage, ref GameProgressComponent progress, List<PlayerUpgradeCommand> playerUpgradeCommands)
        {
            // 강화 화면이 닫힌 뒤나 플레이어가 죽은 뒤에 남은 명령은 정상적으로 무시한다
            if (progress.isUpgradeOpen && TryFindAlivePlayer(storage, out int playerIndex))
            {
                ref Entity entity = ref storage.entities[playerIndex];
                for (int commandIndex = 0; commandIndex < playerUpgradeCommands.Count; commandIndex++)
                {
                    Apply(ref entity.playerUpgradeLevels, ref entity.health, ref entity.mana, in entity.weaponSlots, ref progress, in entity.playerVitals, in entity.playerUpgrade, storage.entities, playerUpgradeCommands[commandIndex]);
                }
            }

            playerUpgradeCommands.Clear();
        }

        // 사망한(Die) 플레이어는 찾지 않는다. 최대 체력 강화가 체력 0인 플레이어의 체력을 다시 채우지 않게 하기 위해서다
        private static bool TryFindAlivePlayer(EntityStorage storage, out int playerIndex)
        {
            for (playerIndex = 0; playerIndex < storage.entities.Length; playerIndex++)
            {
                ref readonly Entity entity = ref storage.entities[playerIndex];
                if ((entity.mask & ComponentMask.PlayerActionState) != 0 && entity.playerActionState.state != PlayerState.Die)
                {
                    return true;
                }
            }

            playerIndex = -1;
            return false;
        }

        // 버튼은 최대 레벨이거나 골드가 모자라면 눌리지 않으므로, 여기서 막히는 명령은 경고하고 버린다
        private static void Apply(ref PlayerUpgradeLevelsComponent levels, ref HealthComponent health, ref ManaComponent mana, in PlayerWeaponSlotsComponent weaponSlots, ref GameProgressComponent progress, in PlayerVitalsComponent vitals, in PlayerUpgradeComponent upgrade, Entity[] weaponEntities, in PlayerUpgradeCommand command)
        {
            if (!TryGetLevel(in levels, in weaponSlots, weaponEntities, in command, out int level))
            {
                Debug.LogWarning($"PlayerUpgradeSystem >> 처리하지 않는 항목이거나 무기 번호 {command.weaponIndex}가 무기 목록 밖이라 {command.type} 강화 명령을 버렸다.");
                return;
            }
            if (level >= upgrade.maxLevel)
            {
                Debug.LogWarning($"PlayerUpgradeSystem >> {command.type} 레벨이 이미 최대({upgrade.maxLevel})라 강화 명령을 버렸다.");
                return;
            }
            int cost = PlayerUpgradeUtility.GetCost(in upgrade, level);
            if (progress.gold < cost)
            {
                Debug.LogWarning($"PlayerUpgradeSystem >> 골드({progress.gold})가 비용({cost})보다 적어 {command.type} 강화 명령을 버렸다.");
                return;
            }

            progress.gold -= cost;
            int nextLevel = level + 1;
            switch (command.type)
            {
                case PlayerUpgradeCommandType.MaxHealth:
                    levels.maxHealthLevel = nextLevel;
                    RaiseMaxHealth(ref health, in vitals, in upgrade, nextLevel);
                    break;
                case PlayerUpgradeCommandType.MaxMana:
                    levels.maxManaLevel = nextLevel;
                    RaiseMaxMana(ref mana, in vitals, in upgrade, nextLevel);
                    break;
                case PlayerUpgradeCommandType.RunManaCost:
                    levels.runManaCostLevel = nextLevel;
                    break;
                case PlayerUpgradeCommandType.DodgeManaCost:
                    levels.dodgeManaCostLevel = nextLevel;
                    break;
                case PlayerUpgradeCommandType.JumpManaCost:
                    levels.jumpManaCostLevel = nextLevel;
                    break;
                case PlayerUpgradeCommandType.Weapon:
                    ref Entity weaponEntity = ref weaponEntities[weaponSlots.weapons[command.weaponIndex].index];
                    weaponEntity.upgradeLevel.level = nextLevel;
                    if ((weaponEntity.mask & MAGAZINE_MASK) == MAGAZINE_MASK)
                    {
                        FillAddedAmmo(ref weaponEntity.magazineState, in weaponEntity.magazine, in weaponEntity.weaponUpgrade, level, nextLevel);
                    }
                    break;
            }
        }

        // 최대 체력을 새 레벨 값으로 바꾸고, 늘어난 만큼 체력도 채운다
        private static void RaiseMaxHealth(ref HealthComponent health, in PlayerVitalsComponent vitals, in PlayerUpgradeComponent upgrade, int level)
        {
            float maxHealth = PlayerUpgradeUtility.Increase(vitals.startingMaxHealth, upgrade.maxHealthRatioPerLevel, level);
            health.current += maxHealth - health.max;
            health.max = maxHealth;
        }

        // 최대 마나를 새 레벨 값으로 바꾸고, 늘어난 만큼 마나도 채운다
        private static void RaiseMaxMana(ref ManaComponent mana, in PlayerVitalsComponent vitals, in PlayerUpgradeComponent upgrade, int level)
        {
            float maxMana = PlayerUpgradeUtility.Increase(vitals.startingMaxMana, upgrade.maxManaRatioPerLevel, level);
            mana.current += maxMana - mana.max;
            mana.max = maxMana;
        }

        // 무기 레벨이 올라 최대 탄약 수가 늘어난 만큼 탄창에도 바로 넣는다
        private static void FillAddedAmmo(ref MagazineStateComponent magazineState, in MagazineComponent magazine, in WeaponUpgradeRuleComponent weaponUpgrade, int previousLevel, int nextLevel)
        {
            magazineState.current += PlayerUpgradeUtility.GetMaxAmmo(in magazine, in weaponUpgrade, nextLevel) - PlayerUpgradeUtility.GetMaxAmmo(in magazine, in weaponUpgrade, previousLevel);
        }

        // 항목의 지금 레벨을 꺼낸다. Weapon은 그 무기 엔티티의 레벨이다. Weapon의 무기 번호가 무기 칸 밖이거나 처리하지 않는 항목이면 false를 돌려준다
        private static bool TryGetLevel(in PlayerUpgradeLevelsComponent levels, in PlayerWeaponSlotsComponent weaponSlots, Entity[] weaponEntities, in PlayerUpgradeCommand command, out int level)
        {
            switch (command.type)
            {
                case PlayerUpgradeCommandType.MaxHealth:
                    level = levels.maxHealthLevel;
                    return true;
                case PlayerUpgradeCommandType.MaxMana:
                    level = levels.maxManaLevel;
                    return true;
                case PlayerUpgradeCommandType.RunManaCost:
                    level = levels.runManaCostLevel;
                    return true;
                case PlayerUpgradeCommandType.DodgeManaCost:
                    level = levels.dodgeManaCostLevel;
                    return true;
                case PlayerUpgradeCommandType.JumpManaCost:
                    level = levels.jumpManaCostLevel;
                    return true;
                case PlayerUpgradeCommandType.Weapon:
                    EntityIdentifier[] weaponIdentifiers = weaponSlots.weapons;
                    if (weaponIdentifiers == null || command.weaponIndex < 0 || command.weaponIndex >= weaponIdentifiers.Length)
                    {
                        level = 0;
                        return false;
                    }
                    level = weaponEntities[weaponIdentifiers[command.weaponIndex].index].upgradeLevel.level;
                    return true;
                default:
                    level = 0;
                    return false;
            }
        }
    }
}
