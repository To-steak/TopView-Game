using TopView.Components;

namespace TopView.Entities
{
    // 무기 하나의 데이터. 주인이 생긴 틱에 함께 생기고, 주인이 사라지면 사라진다
    public struct WeaponEntity
    {
        // 살아 있는 칸인지. false면 빈 칸이다
        public bool isAlive;
        // 이 칸이 채워진 횟수
        public int generation;

        // 이 무기를 든 주인
        public OwnerComponent owner;
        // 무기 값. 생성 때 Definition에서 복사한다
        public WeaponComponent weapon;
    }
}
