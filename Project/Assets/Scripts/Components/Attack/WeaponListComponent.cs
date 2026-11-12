namespace TopView.Components
{
    // 플레이어의 무기 목록. 생성 때 Definition에서 복사하며 바뀌지 않는다.
    public struct WeaponListComponent
    {
        // 무기 목록. 칸을 다시 쓸 때 길이가 같으면 재사용하는 배열이다
        public WeaponComponent[] weapons;
    }
}
