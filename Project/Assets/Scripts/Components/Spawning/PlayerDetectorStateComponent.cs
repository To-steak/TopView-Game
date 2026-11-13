namespace TopView.Components
{
    // 플레이어 감지기의 상태. 런타임 값이며 번호가 같은 PlayerDetectorComponent의 상태다.
    public struct PlayerDetectorStateComponent
    {
        // 이미 플레이어를 감지했는지. true면 더 감지하지 않는다
        public bool hasTriggered;
    }
}
