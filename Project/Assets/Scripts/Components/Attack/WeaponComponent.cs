using System;

namespace TopView.Components
{
    // 모든 무기에 있는 값. 판정 방식, 탄약, 공격 시간은 무기마다 가진 컴포넌트가 정한다.
    [Serializable]
    public struct WeaponComponent
    {
        // 한 번 맞을 때의 피해량
        public float damage;
        // 조준 평면의 높이. 플레이어 발 높이에서 잰다(유닛)
        public float aimHeight;
    }
}
