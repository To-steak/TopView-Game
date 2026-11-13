namespace TopView.Components
{
    // 적 스포너의 상태. 런타임 값이며 번호가 같은 EnemySpawnerComponent의 상태다.
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
