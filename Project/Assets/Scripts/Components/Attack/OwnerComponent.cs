using TopView.Entities;

namespace TopView.Components
{
    // 런타임 값. 이 엔티티를 든 주인.
    public struct OwnerComponent
    {
        // 주인 엔티티. 플레이어 저장소의 식별자다
        public EntityIdentifier owner;
        // 주인의 목록에서 이 엔티티가 놓인 칸 번호
        public int slotIndex;
    }
}
