using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Requests;
using UnityEngine;

namespace TopView.Systems
{
    // 플레이어 무기의 조건 판정, 공격 발동, 재장전, 교체를 처리한다. 언제 부를지는 PlayerStateSystem이 정한다.
    public static class PlayerWeaponSystem
    {
        /// <summary>
        /// 공격을 내보낸다. 탄약이 없으면 아무것도 하지 않는다. Ranged와 Throw는 투사체 생성 요청을, Melee는 attackEndTicks까지 이어지는 근접 판정 생성 요청을 넣는다.
        /// </summary>
        /// <param name="player">플레이어</param>
        /// <param name="identifier">플레이어를 가리키는 값</param>
        /// <param name="weapon">들고 있는 무기</param>
        /// <param name="projectileSpawnRequests">투사체 생성 요청 큐</param>
        /// <param name="meleeSpawnRequests">근접 판정 생성 요청 큐</param>
        public static void CommitAttack(ref PlayerEntity player, in EntityIdentifier identifier, ref WeaponComponent weapon, Queue<ProjectileSpawnRequest> projectileSpawnRequests, Queue<MeleeSpawnRequest> meleeSpawnRequests)
        {
            player.isAttackCommitted = true;

            switch (weapon.kind)
            {
                case WeaponKind.Ranged:
                    if (weapon.currentAmmo <= 0)
                    {
                        Debug.LogWarning($"PlayerWeaponSystem >> 탄창 탄약이 {weapon.currentAmmo}이라 원거리 공격을 내보내지 않았다.");
                        return;
                    }
                    weapon.currentAmmo--;
                    RequestProjectile(in player, in weapon, player.currentRotation * Vector3.forward, projectileSpawnRequests);
                    break;

                case WeaponKind.Throw:
                    if (weapon.reserveAmmo <= 0)
                    {
                        Debug.LogWarning($"PlayerWeaponSystem >> 남은 탄약이 {weapon.reserveAmmo}이라 투척 공격을 내보내지 않았다.");
                        return;
                    }
                    weapon.reserveAmmo--;
                    Vector3 throwDirection = (player.currentRotation * Vector3.forward + Vector3.up * weapon.projectile.upwardRatio).normalized;
                    RequestProjectile(in player, in weapon, throwDirection, projectileSpawnRequests);
                    break;

                case WeaponKind.Melee:
                    meleeSpawnRequests.Enqueue(new MeleeSpawnRequest
                    {
                        owner = new EntityReference { kind = EntityKind.Player, identifier = identifier },
                        faction = Faction.Player,
                        center = player.currentPosition + player.currentRotation * Vector3.forward * weapon.meleeOffset,
                        radius = weapon.meleeRadius,
                        damage = weapon.damage,
                        durationTicks = weapon.attackEndTicks - weapon.attackStartTicks
                    });
                    break;
            }
        }

        /// <summary>
        /// 공격 상태를 끝낼 때인지 판정한다. Melee는 후딜레이까지 기다리고, Ranged와 Throw는 투사체를 내보내면 바로 끝난다.
        /// </summary>
        /// <param name="player">플레이어</param>
        /// <param name="weapon">들고 있는 무기</param>
        /// <returns>공격 상태를 끝낼 때면 true</returns>
        public static bool IsAttackFinished(in PlayerEntity player, in WeaponComponent weapon)
        {
            return weapon.kind == WeaponKind.Melee
                ? player.stateElapsedTicks >= weapon.attackDurationTicks
                : player.isAttackCommitted;
        }

        /// <summary>
        /// 들고 있는 무기로 들어갈 공격 상태를 구한다.
        /// </summary>
        /// <param name="player">플레이어</param>
        /// <param name="attackState">들어갈 공격 상태</param>
        /// <returns>들어갈 수 있으면 true. 탄약이 없거나 무기가 없으면 false</returns>
        public static bool TryGetAttackState(in PlayerEntity player, out PlayerState attackState)
        {
            attackState = PlayerState.Idle;
            if (!IsValidWeaponIndex(in player, player.currentWeaponIndex))
            {
                return false;
            }

            ref readonly WeaponComponent weapon = ref player.weapons[player.currentWeaponIndex];
            switch (weapon.kind)
            {
                case WeaponKind.Melee:
                    attackState = PlayerState.Swing;
                    return true;

                case WeaponKind.Ranged:
                    attackState = PlayerState.Shot;
                    return weapon.currentAmmo > 0;

                case WeaponKind.Throw:
                    attackState = PlayerState.Throw;
                    return weapon.reserveAmmo > 0;
            }

            return false;
        }

        /// <summary>
        /// Ranged의 연사 간격을 기다리는 중인지 판정한다. 공격 상태에 들어가야 대기가 생기므로 무기 번호는 유효하다.
        /// </summary>
        /// <param name="player">플레이어</param>
        /// <returns>기다리는 중이면 true</returns>
        public static bool IsWaitingNextShot(in PlayerEntity player)
        {
            return player.attackCooldownTicks > 0 && player.weapons[player.currentWeaponIndex].kind == WeaponKind.Ranged;
        }

        /// <summary>
        /// 그 번호의 무기로 교체할 수 있는지 판정한다.
        /// </summary>
        /// <param name="player">플레이어</param>
        /// <param name="weaponIndex">바꿀 무기 번호</param>
        /// <returns>들고 있지 않은 유효한 무기면 true</returns>
        public static bool CanSwap(in PlayerEntity player, int weaponIndex)
        {
            return weaponIndex != player.currentWeaponIndex && IsValidWeaponIndex(in player, weaponIndex);
        }

        /// <summary>
        /// 교체 상태에서 swapStartTicks가 되면 무기를 바꾸고 연사 대기를 지운다. swapStartTicks가 상태 길이보다 길어도 상태가 끝나기 전에 바꾼다.
        /// </summary>
        /// <param name="player">플레이어</param>
        public static void CommitSwap(ref PlayerEntity player)
        {
            if (player.currentWeaponIndex != player.pendingWeaponIndex && player.stateElapsedTicks >= Mathf.Min(player.action.swapStartTicks, player.action.swapTicks))
            {
                player.currentWeaponIndex = player.pendingWeaponIndex;
                player.attackCooldownTicks = 0;
            }
        }

        /// <summary>
        /// 들고 있는 무기를 재장전할 수 있는지 판정한다.
        /// </summary>
        /// <param name="player">플레이어</param>
        /// <returns>Ranged이고 탄창이 덜 찼으며 예비 탄약이 있으면 true</returns>
        public static bool CanReload(in PlayerEntity player)
        {
            if (!IsValidWeaponIndex(in player, player.currentWeaponIndex))
            {
                return false;
            }

            ref readonly WeaponComponent weapon = ref player.weapons[player.currentWeaponIndex];
            return weapon.kind == WeaponKind.Ranged && weapon.currentAmmo < weapon.maxAmmo && weapon.reserveAmmo > 0;
        }

        /// <summary>
        /// 예비 탄약에서 모자란 만큼 장전한다.
        /// </summary>
        /// <param name="player">플레이어</param>
        public static void Reload(ref PlayerEntity player)
        {
            ref WeaponComponent weapon = ref player.weapons[player.currentWeaponIndex];
            int loadAmmo = Mathf.Min(weapon.maxAmmo - weapon.currentAmmo, weapon.reserveAmmo);
            weapon.currentAmmo += loadAmmo;
            weapon.reserveAmmo -= loadAmmo;
        }

        // 총구 위치에서 플레이어 회전으로 투사체를 내보낸다.
        private static void RequestProjectile(in PlayerEntity player, in WeaponComponent weapon, Vector3 direction, Queue<ProjectileSpawnRequest> projectileSpawnRequests)
        {
            projectileSpawnRequests.Enqueue(new ProjectileSpawnRequest
            {
                projectile = weapon.projectile,
                faction = Faction.Player,
                damage = weapon.damage,
                position = player.currentPosition + player.currentRotation * player.movement.muzzleOffset,
                rotation = player.currentRotation,
                velocity = direction * weapon.projectile.speed
            });
        }

        private static bool IsValidWeaponIndex(in PlayerEntity player, int weaponIndex)
        {
            return weaponIndex >= 0 && weaponIndex < player.weapons.Length;
        }
    }
}
