using TopView.Entities;

namespace TopView.Components
{
    // 런타임 값. 무기를 든 주인이 공격 키를 누를 때 들어갈 행동 상태. 판정 방식과 따로 정하며, 판을 준비할 때 Core가 무기 종류로 채운다.
    public struct AttackStateComponent
    {
        // 들어갈 행동 상태. Swing, Shot, ThrowCharge 중 하나다
        public PlayerState state;
    }
}
