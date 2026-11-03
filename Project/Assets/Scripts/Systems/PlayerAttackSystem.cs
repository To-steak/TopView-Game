using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Requests;
using UnityEngine;

namespace TopView.Systems
{
    // 공격 상태의 플레이어가 공격이 나갈 시간이 되면 공격을 내보낸다.
    public static class PlayerAttackSystem
    {
        /// <summary>
        /// Shot, Swing, Throw 상태에서 attackStartTicks가 지났고 아직 공격이 나가지 않았으면 공격을 내보낸다. PlayerMoveInputSystem 다음에 부른다.
        /// </summary>
        /// <param name="players">플레이어 저장소</param>
        /// <param name="projectileSpawnRequests">투사체 생성 요청 큐</param>
        /// <param name="meleeSpawnRequests">근접 판정 생성 요청 큐</param>
        public static void Tick(PlayerStorage players, Queue<ProjectileSpawnRequest> projectileSpawnRequests, Queue<MeleeSpawnRequest> meleeSpawnRequests)
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
                ref readonly WeaponComponent weapon = ref entity.weaponList.weapons[weaponSlot.currentIndex];
                if (!actionState.isAttackCommitted && actionState.elapsedTicks >= weapon.attackStartTicks)
                {
                    EntityIdentifier identifier = new EntityIdentifier { index = index, generation = entity.generation };
                    CommitAttack(ref actionState, ref weaponSlot, in entity.movement, entity.position.current, entity.rotation.current, in identifier, in weapon, projectileSpawnRequests, meleeSpawnRequests);
                }
            }
        }

        // 공격을 내보낸다. 탄약이 없으면 아무것도 하지 않는다. Ranged와 Throw는 투사체 생성 요청을, Melee는 attackEndTicks까지 이어지는 근접 판정 생성 요청을 넣는다.
        private static void CommitAttack(ref PlayerActionStateComponent actionState, ref PlayerWeaponSlotsComponent weaponSlots, in PlayerMovementComponent movement, Vector3 position, Quaternion rotation, in EntityIdentifier identifier, in WeaponComponent weapon, Queue<ProjectileSpawnRequest> projectileSpawnRequests, Queue<MeleeSpawnRequest> meleeSpawnRequests)
        {
            actionState.isAttackCommitted = true;
            int weaponIndex = weaponSlots.currentIndex;

            switch (weapon.kind)
            {
                case WeaponKind.Ranged:
                    if (weaponSlots.currentAmmo[weaponIndex] <= 0)
                    {
                        Debug.LogWarning($"PlayerAttackSystem >> 탄창 탄약이 {weaponSlots.currentAmmo[weaponIndex]}이라 원거리 공격을 내보내지 않았다.");
                        return;
                    }
                    weaponSlots.currentAmmo[weaponIndex]--;
                    RequestProjectile(in movement, position, rotation, in weapon, rotation * Vector3.forward, projectileSpawnRequests);
                    break;

                case WeaponKind.Throw:
                    if (weaponSlots.reserveAmmo[weaponIndex] <= 0)
                    {
                        Debug.LogWarning($"PlayerAttackSystem >> 남은 탄약이 {weaponSlots.reserveAmmo[weaponIndex]}이라 투척 공격을 내보내지 않았다.");
                        return;
                    }
                    weaponSlots.reserveAmmo[weaponIndex]--;
                    Vector3 throwDirection = (rotation * Vector3.forward + Vector3.up * weapon.projectile.upwardRatio).normalized;
                    RequestProjectile(in movement, position, rotation, in weapon, throwDirection, projectileSpawnRequests);
                    break;

                case WeaponKind.Melee:
                    meleeSpawnRequests.Enqueue(new MeleeSpawnRequest
                    {
                        owner = identifier,
                        faction = Faction.Player,
                        center = position + rotation * Vector3.forward * weapon.meleeOffset,
                        radius = weapon.meleeRadius,
                        damage = weapon.damage,
                        durationTicks = weapon.attackEndTicks - weapon.attackStartTicks
                    });
                    break;
            }
        }

        // 총구 위치에서 플레이어 회전으로 투사체를 내보낸다.
        private static void RequestProjectile(in PlayerMovementComponent movement, Vector3 position, Quaternion rotation, in WeaponComponent weapon, Vector3 direction, Queue<ProjectileSpawnRequest> projectileSpawnRequests)
        {
            projectileSpawnRequests.Enqueue(new ProjectileSpawnRequest
            {
                projectile = weapon.projectile,
                faction = Faction.Player,
                damage = weapon.damage,
                position = position + rotation * movement.muzzleOffset,
                rotation = rotation,
                velocity = direction * weapon.projectile.speed
            });
        }
    }
}
