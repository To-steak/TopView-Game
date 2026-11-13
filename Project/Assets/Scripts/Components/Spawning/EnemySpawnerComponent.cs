namespace TopView.Components
{
    // 런타임 값. 적 스포너가 만들 적. 생성 때 정하며 바뀌지 않는다. 적의 EnemyKindComponent와 나눠 두어 적을 고르는 System이 스포너를 적으로 잡지 않는다.
    public struct EnemySpawnerComponent
    {
        // 이 스포너에서 생기는 적 종류 번호
        public int enemyKindIndex;
    }
}
