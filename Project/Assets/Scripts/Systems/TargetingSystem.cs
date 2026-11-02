using TopView.Components;
using TopView.Entities;
using UnityEngine;

namespace TopView.Systems
{
    // 여러 System이 함께 쓰는 대상 찾기와 XZ 평면 거리·방향 도구.
    public static class TargetingSystem
    {
        // 이보다 가까운 방향으로는 돌지 않는다(유닛²)
        private const float MIN_TURN_DISTANCE_SQUARED = 0.001f;

        /// <summary>
        /// 살아 있는 플레이어의 위치와 피격 반지름을 돌려준다.
        /// </summary>
        /// <param name="masks">칸별 컴포넌트 표시</param>
        /// <param name="positions">위치</param>
        /// <param name="movements">플레이어 이동 값</param>
        /// <param name="playerPosition">플레이어 위치(월드)</param>
        /// <param name="playerRadius">플레이어 이동 충돌 캡슐의 반지름. 감지와 공격 범위에 더한다</param>
        /// <returns>살아 있는 플레이어가 있으면 true</returns>
        public static bool TryGetPlayerTarget(ComponentMask[] masks, PositionComponent[] positions, PlayerMovementComponent[] movements, out Vector3 playerPosition, out float playerRadius)
        {
            for (int index = 0; index < masks.Length; index++)
            {
                if ((masks[index] & ComponentMask.PlayerArchetype) != ComponentMask.PlayerArchetype)
                {
                    continue;
                }

                playerPosition = positions[index].current;
                playerRadius = movements[index].capsuleRadius;
                return true;
            }

            playerPosition = Vector3.zero;
            playerRadius = 0f;
            return false;
        }

        /// <summary>
        /// 목표 쪽(XZ)으로 주어진 각도까지 돈다.
        /// </summary>
        /// <param name="rotation">돌릴 회전</param>
        /// <param name="position">도는 쪽의 위치(월드)</param>
        /// <param name="targetPosition">바라볼 위치(월드)</param>
        /// <param name="maxDegrees">이번 틱에 돌 수 있는 최대 각도(도)</param>
        public static void RotateToward(ref Quaternion rotation, Vector3 position, Vector3 targetPosition, float maxDegrees)
        {
            Vector3 direction = targetPosition - position;
            direction.y = 0f;
            if (direction.sqrMagnitude > MIN_TURN_DISTANCE_SQUARED)
            {
                rotation = Quaternion.RotateTowards(rotation, Quaternion.LookRotation(direction), maxDegrees);
            }
        }

        /// <summary>
        /// 두 위치의 XZ 거리가 주어진 거리 이하인지 확인한다.
        /// </summary>
        /// <param name="from">한쪽 위치(월드)</param>
        /// <param name="to">다른 쪽 위치(월드)</param>
        /// <param name="distance">비교할 거리(유닛)</param>
        /// <returns>XZ 거리가 distance 이하이면 true</returns>
        public static bool IsWithin(Vector3 from, Vector3 to, float distance)
        {
            float x = to.x - from.x;
            float z = to.z - from.z;
            return x * x + z * z <= distance * distance;
        }
    }
}
