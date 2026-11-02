using TopView.Entities;

namespace TopView.Components
{
    // 런타임 값. 근접 판정이 이미 맞힌 대상. 한 대상을 한 번만 맞히는 데 쓴다.
    public struct MeleeHitsComponent
    {
        // 이미 맞힌 대상. 저장소를 만들 때 할당해 다시 쓰는 배열이며 count까지만 쓴다
        public EntityReference[] targets;
        // 이미 맞힌 대상 수
        public int count;
    }
}
