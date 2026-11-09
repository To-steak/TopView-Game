using TopView.Components;
using UnityEngine;

namespace TopView.Utilities
{
    // 길찾기 목적지를 정하고 지우는 도구.
    public static class EnemyNavigationUtility
    {
        /// <summary>
        /// 새 목적지를 정한다. 경로는 다음 Tick에 다시 구하고, 도착 여부를 지운다.
        /// </summary>
        /// <param name="agent">목적지와 경로</param>
        /// <param name="destination">목적지(월드)</param>
        /// <param name="stoppingDistance">남은 경로가 이 거리 안이면 도착한 것으로 본다(유닛)</param>
        public static void SetDestination(ref NavigationAgentComponent agent, Vector3 destination, float stoppingDistance)
        {
            agent.hasDestination = true;
            agent.destination = destination;
            agent.stoppingDistance = stoppingDistance;
            agent.isPathDirty = true;
            agent.hasReachedDestination = false;
        }

        /// <summary>
        /// 목적지와 경로, 속도를 지워 멈춘다.
        /// </summary>
        /// <param name="agent">목적지와 경로</param>
        public static void StopMove(ref NavigationAgentComponent agent)
        {
            agent.hasDestination = false;
            agent.isPathDirty = false;
            agent.hasReachedDestination = false;
            agent.pathCornerCount = 0;
            agent.pathCornerIndex = 0;
            agent.velocity = Vector3.zero;
        }
    }
}
