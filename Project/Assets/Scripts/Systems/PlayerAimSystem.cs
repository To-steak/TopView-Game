using TopView.Commands;
using TopView.Entities;
using UnityEngine;

namespace TopView.Systems
{
    // 회전이 잠겨 있지 않은 플레이어를 조준점 쪽으로 돌린다.
    public static class PlayerAimSystem
    {
        // 조준점이 이보다 가까우면 돌지 않는다(유닛²)
        private const float MIN_AIM_DISTANCE_SQUARED = 0.01f;

        /// <summary>
        /// 회전이 잠겨 있지 않고 조준점이 있으면 조준점을 바라본다.
        /// </summary>
        /// <param name="players">플레이어 저장소</param>
        /// <param name="command">플레이어 입력</param>
        public static void Tick(PlayerStorage players, in PlayerCommand command)
        {
            for (int playerIndex = 0; playerIndex < players.entities.Length; playerIndex++)
            {
                ref PlayerEntity player = ref players.entities[playerIndex];
                if (!player.isAlive)
                {
                    continue;
                }

                if (!player.isRotationLocked && command.hasAimPoint)
                {
                    Vector3 aimDirection = command.aimPoint - player.currentPosition;
                    aimDirection.y = 0f;
                    if (aimDirection.sqrMagnitude > MIN_AIM_DISTANCE_SQUARED)
                    {
                        player.currentRotation = Quaternion.LookRotation(aimDirection);
                    }
                }
            }
        }
    }
}
