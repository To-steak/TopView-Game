using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Records;
using TopView.Requests;
using TopView.Utilities;

namespace TopView.Systems
{
    // 주인이 공격 상태일 때 근접 공격 무기의 판정 시점이 되면 근접 판정을 내보낸다.
    public static class PlayerMeleeAttackSystem
    {
        // 처리할 무기가 가진 컴포넌트
        private const ComponentMask WEAPON_MASK = ComponentMask.Owner | ComponentMask.Weapon | ComponentMask.UpgradeLevel | ComponentMask.WeaponUpgrade
            | ComponentMask.MeleeAttack | ComponentMask.MeleeView;
        // 공격을 내보내는 주인이 가진 컴포넌트
        private const ComponentMask OWNER_MASK = ComponentMask.PlayerActionState | ComponentMask.WeaponSlots | ComponentMask.PlayerUpgrade | ComponentMask.Position | ComponentMask.Rotation;

        /// <summary>
        /// 근접 공격 무기를 든 주인이 공격 상태이고 판정 시점이 지났으면 근접 판정을 내보낸다. 판정 범위와 앞쪽으로 띄우는 거리는 근접 범위 강화를 반영한다. PlayerMoveInputSystem 다음에 부른다.
        /// </summary>
        /// <param name="storage">엔티티 저장소. 무기(Owner, Weapon, UpgradeLevel, WeaponUpgrade, MeleeAttack, MeleeView)를 읽고, 주인(PlayerActionState, WeaponSlots, PlayerUpgrade, Position, Rotation)의 공격 진행을 고친다</param>
        /// <param name="meleeSpawnRequests">근접 판정 생성 요청 큐</param>
        /// <param name="attackCommittedRecords">공격을 내보낸 기록 목록</param>
        public static void Tick(EntityStorage storage, Queue<MeleeSpawnRequest> meleeSpawnRequests, List<PlayerAttackCommittedRecord> attackCommittedRecords)
        {
            for (int index = 0; index < storage.entities.Length; index++)
            {
                ref readonly Entity weapon = ref storage.entities[index];
                if ((weapon.mask & WEAPON_MASK) != WEAPON_MASK)
                {
                    continue;
                }

                ref Entity owner = ref storage.entities[weapon.owner.owner.index];
                EntityIdentifier weaponIdentifier = new EntityIdentifier { index = index, generation = weapon.generation };
                if ((owner.mask & OWNER_MASK) != OWNER_MASK || !PlayerWeaponUtility.IsAttackingWith(in owner, in weapon.owner, in weaponIdentifier))
                {
                    continue;
                }

                ref PlayerActionStateComponent actionState = ref owner.playerActionState;
                if (!AttackUtility.IsShotDue(in weapon.meleeAttack, actionState.committedShotCount, actionState.elapsedTicks))
                {
                    continue;
                }

                int weaponLevel = weapon.upgradeLevel.level;
                float meleeRangeScale = PlayerUpgradeUtility.GetMeleeRangeScale(in weapon.weaponUpgrade, weaponLevel);
                MeleeAttackComponent melee = weapon.meleeAttack;
                melee.shape.radius *= meleeRangeScale;
                melee.shape.offset *= meleeRangeScale;

                actionState.committedShotCount++;
                float damage = PlayerUpgradeUtility.GetDamage(in weapon.weapon, in owner.playerUpgrade, weaponLevel);
                MeleeSpawnRequest spawnRequest = AttackUtility.CreateSpawnRequest(in melee, owner.position.current, owner.rotation.current, in weapon.owner.owner, Faction.Player, damage);
                meleeSpawnRequests.Enqueue(spawnRequest);
                attackCommittedRecords.Add(new PlayerAttackCommittedRecord
                {
                    player = weapon.owner.owner,
                    weaponIndex = weapon.owner.slotIndex,
                    impactPosition = spawnRequest.center,
                    impactEffectIndex = weapon.meleeView.impactEffectIndex,
                    impactScale = meleeRangeScale
                });
            }
        }
    }
}
