using System.Collections.Generic;
using TopView.Components;
using TopView.Requests;
using UnityEngine;

namespace TopView.Systems
{
    // 게임오버와 제한 시간을 처리한다.
    public static class GameRuleSystem
    {
        /// <summary>
        /// 진행 중이면 게임오버 요청을 반영하고 남은 시간을 1틱 줄인다. 진행 중이 아니면 들어온 요청을 버린다.
        /// </summary>
        /// <param name="progress">한 판의 진행 상태</param>
        /// <param name="gameOverRequests">게임오버 요청 큐</param>
        public static void Tick(ref GameProgressComponent progress, Queue<GameOverRequest> gameOverRequests)
        {
            if (progress.phase != GamePhase.Playing)
            {
                if (gameOverRequests.Count > 0)
                {
                    Debug.LogWarning($"GameRuleSystem >> 진행 단계가 {progress.phase}일 때 들어온 게임오버 요청 {gameOverRequests.Count}개를 버렸다.");
                    gameOverRequests.Clear();
                }
                return;
            }

            if (gameOverRequests.Count > 0)
            {
                gameOverRequests.Clear();
                progress.phase = GamePhase.Over;
                return;
            }

            if (progress.remainingTicks > 0)
            {
                progress.remainingTicks--;
                if (progress.remainingTicks == 0)
                {
                    progress.phase = GamePhase.Over;
                }
            }
        }
    }
}
