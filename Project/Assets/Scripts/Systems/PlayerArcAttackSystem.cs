using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Records;
using TopView.Requests;
using TopView.Utilities;
using UnityEngine;

namespace TopView.Systems
{
    // 주인이 공격 상태일 때 포물선 투사체 무기의 발 시점이 되면 발마다 충전량으로 정한 착탄점으로 포물선 투사체를 던진다.
    public static class PlayerArcAttackSystem
    {
        // 처리할 무기가 가진 컴포넌트
        private const ComponentMask WEAPON_MASK = ComponentMask.Owner | ComponentMask.Weapon | ComponentMask.UpgradeLevel | ComponentMask.WeaponUpgrade
            | ComponentMask.ArcAttack | ComponentMask.Throw | ComponentMask.ProjectileView;
        // 공격을 내보내는 주인이 가진 컴포넌트
        private const ComponentMask OWNER_MASK = ComponentMask.PlayerActionState | ComponentMask.WeaponSlots | ComponentMask.PlayerUpgrade | ComponentMask.Position | ComponentMask.Rotation;

        /// <summary>
        /// 포물선 투사체 무기를 든 주인이 공격 상태이면 시점이 지난 발을 던진다. 첫 발이 나갈 때 탄약을 하나 쓰며, 탄약이 없거나 비행 시간이 잘못되었으면 아무 발도 내보내지 않는다. 폭발 반지름은 강화가 반영된 값으로 바꿔 담는다. PlayerMoveInputSystem 다음에 부른다.
        /// </summary>
        /// <param name="storage">엔티티 저장소. 무기(Owner, Weapon, UpgradeLevel, WeaponUpgrade, ArcAttack, Throw, ProjectileView)의 탄약을 쓰고, 주인(PlayerActionState, WeaponSlots, PlayerUpgrade, Position, Rotation)의 공격 진행을 고친다</param>
        /// <param name="spawnRequests">포물선 투사체 생성 요청 큐</param>
        /// <param name="attackCommittedRecords">공격을 내보낸 기록 목록. 발마다 남긴다</param>
        public static void Tick(EntityStorage storage, Queue<ArcProjectileSpawnRequest> spawnRequests, List<PlayerAttackCommittedRecord> attackCommittedRecords)
        {
            for (int index = 0; index < storage.entities.Length; index++)
            {
                ref Entity weapon = ref storage.entities[index];
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
                if (actionState.committedShotCount == 0 && AttackUtility.IsShotDue(in weapon.arcAttack, 0, actionState.elapsedTicks))
                {
                    if (weapon.arcAttack.projectile.flight.flightTicks <= 0)
                    {
                        Debug.LogWarning($"PlayerArcAttackSystem >> flightTicks({weapon.arcAttack.projectile.flight.flightTicks})가 0 이하라 포물선 투사체 공격을 내보내지 않았다. flightTicks는 0보다 커야 한다.");
                        actionState.committedShotCount = AttackUtility.GetShotCount(in weapon.arcAttack);
                        continue;
                    }
                    if (!PlayerWeaponUtility.TryUseAmmo(ref weapon, out int remainingAmmo))
                    {
                        Debug.LogWarning($"PlayerArcAttackSystem >> 탄약이 {remainingAmmo}이라 포물선 투사체 공격을 내보내지 않았다.");
                        actionState.committedShotCount = AttackUtility.GetShotCount(in weapon.arcAttack);
                        continue;
                    }
                }

                int weaponLevel = weapon.upgradeLevel.level;
                float damage = PlayerUpgradeUtility.GetDamage(in weapon.weapon, in owner.playerUpgrade, weaponLevel);
                ArcAttackComponent arc = weapon.arcAttack;
                arc.projectile.explosion.radius = PlayerUpgradeUtility.GetExplosionRadius(in weapon, weaponLevel);
                Vector3 position = owner.position.current;
                Quaternion rotation = owner.rotation.current;
                Vector3 targetPosition = PlayerWeaponUtility.GetThrowTargetPosition(in weapon.throwing, position, rotation, actionState.throwCharge);
                while (AttackUtility.IsShotDue(in arc, actionState.committedShotCount, actionState.elapsedTicks))
                {
                    Vector3 shotPosition = AttackUtility.GetShotPosition(in arc.shots[actionState.committedShotCount], position, rotation);
                    spawnRequests.Enqueue(AttackUtility.CreateSpawnRequest(in arc, in weapon.projectileView, shotPosition, rotation, targetPosition, Faction.Player, damage));
                    attackCommittedRecords.Add(CreateRecord(in weapon.owner));
                    actionState.committedShotCount++;
                }
            }
        }

        // 투사체 한 발의 공격 기록. 임팩트 이펙트가 없다
        private static PlayerAttackCommittedRecord CreateRecord(in OwnerComponent owner)
        {
            return new PlayerAttackCommittedRecord
            {
                player = owner.owner,
                weaponIndex = owner.slotIndex,
                impactPosition = Vector3.zero,
                impactEffectIndex = -1,
                impactScale = 1f
            };
        }
    }
}
