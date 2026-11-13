using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Records;
using TopView.Requests;
using TopView.Utilities;
using UnityEngine;

namespace TopView.Systems
{
    // 공격 상태의 플레이어가 공격이 나갈 시간이 되면 공격을 내보낸다.
    public static class PlayerAttackSystem
    {
        /// <summary>
        /// Shot, Swing, Throw 상태에서 시점이 지난 발을 내보낸다. Melee는 근접 판정 하나, Ranged와 Throw는 무기의 shots 칸마다 투사체 하나다. PlayerMoveInputSystem 다음에 부른다.
        /// </summary>
        /// <param name="players">플레이어 저장소</param>
        /// <param name="weapons">무기 저장소</param>
        /// <param name="straightProjectileSpawnRequests">직선 투사체 생성 요청 큐. Ranged가 넣는다</param>
        /// <param name="arcProjectileSpawnRequests">포물선 투사체 생성 요청 큐. Throw가 넣는다</param>
        /// <param name="meleeSpawnRequests">근접 판정 생성 요청 큐</param>
        /// <param name="attackCommittedRecords">공격을 내보낸 기록 목록. 발이 실제로 나갔을 때만 발마다 남긴다</param>
        public static void Tick(PlayerStorage players, WeaponStorage weapons, Queue<StraightProjectileSpawnRequest> straightProjectileSpawnRequests, Queue<ArcProjectileSpawnRequest> arcProjectileSpawnRequests, Queue<MeleeSpawnRequest> meleeSpawnRequests, List<PlayerAttackCommittedRecord> attackCommittedRecords)
        {
            for (int index = 0; index < players.entities.Length; index++)
            {
                ref PlayerEntity entity = ref players.entities[index];
                if (!entity.isAlive)
                {
                    continue;
                }

                ref PlayerActionStateComponent actionState = ref entity.actionState;
                if (actionState.state != PlayerState.Shot && actionState.state != PlayerState.Swing && actionState.state != PlayerState.Throw)
                {
                    continue;
                }

                ref PlayerWeaponSlotsComponent weaponSlot = ref entity.weaponSlots;
                ref WeaponEntity weaponEntity = ref weapons.entities[weaponSlot.weapons[weaponSlot.currentIndex].index];
                ref readonly WeaponComponent weapon = ref weaponEntity.weapon;
                EntityIdentifier identifier = new EntityIdentifier { index = index, generation = entity.generation };
                int weaponLevel = weaponEntity.upgradeLevel.level;
                float damage = PlayerUpgradeUtility.GetDamage(in weapon, in entity.upgrade, weaponLevel);
                Vector3 position = entity.position.current;
                Quaternion rotation = entity.rotation.current;

                switch (weapon.kind)
                {
                    case WeaponKind.Melee:
                        CommitMelee(ref actionState, position, rotation, in identifier, in weapon, damage, weaponLevel, weaponSlot.currentIndex, meleeSpawnRequests, attackCommittedRecords);
                        break;

                    case WeaponKind.Ranged:
                        CommitStraight(ref actionState, ref weaponEntity.ammoState, position, rotation, in identifier, in weapon, damage, weaponLevel, weaponSlot.currentIndex, straightProjectileSpawnRequests, attackCommittedRecords);
                        break;

                    case WeaponKind.Throw:
                        CommitArc(ref actionState, ref weaponEntity.ammoState, position, rotation, in identifier, in weapon, damage, weaponLevel, weaponSlot.currentIndex, arcProjectileSpawnRequests, attackCommittedRecords);
                        break;

                    default:
                        // 경고를 한 번만 남기도록 내보낸 것으로 친다. 이 종류의 발 수는 0이라 공격 상태는 끝날 수 있다
                        if (actionState.committedShotCount == 0)
                        {
                            Debug.LogWarning($"PlayerAttackSystem >> 처리하지 않는 무기 종류({weapon.kind})라 공격을 내보내지 않았다.");
                            actionState.committedShotCount = 1;
                        }
                        break;
                }
            }
        }

        // 판정 시점이 되면 근접 판정을 내보낸다. 판정 범위와 앞쪽으로 띄우는 거리는 근접 범위 강화를 반영한다.
        private static void CommitMelee(ref PlayerActionStateComponent actionState, Vector3 position, Quaternion rotation, in EntityIdentifier identifier, in WeaponComponent weapon, float damage, int weaponLevel, int weaponIndex, Queue<MeleeSpawnRequest> meleeSpawnRequests, List<PlayerAttackCommittedRecord> attackCommittedRecords)
        {
            if (!AttackUtility.IsShotDue(in weapon.melee, actionState.committedShotCount, actionState.elapsedTicks))
            {
                return;
            }

            float meleeRangeScale = PlayerUpgradeUtility.GetMeleeRangeScale(in weapon, weaponLevel);
            MeleeAttackComponent melee = weapon.melee;
            melee.shape.radius *= meleeRangeScale;
            melee.shape.offset *= meleeRangeScale;

            actionState.committedShotCount++;
            MeleeSpawnRequest spawnRequest = AttackUtility.CreateSpawnRequest(in melee, position, rotation, in identifier, Faction.Player, damage);
            meleeSpawnRequests.Enqueue(spawnRequest);
            attackCommittedRecords.Add(new PlayerAttackCommittedRecord
            {
                player = identifier,
                weaponIndex = weaponIndex,
                impactPosition = spawnRequest.center,
                impactEffectIndex = weapon.meleeView.impactEffectIndex,
                impactScale = meleeRangeScale
            });
        }

        // 시점이 지난 발마다 그 발의 위치에서 플레이어 회전의 정면으로 직선 투사체를 내보낸다. 첫 발이 나갈 때 탄창 탄약을 하나 쓰며, 탄약이 없으면 아무 발도 내보내지 않는다.
        // 폭발 반지름은 강화가 반영된 값으로 바꿔 담는다.
        private static void CommitStraight(ref PlayerActionStateComponent actionState, ref AmmoStateComponent ammo, Vector3 position, Quaternion rotation, in EntityIdentifier identifier, in WeaponComponent weapon, float damage, int weaponLevel, int weaponIndex, Queue<StraightProjectileSpawnRequest> spawnRequests, List<PlayerAttackCommittedRecord> attackCommittedRecords)
        {
            if (actionState.committedShotCount == 0 && AttackUtility.IsShotDue(in weapon.straight, 0, actionState.elapsedTicks))
            {
                if (ammo.current <= 0)
                {
                    Debug.LogWarning($"PlayerAttackSystem >> 탄창 탄약이 {ammo.current}이라 원거리 공격을 내보내지 않았다.");
                    actionState.committedShotCount = AttackUtility.GetShotCount(in weapon.straight);
                    return;
                }
                ammo.current--;
            }

            StraightAttackComponent straight = weapon.straight;
            straight.projectile.explosion.radius = PlayerUpgradeUtility.GetExplosionRadius(in weapon, weaponLevel);
            while (AttackUtility.IsShotDue(in straight, actionState.committedShotCount, actionState.elapsedTicks))
            {
                Vector3 shotPosition = AttackUtility.GetShotPosition(in straight.shots[actionState.committedShotCount], position, rotation);
                spawnRequests.Enqueue(AttackUtility.CreateSpawnRequest(in straight, in weapon.projectileView, shotPosition, rotation, Faction.Player, damage));
                attackCommittedRecords.Add(CreateProjectileRecord(in identifier, weaponIndex));
                actionState.committedShotCount++;
            }
        }

        // 시점이 지난 발마다 그 발의 위치에서 충전량으로 정한 착탄점으로 포물선 투사체를 던진다. 첫 발이 나갈 때 예비 탄약을 하나 쓰며, 탄약이 없거나 비행 시간이 잘못되었으면 아무 발도 내보내지 않는다.
        // 폭발 반지름은 강화가 반영된 값으로 바꿔 담는다.
        private static void CommitArc(ref PlayerActionStateComponent actionState, ref AmmoStateComponent ammo, Vector3 position, Quaternion rotation, in EntityIdentifier identifier, in WeaponComponent weapon, float damage, int weaponLevel, int weaponIndex, Queue<ArcProjectileSpawnRequest> spawnRequests, List<PlayerAttackCommittedRecord> attackCommittedRecords)
        {
            if (actionState.committedShotCount == 0 && AttackUtility.IsShotDue(in weapon.arc, 0, actionState.elapsedTicks))
            {
                if (ammo.reserve <= 0)
                {
                    Debug.LogWarning($"PlayerAttackSystem >> 남은 탄약이 {ammo.reserve}이라 투척 공격을 내보내지 않았다.");
                    actionState.committedShotCount = AttackUtility.GetShotCount(in weapon.arc);
                    return;
                }
                if (weapon.arc.projectile.flight.flightTicks <= 0)
                {
                    Debug.LogWarning($"PlayerAttackSystem >> flightTicks({weapon.arc.projectile.flight.flightTicks})가 0 이하라 투척 공격을 내보내지 않았다. flightTicks는 0보다 커야 한다.");
                    actionState.committedShotCount = AttackUtility.GetShotCount(in weapon.arc);
                    return;
                }
                ammo.reserve--;
            }

            ArcAttackComponent arc = weapon.arc;
            arc.projectile.explosion.radius = PlayerUpgradeUtility.GetExplosionRadius(in weapon, weaponLevel);
            Vector3 targetPosition = PlayerWeaponUtility.GetThrowTargetPosition(in weapon.throwing, position, rotation, actionState.throwCharge);
            while (AttackUtility.IsShotDue(in arc, actionState.committedShotCount, actionState.elapsedTicks))
            {
                Vector3 shotPosition = AttackUtility.GetShotPosition(in arc.shots[actionState.committedShotCount], position, rotation);
                spawnRequests.Enqueue(AttackUtility.CreateSpawnRequest(in arc, in weapon.projectileView, shotPosition, rotation, targetPosition, Faction.Player, damage));
                attackCommittedRecords.Add(CreateProjectileRecord(in identifier, weaponIndex));
                actionState.committedShotCount++;
            }
        }

        // 투사체 한 발의 공격 기록. 임팩트 이펙트가 없다
        private static PlayerAttackCommittedRecord CreateProjectileRecord(in EntityIdentifier identifier, int weaponIndex)
        {
            return new PlayerAttackCommittedRecord
            {
                player = identifier,
                weaponIndex = weaponIndex,
                impactPosition = Vector3.zero,
                impactEffectIndex = -1,
                impactScale = 1f
            };
        }
    }
}
