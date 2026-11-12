using TopView.Entities;

namespace TopView.Components
{
    // 런타임 값. 플레이어가 든 무기와 무기별 탄약. 번호는 WeaponListComponent의 무기 목록과 같다.
    public struct PlayerWeaponSlotsComponent
    {
        // 무기별 무기 엔티티의 식별자. 생성 때 비워 두고 WeaponSpawnSystem이 채운다. 재사용하는 배열이다
        public EntityIdentifier[] weapons;
        // 무기별 장전된 탄약 수. 재사용하는 배열이다
        public int[] currentAmmo;
        // 무기별 예비 탄약 수. 재사용하는 배열이다
        public int[] reserveAmmo;
        // 들고 있는 무기 번호
        public int currentIndex;
        // 교체 상태에서 swapStartTicks에 바꿀 무기 번호
        public int pendingIndex;
    }
}
