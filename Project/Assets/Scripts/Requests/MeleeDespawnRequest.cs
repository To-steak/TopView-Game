using TopView.Entities;

namespace TopView.Requests
{
    // 근접 판정을 없애라는 요청. 근접 판정 System들이 넣고, 바로 뒤의 MeleeDespawnSystem이 처리한다
    public struct MeleeDespawnRequest
    {
        // 없앨 근접 판정
        public EntityIdentifier target;
    }
}
