using System.Collections.Generic;

namespace TopView.Commands
{
    // 입력을 Logic으로 넘기는 명령 버퍼
    public class CommandBuffer
    {
        // 일시정지 명령. PauseSystem이 소비한다
        public List<PauseCommand> pauseCommands = new();
        // 플레이어 탈출 명령. PlayerUnstuckSystem이 소비한다
        public List<PlayerUnstuckCommand> playerUnstuckCommands = new();
        // 플레이어 강화 명령. PlayerUpgradeSystem이 소비한다
        public List<PlayerUpgradeCommand> playerUpgradeCommands = new();
        // 디버그 명령. DebugCommandSystem이 소비한다
        public List<DebugCommand> debugCommands = new();
        // 플레이어 입력
        public PlayerCommand playerCommand = new() { swapWeaponIndex = -1 };
    }
}
