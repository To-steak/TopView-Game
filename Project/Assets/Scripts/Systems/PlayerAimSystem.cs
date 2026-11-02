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

        // 조준으로 도는 엔티티가 가지는 컴포넌트
        private const ComponentMask AIM_QUERY = ComponentMask.Position | ComponentMask.Rotation | ComponentMask.PlayerMotion;

        /// <summary>
        /// 회전이 잠겨 있지 않고 조준점이 있으면 조준점을 바라본다.
        /// </summary>
        /// <param name="masks">칸별 컴포넌트 표시</param>
        /// <param name="positions">위치</param>
        /// <param name="rotations">회전</param>
        /// <param name="motions">플레이어 이동 상태</param>
        /// <param name="command">플레이어 입력</param>
        public static void Tick(ComponentMask[] masks, PositionComponent[] positions, RotationComponent[] rotations, PlayerMotionComponent[] motions, in PlayerCommand command)
        {
            for (int index = 0; index < masks.Length; index++)
            {
                if ((masks[index] & AIM_QUERY) != AIM_QUERY)
                {
                    continue;
                }

                if (!motions[index].isRotationLocked && command.hasAimPoint)
                {
                    Vector3 aimDirection = command.aimPoint - positions[index].current;
                    aimDirection.y = 0f;
                    if (aimDirection.sqrMagnitude > MIN_AIM_DISTANCE_SQUARED)
                    {
                        rotations[index].current = Quaternion.LookRotation(aimDirection);
                    }
                }
            }
        }
    }
}
