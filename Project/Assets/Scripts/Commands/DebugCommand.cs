namespace TopView.Commands
{
    // 테스트용 디버그 명령. 에디터에서 World의 Context Menu가 넣고 DebugCommandSystem이 소비한다.
    public struct DebugCommand
    {
        // 명령 종류
        public DebugCommandType type;
        // AddGold일 때 더할 골드. 다른 명령은 쓰지 않는다
        public int amount;
    }
}
