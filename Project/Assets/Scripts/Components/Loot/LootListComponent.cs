namespace TopView.Components
{
    // 런타임 값. 적의 드롭 표. 생성 때 Definition에서 복사하며 바뀌지 않는다.
    public struct LootListComponent
    {
        // 드롭 표. 칸을 다시 쓸 때 길이가 같으면 재사용하는 배열이다
        public EnemyLootComponent[] loots;
    }
}
