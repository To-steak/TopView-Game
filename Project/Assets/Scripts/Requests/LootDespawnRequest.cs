using TopView.Entities;

namespace TopView.Requests
{
    // 전리품을 없애라는 요청. 전리품 System들이 넣고, 바로 뒤의 LootDespawnSystem이 처리한다
    public struct LootDespawnRequest
    {
        // 없앨 전리품
        public EntityIdentifier target;
    }
}
