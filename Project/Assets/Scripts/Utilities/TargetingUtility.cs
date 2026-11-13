using UnityEngine;

namespace TopView.Utilities
{
    // 여러 System이 함께 쓰는 XZ 평면 거리·방향 도구.
    public static class TargetingUtility
    {
        // 이보다 가까운 방향으로는 돌지 않는다(유닛²)
        private const float MIN_TURN_DISTANCE_SQUARED = 0.001f;

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
            float deltaX = to.x - from.x;
            float deltaZ = to.z - from.z;
            return deltaX * deltaX + deltaZ * deltaZ <= distance * distance;
        }
    }
}
