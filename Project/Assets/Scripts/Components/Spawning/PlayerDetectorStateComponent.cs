namespace TopView.Components
{
    // 런타임 값. 플레이어 감지기의 감지 여부.
    public struct PlayerDetectorStateComponent
    {
        // 이미 플레이어를 감지했는지. true면 더 감지하지 않는다
        public bool hasTriggered;
    }
}
