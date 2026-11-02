namespace TopView.Entities
{
    // 엔티티가 담긴 저장소의 종류. EntityReference가 어느 저장소의 칸인지 가린다
    public enum EntityKind
    {
        // PlayerStorage
        Player,
        // EnemyStorage
        Enemy,
        // BossStorage
        Boss
    }
}
