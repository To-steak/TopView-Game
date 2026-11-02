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
        /// <param name="masks">칸별 컴포넌트 표시</param>
        /// <param name="generations">칸이 채워진 횟수</param>
        /// <param name="positions">위치</param>
        /// <param name="rotations">회전</param>
        /// <param name="movements">플레이어 이동 값</param>
        /// <param name="actionStates">플레이어 행동 상태와 공격 진행</param>
        /// <param name="weaponLists">무기 목록</param>
        /// <param name="weaponSlots">든 무기와 무기별 탄약</param>
        /// <param name="projectileSpawnRequests">투사체 생성 요청 큐</param>
        /// <param name="meleeSpawnRequests">근접 판정 생성 요청 큐</param>
        public static void Tick(ComponentMask[] masks, int[] generations, PositionComponent[] positions, RotationComponent[] rotations, PlayerMovementComponent[] movements, PlayerActionStateComponent[] actionStates, WeaponListComponent[] weaponLists, PlayerWeaponSlotsComponent[] weaponSlots, Queue<ProjectileSpawnRequest> projectileSpawnRequests, Queue<MeleeSpawnRequest> meleeSpawnRequests)
        {
            for (int index = 0; index < masks.Length; index++)
            {
                if ((masks[index] & ComponentMask.PlayerArchetype) != ComponentMask.PlayerArchetype)
                {
                    continue;
                }

                ref PlayerActionStateComponent actionState = ref actionStates[index];
                if (actionState.state != PlayerState.Shot && actionState.state != PlayerState.Swing && actionState.state != PlayerState.Throw)
                {
                    continue;
                }

                ref PlayerWeaponSlotsComponent weaponSlot = ref weaponSlots[index];
                ref readonly WeaponComponent weapon = ref weaponLists[index].weapons[weaponSlot.currentIndex];
                if (!actionState.isAttackCommitted && actionState.elapsedTicks >= weapon.attackStartTicks)
                {
                    EntityIdentifier identifier = new EntityIdentifier { index = index, generation = generations[index] };
                    PlayerWeaponSystem.CommitAttack(ref actionState, ref weaponSlot, in movements[index], positions[index].current, rotations[index].current, in identifier, in weapon, projectileSpawnRequests, meleeSpawnRequests);
                }
            }
        }
    }
}
