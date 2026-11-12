using TopView.Entities;

namespace TopView.Components
{
    // 런타임 값. 플레이어가 든 무기. 배열의 번호는 무기 칸 번호이고, 든 무기의 값은 weapons의 식별자로 무기 저장소에서 읽는다.
    public struct PlayerWeaponSlotsComponent
    {
        // 무기별 무기 엔티티의 식별자. 생성 때 비워 두고 같은 틱의 WeaponSpawnSystem이 채운다. 무기는 주인이 사라질 때까지 살아 있어 generation을 확인하지 않고 읽는다. 재사용하는 배열이다
        public EntityIdentifier[] weapons;
        // 들고 있는 무기 번호
        public int currentIndex;
        // 교체 상태에서 swapStartTicks에 바꿀 무기 번호
        public int pendingIndex;
    }
}
