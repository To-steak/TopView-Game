using System.Collections.Generic;

namespace TopView.Commands
{
    // 입력을 Logic으로 넘기는 명령 버퍼
    public class CommandBuffer
    {
        // 판 흐름 명령. Restart와 Exit는 Runner가 소비한다
        public List<GameFlowCommand> gameFlowCommands = new List<GameFlowCommand>();
        // 플레이어 입력
        public PlayerCommand playerCommand = new PlayerCommand { swapWeaponIndex = -1 };
    }
}
