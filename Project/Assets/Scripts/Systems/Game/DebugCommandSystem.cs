using System.Collections.Generic;
using TopView.Commands;
using TopView.Components;
using TopView.Utilities;
using UnityEngine;

namespace TopView.Systems
{
    // 테스트용 디버그 명령을 처리한다.
    public static class DebugCommandSystem
    {
        /// <summary>
        /// 디버그 명령을 모두 소비한다. 일시정지 중(강화 화면 포함)에도 돌도록 PauseSystem 뒤, 진행 조건 밖에서 부른다.
        /// </summary>
        /// <param name="progress">한 판의 진행 상태. 골드를 더한다</param>
        /// <param name="debugCommands">디버그 명령 목록</param>
        public static void Tick(ref GameProgressComponent progress, List<DebugCommand> debugCommands)
        {
            for (int commandIndex = 0; commandIndex < debugCommands.Count; commandIndex++)
            {
                Apply(ref progress, debugCommands[commandIndex]);
            }

            debugCommands.Clear();
        }

        private static void Apply(ref GameProgressComponent progress, in DebugCommand command)
        {
            switch (command.type)
            {
                case DebugCommandType.AddGold:
                    AddGold(ref progress, command.amount);
                    break;
                default:
                    Debug.LogWarning($"DebugCommandSystem >> 처리하지 않는 디버그 명령 {command.type}을 버렸다.");
                    break;
            }
        }

        // int 범위를 넘으면 음수가 되지 않도록 최댓값에서 멈춘다
        private static void AddGold(ref GameProgressComponent progress, int amount)
        {
            if (GoldUtility.Add(ref progress, amount))
            {
                Debug.LogWarning($"DebugCommandSystem >> 골드가 int 최댓값을 넘어 {int.MaxValue}에서 멈췄다.");
            }
        }
    }
}
