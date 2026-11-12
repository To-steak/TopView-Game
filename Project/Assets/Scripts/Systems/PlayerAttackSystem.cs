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
        /// Shot, Swing, Throw 상태에서 timing.startTicks가 지났고 아직 공격이 나가지 않았으면 공격을 내보낸다. PlayerMoveInputSystem 다음에 부른다.
        /// </summary>
        /// <param name="players">플레이어 저장소</param>
        /// <param name="weapons">무기 저장소</param>
        /// <param name="projectileSpawnRequests">투사체 생성 요청 큐</param>
        /// <param name="meleeSpawnRequests">근접 판정 생성 요청 큐</param>
        /// <param name="attackCommittedRecords">공격을 내보낸 기록 목록. 공격이 실제로 나갔을 때만 남긴다</param>
        public static void Tick(PlayerStorage players, WeaponStorage weapons, Queue<ProjectileSpawnRequest> projectileSpawnRequests, Queue<MeleeSpawnRequest> meleeSpawnRequests, List<PlayerAttackCommittedRecord> attackCommittedRecords)
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
                if (!actionState.isAttackCommitted && actionState.elapsedTicks >= weapon.timing.startTicks)
                {
                    EntityIdentifier identifier = new EntityIdentifier { index = index, generation = entity.generation };
                    int weaponLevel = PlayerUpgradeUtility.GetWeaponLevel(in entity.upgradeLevels, weaponSlot.currentIndex);
                    if (CommitAttack(ref actionState, ref weaponEntity.ammoState, entity.position.current, entity.rotation.current, in identifier, in weapon, in entity.upgrade, weaponLevel, projectileSpawnRequests, meleeSpawnRequests, out Vector3 meleeCenter))
                    {
                        attackCommittedRecords.Add(new PlayerAttackCommittedRecord
                        {
                            player = identifier,
                            weaponIndex = weaponSlot.currentIndex,
                            impactPosition = meleeCenter,
                            impactEffectIndex = weapon.kind == WeaponKind.Melee ? weapon.impactEffectIndex : -1,
                            impactScale = weapon.kind == WeaponKind.Melee ? PlayerUpgradeUtility.GetMeleeRangeScale(in weapon, weaponLevel) : 1f
                        });
                    }
                }
            }
        }

        // 공격을 내보낸다. 탄약이 없으면 아무것도 하지 않는다. Ranged와 Throw는 투사체 생성 요청을, Melee는 timing.endTicks까지 이어지는 근접 판정 생성 요청을 넣는다.
        // 공격이 실제로 나갔으면 true를 돌려준다. meleeCenter는 Melee일 때 판정 원의 중심이고, 그 밖에는 Vector3.zero다. 피해량, 근접 범위, 폭발 반지름은 무기 강화 레벨을 반영한다.
        private static bool CommitAttack(ref PlayerActionStateComponent actionState, ref AmmoStateComponent ammo, Vector3 position, Quaternion rotation, in EntityIdentifier identifier, in WeaponComponent weapon, in PlayerUpgradeComponent upgrade, int weaponLevel, Queue<ProjectileSpawnRequest> projectileSpawnRequests, Queue<MeleeSpawnRequest> meleeSpawnRequests, out Vector3 meleeCenter)
        {
            meleeCenter = Vector3.zero;
            float damage = PlayerUpgradeUtility.GetDamage(in weapon, in upgrade, weaponLevel);
            float explosionRadius = PlayerUpgradeUtility.GetExplosionRadius(in weapon, weaponLevel);
            actionState.isAttackCommitted = true;

            switch (weapon.kind)
            {
                case WeaponKind.Ranged:
                    if (ammo.current <= 0)
                    {
                        Debug.LogWarning($"PlayerAttackSystem >> 탄창 탄약이 {ammo.current}이라 원거리 공격을 내보내지 않았다.");
                        return false;
                    }
                    ammo.current--;
                    RequestProjectile(position, rotation, in weapon, damage, explosionRadius, rotation * Vector3.forward * weapon.projectile.speed, Vector3.zero, projectileSpawnRequests);
                    break;

                case WeaponKind.Throw:
                    if (ammo.reserve <= 0)
                    {
                        Debug.LogWarning($"PlayerAttackSystem >> 남은 탄약이 {ammo.reserve}이라 투척 공격을 내보내지 않았다.");
                        return false;
                    }
                    if (weapon.projectile.flightTicks <= 0)
                    {
                        Debug.LogWarning($"PlayerAttackSystem >> flightTicks({weapon.projectile.flightTicks})가 0 이하라 투척 공격을 내보내지 않았다. flightTicks는 0보다 커야 한다.");
                        return false;
                    }
                    ammo.reserve--;
                    Vector3 targetPosition = PlayerWeaponUtility.GetThrowTargetPosition(in weapon.throwing, position, rotation, actionState.throwCharge);
                    RequestProjectile(position, rotation, in weapon, damage, explosionRadius, Vector3.zero, targetPosition, projectileSpawnRequests);
                    break;

                case WeaponKind.Melee:
                    float meleeRangeScale = PlayerUpgradeUtility.GetMeleeRangeScale(in weapon, weaponLevel);
                    meleeCenter = MeleeShapeUtility.GetCenter(position, rotation, weapon.meleeShape.offset * meleeRangeScale);
                    meleeSpawnRequests.Enqueue(new MeleeSpawnRequest
                    {
                        owner = identifier,
                        faction = Faction.Player,
                        center = meleeCenter,
                        radius = weapon.meleeShape.radius * meleeRangeScale,
                        damage = damage,
                        durationTicks = weapon.timing.endTicks - weapon.timing.startTicks
                    });
                    break;

                default:
                    Debug.LogWarning($"PlayerAttackSystem >> 처리하지 않는 무기 종류({weapon.kind})라 공격을 내보내지 않았다.");
                    return false;
            }

            return true;
        }

        // 무기의 총구 위치에서 플레이어 회전으로 투사체를 내보낸다. Straight는 velocity로, Grenade는 targetPosition을 향해 날아간다. 폭발 반지름은 강화가 반영된 값으로 바꿔 담는다.
        private static void RequestProjectile(Vector3 position, Quaternion rotation, in WeaponComponent weapon, float damage, float explosionRadius, Vector3 velocity, Vector3 targetPosition, Queue<ProjectileSpawnRequest> projectileSpawnRequests)
        {
            ProjectileComponent projectile = weapon.projectile;
            projectile.explosionRadius = explosionRadius;
            projectileSpawnRequests.Enqueue(new ProjectileSpawnRequest
            {
                projectile = projectile,
                faction = Faction.Player,
                damage = damage,
                position = position + rotation * weapon.muzzleOffset,
                rotation = rotation,
                velocity = velocity,
                targetPosition = targetPosition
            });
        }
    }
}
