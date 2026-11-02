using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Requests;

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
                    PlayerWeaponSystem.CommitAttack(ref actionState, ref weaponSlot, in entity.movement, entity.position.current, entity.rotation.current, in identifier, in weapon, projectileSpawnRequests, meleeSpawnRequests);
                }
            }
        }
    }
}
