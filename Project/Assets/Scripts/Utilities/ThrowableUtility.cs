using TopView.Components;
using UnityEngine;

namespace TopView.Utilities
{
    // 던지는 투사체(Grenade)의 포물선 계산과 환경 충돌 검사 도구. 실제 비행과 투척 미리보기가 함께 써서 둘이 어긋나지 않게 한다.
    public static class ThrowableUtility
    {
        /// <summary>
        /// 던진 뒤 지난 시간으로 비행 진행도를 구한다.
        /// </summary>
        /// <param name="elapsedTicks">던진 뒤 지난 시간(틱)</param>
        /// <param name="flightTicks">착탄점에 닿기까지의 시간(틱)</param>
        /// <returns>진행도(0~1). flightTicks가 0 이하면 1</returns>
        public static float GetArcProgress(int elapsedTicks, int flightTicks)
        {
            return flightTicks > 0 ? Mathf.Clamp01((float)elapsedTicks / flightTicks) : 1f;
        }

        /// <summary>
        /// 던진 위치와 착탄점을 이은 선분 위로 arcHeight만큼 솟는 포물선에서 진행도의 위치를 구한다. 수평은 진행도에 비례해 일정하게 나아가고, 진행도 0.5에서 가장 높다.
        /// </summary>
        /// <param name="launchPosition">던진 위치(월드)</param>
        /// <param name="targetPosition">착탄점(월드)</param>
        /// <param name="arcHeight">선분 위로 솟는 최고 높이(유닛)</param>
        /// <param name="progress">진행도(0~1)</param>
        /// <returns>위치(월드)</returns>
        public static Vector3 GetArcPosition(Vector3 launchPosition, Vector3 targetPosition, float arcHeight, float progress)
        {
            return Vector3.Lerp(launchPosition, targetPosition, progress) + Vector3.up * (arcHeight * 4f * progress * (1f - progress));
        }

        /// <summary>
        /// from에서 to로 한 틱 움직일 때 환경 레이어에 닿는지 투사체 반지름의 구로 검사한다. 결과만 돌려주고 위치 데이터는 부르는 System이 고친다.
        /// </summary>
        /// <param name="projectile">투사체 값</param>
        /// <param name="from">이번 틱 시작 위치(월드)</param>
        /// <param name="to">이번 틱 목표 위치(월드)</param>
        /// <param name="stopPosition">닿았으면 닿은 자리, 아니면 to</param>
        /// <returns>닿았으면 true</returns>
        public static bool TrySweep(in ProjectileComponent projectile, Vector3 from, Vector3 to, out Vector3 stopPosition)
        {
            stopPosition = to;
            Vector3 displacement = to - from;
            float distance = displacement.magnitude;
            if (distance <= 0f)
            {
                return false;
            }

            Vector3 direction = displacement / distance;
            if (Physics.SphereCast(from, projectile.radius, direction, out RaycastHit hit, distance, projectile.collisionLayer, QueryTriggerInteraction.Ignore))
            {
                stopPosition = from + direction * hit.distance;
                return true;
            }

            return false;
        }
    }
}
