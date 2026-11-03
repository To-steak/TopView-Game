using TopView.Components;
using UnityEngine;

namespace TopView.Utilities
{
    // 플레이어 이동 충돌 캡슐의 계산 도구.
    public static class PlayerMovementUtility
    {
        /// <summary>
        /// 플레이어 이동 충돌 캡슐의 위아래 구 중심을 구한다.
        /// </summary>
        /// <param name="movement">플레이어 이동 값</param>
        /// <param name="rotation">플레이어 회전(월드)</param>
        /// <param name="position">캡슐을 놓을 플레이어 위치</param>
        /// <param name="top">위쪽 구의 중심</param>
        /// <param name="bottom">아래쪽 구의 중심</param>
        public static void GetCapsulePoints(in PlayerMovementComponent movement, Quaternion rotation, Vector3 position, out Vector3 top, out Vector3 bottom)
        {
            Vector3 center = position + rotation * movement.capsuleCenter;
            float halfLine = Mathf.Max(movement.capsuleHeight * 0.5f - movement.capsuleRadius, 0f);
            top = center + Vector3.up * halfLine;
            bottom = center - Vector3.up * halfLine;
        }
    }
}
