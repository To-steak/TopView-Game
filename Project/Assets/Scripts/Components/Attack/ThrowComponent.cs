using System;

namespace TopView.Components
{
    // 투척 무기를 던지기 전에 착탄점을 정하는 값. 던진 뒤의 비행은 ProjectileComponent가 정한다.
    [Serializable]
    public struct ThrowComponent
    {
        // 1틱에 오르는 투척 충전량(%). 1이면 100틱(50Hz에서 2초)에 최대가 된다. 0보다 커야 한다
        public float chargePerTick;
        // 충전량이 100일 때 착탄점까지의 거리(유닛, XZ). 던진 쪽 발 높이에 착탄점을 둔다
        public float maxThrowDistance;
    }
}
