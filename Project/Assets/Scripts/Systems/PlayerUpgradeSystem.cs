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
        /// <summary>
        /// 강화 명령을 모두 소비한다. 강화 화면이 열려 있을 때만 살아 있는 첫 플레이어를 강화한다. 일시정지 중에도 돌도록 PauseSystem 바로 뒤에서 부른다.
        /// </summary>
        /// <param name="players">플레이어 저장소</param>
        /// <param name="progress">한 판의 진행 상태. 골드를 쓴다</param>
        /// <param name="playerUpgradeCommands">플레이어 강화 명령 목록</param>
        public static void Tick(PlayerStorage players, ref GameProgressComponent progress, List<PlayerUpgradeCommand> playerUpgradeCommands)
        {
            // 강화 화면이 닫힌 뒤나 플레이어가 죽은 뒤에 남은 명령은 정상적으로 무시한다
            if (progress.isUpgradeOpen && TryFindAlivePlayer(players, out int playerIndex))
            {
                ref PlayerEntity entity = ref players.entities[playerIndex];
                for (int commandIndex = 0; commandIndex < playerUpgradeCommands.Count; commandIndex++)
                {
                    Apply(ref entity.upgradeLevels, ref entity.health, ref entity.mana, ref entity.weaponSlots, ref progress, in entity.vitals, in entity.upgrade, entity.weaponList.weapons, playerUpgradeCommands[commandIndex]);
                }
            }

            playerUpgradeCommands.Clear();
        }

        private static bool TryFindAlivePlayer(PlayerStorage players, out int playerIndex)
        {
            for (playerIndex = 0; playerIndex < players.entities.Length; playerIndex++)
            {
                if (players.entities[playerIndex].isAlive)
                {
                    return true;
                }
            }

            playerIndex = -1;
            return false;
        }

        // 버튼은 최대 레벨이거나 골드가 모자라면 눌리지 않으므로, 여기서 막히는 명령은 경고하고 버린다
        private static void Apply(ref PlayerUpgradeLevelsComponent levels, ref HealthComponent health, ref ManaComponent mana, ref PlayerWeaponSlotsComponent weaponSlots, ref GameProgressComponent progress, in PlayerVitalsComponent vitals, in PlayerUpgradeComponent upgrade, WeaponComponent[] weapons, in PlayerUpgradeCommand command)
        {
            if (!TryGetLevel(in levels, in command, out int level))
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
                    levels.weaponLevels[command.weaponIndex] = nextLevel;
                    FillAddedAmmo(ref weaponSlots, in weapons[command.weaponIndex], command.weaponIndex, level, nextLevel);
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
        private static void FillAddedAmmo(ref PlayerWeaponSlotsComponent weaponSlots, in WeaponComponent weapon, int weaponIndex, int previousLevel, int nextLevel)
        {
            weaponSlots.currentAmmo[weaponIndex] += PlayerUpgradeUtility.GetMaxAmmo(in weapon, nextLevel) - PlayerUpgradeUtility.GetMaxAmmo(in weapon, previousLevel);
        }

        // 항목의 지금 레벨을 꺼낸다. Weapon의 무기 번호가 레벨 배열 밖이거나 처리하지 않는 항목이면 false를 돌려준다
        private static bool TryGetLevel(in PlayerUpgradeLevelsComponent levels, in PlayerUpgradeCommand command, out int level)
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
                    int[] damageLevels = levels.weaponLevels;
                    if (damageLevels == null || command.weaponIndex < 0 || command.weaponIndex >= damageLevels.Length)
                    {
                        level = 0;
                        return false;
                    }
                    level = damageLevels[command.weaponIndex];
                    return true;
                default:
                    level = 0;
                    return false;
            }
        }
    }
}
