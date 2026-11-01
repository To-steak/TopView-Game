using TopView.Commands;

namespace TopView.Systems
{
    // 명령 버퍼의 플레이어 입력을 다룬다.
    public static class CommandSystem
    {
        /// <summary>
        /// 플레이어 입력 중 누름 입력을 지운다. 이동, 달리기, 조준처럼 계속 유지되는 입력은 남긴다. 틱 마지막에 부른다.
        /// </summary>
        /// <param name="playerCommand">플레이어 입력</param>
        public static void ClearPlayerPresses(ref PlayerCommand playerCommand)
        {
            playerCommand.isJumpPressed = false;
            playerCommand.isDodgePressed = false;
            playerCommand.isAttackPressed = false;
            playerCommand.isReloadPressed = false;
            playerCommand.swapWeaponIndex = -1;
        }
    }
}
