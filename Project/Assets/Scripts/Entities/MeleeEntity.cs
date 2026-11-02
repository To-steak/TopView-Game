using TopView.Components;

namespace TopView.Entities
{
    // 근접 판정 하나의 데이터. 공격이 나갈 때 생기고, 수명 동안 범위 안의 대상을 한 번씩 맞힌 뒤 사라진다
    public struct MeleeEntity
    {
        // 살아 있는 칸인지. false면 빈 칸이다
        public bool isAlive;
        // 이 칸이 채워진 횟수
        public int generation;

        // 위치. 판정 원의 중심이며 바뀌지 않는다
        public PositionComponent position;
        // 판정 값과 진행 시간
        public MeleeComponent melee;
        // 이미 맞힌 대상
        public MeleeHitsComponent hits;
        // 공격의 진영과 피해량
        public AttackPayloadComponent payload;
    }
}
