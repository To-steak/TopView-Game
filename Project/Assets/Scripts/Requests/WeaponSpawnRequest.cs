using TopView.Components;
using TopView.Entities;

namespace TopView.Requests
{
    // 무기를 만들어 달라는 요청. 플레이어가 생길 때 PlayerSpawnSystem이 무기마다 넣는다
    public struct WeaponSpawnRequest
    {
        // 이 무기를 들 주인
        public EntityIdentifier owner;
        // 주인의 무기 목록에서 이 무기가 놓일 칸 번호
        public int slotIndex;
        // 무기 값
        public WeaponComponent weapon;
    }
}
