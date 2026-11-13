using UnityEngine;

namespace TopView.Utilities
{
    // 근접 판정 구를 놓는 도구. 플레이어와 적의 근접 공격, 에디터 Gizmo가 함께 쓴다.
    public static class MeleeShapeUtility
    {
        /// <summary>
        /// 근접 판정 구의 중심을 구한다. 공격하는 쪽의 위치에서 바라보는 쪽으로 offset만큼 띄운다.
        /// </summary>
        /// <param name="position">공격하는 쪽의 위치(월드)</param>
        /// <param name="rotation">공격하는 쪽의 회전</param>
        /// <param name="offset">앞쪽으로 띄우는 거리(유닛). 강화 배율이 있으면 곱한 값을 넘긴다</param>
        /// <returns>판정 구의 중심(월드)</returns>
        public static Vector3 GetCenter(Vector3 position, Quaternion rotation, float offset)
        {
            return position + rotation * Vector3.forward * offset;
        }
    }
}
