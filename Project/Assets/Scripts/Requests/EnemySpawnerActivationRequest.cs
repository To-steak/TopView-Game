namespace TopView.Requests
{
    // 적 스포너 하나를 발동해 달라는 요청. PlayerDetectorSystem이 넣는다.
    public struct EnemySpawnerActivationRequest
    {
        // 발동할 적 스포너의 연결 번호(SpawnerLinkComponent.linkIndex)
        public int spawnerLinkIndex;
    }
}
