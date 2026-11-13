using TopView.Entities;

namespace TopView.Requests
{
    // 투사체를 없애라는 요청. 직선·포물선 투사체가 큐 하나를 함께 쓰며, 투사체 System들이 넣고 바로 뒤의 ProjectileDespawnSystem이 처리한다
    public struct ProjectileDespawnRequest
    {
        // 없앨 투사체
        public EntityIdentifier target;
    }
}
