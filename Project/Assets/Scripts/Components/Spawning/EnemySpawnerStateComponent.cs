namespace TopView.Components
{
    // 런타임 값. 적 스포너의 발동과 생성 진행.
    public struct EnemySpawnerStateComponent
    {
        // 발동되어 적을 내보내는 중인지
        public bool isActive;
        // 다음 적을 내보내기까지 남은 시간(틱)
        public int remainingTicks;
        // 지금까지 내보낸 적의 수. 0보다 크면 이미 발동한 스포너다
        public int spawnedCount;
    }
}
