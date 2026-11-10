using System.Collections.Generic;

namespace TopView.Commands
{
    // 입력을 Logic으로 넘기는 명령 버퍼
    public class CommandBuffer
    {
        // 판 흐름 명령. Pause, Resume, OpenUpgrade, CloseUpgrade는 PauseSystem이, 나머지는 Runner가 소비한다
        public List<GameFlowCommand> gameFlowCommands = new List<GameFlowCommand>();
        // 설정 명령. Runner가 소비한다
        public List<SettingsCommand> settingsCommands = new List<SettingsCommand>();
        // 플레이어 강화 명령. PlayerUpgradeSystem이 소비한다
        public List<PlayerUpgradeCommand> playerUpgradeCommands = new List<PlayerUpgradeCommand>();
        // 플레이어 입력
        public PlayerCommand playerCommand = new PlayerCommand { swapWeaponIndex = -1 };
    }
}
