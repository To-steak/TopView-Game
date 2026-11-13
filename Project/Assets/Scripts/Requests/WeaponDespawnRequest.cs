using TopView.Entities;

namespace TopView.Requests
{
    // 무기를 없애라는 요청. WeaponOwnerDeathSystem이 넣고, 바로 뒤의 WeaponDespawnSystem이 처리한다
    public struct WeaponDespawnRequest
    {
        // 없앨 무기
        public EntityIdentifier target;
    }
}
