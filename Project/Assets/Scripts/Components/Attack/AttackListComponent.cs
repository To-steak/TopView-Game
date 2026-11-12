namespace TopView.Components
{
    // 엔티티가 고를 수 있는 공격 목록. 생성 때 Definition에서 복사하며 바뀌지 않는다.
    public struct AttackListComponent
    {
        // 공격 목록. 칸을 다시 쓸 때 길이가 같으면 재사용하는 배열이다
        public EnemyAttackComponent[] attacks;
    }
}
