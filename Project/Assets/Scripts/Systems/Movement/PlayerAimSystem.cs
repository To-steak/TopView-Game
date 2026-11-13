using TopView.Commands;
using TopView.Components;
using TopView.Entities;
using UnityEngine;

namespace TopView.Systems
{
    // 회전이 잠겨 있지 않은 플레이어를 조준점 쪽으로 돌린다.
    public static class PlayerAimSystem
    {
        // 조준점이 이보다 가까우면 돌지 않는다(유닛²)
        private const float MIN_AIM_DISTANCE_SQUARED = 0.01f;

        // 처리할 플레이어가 가진 컴포넌트
        private const ComponentMask PLAYER_MASK = ComponentMask.PlayerActionState | ComponentMask.Position | ComponentMask.Rotation | ComponentMask.PlayerMotion;

        /// <summary>
        /// 회전이 잠겨 있지 않고 조준점이 있으면 조준점을 바라본다.
        /// </summary>
        /// <param name="storage">엔티티 저장소. 플레이어(PlayerActionState, Position, Rotation, PlayerMotion)만 처리한다</param>
        /// <param name="command">플레이어 입력</param>
        public static void Tick(EntityStorage storage, in PlayerCommand command)
        {
            for (int index = 0; index < storage.entities.Length; index++)
            {
                ref Entity entity = ref storage.entities[index];
                if ((entity.mask & PLAYER_MASK) == PLAYER_MASK && !entity.playerMotion.isRotationLocked && command.hasAimPoint)
                {
                    Aim(ref entity.rotation, entity.position.current, command.aimPoint);
                }
            }
        }

        private static void Aim(ref RotationComponent rotation, Vector3 position, Vector3 aimPoint)
        {
            Vector3 aimDirection = aimPoint - position;
            aimDirection.y = 0f;
            if (aimDirection.sqrMagnitude > MIN_AIM_DISTANCE_SQUARED)
            {
                rotation.current = Quaternion.LookRotation(aimDirection);
            }
        }
    }
}
