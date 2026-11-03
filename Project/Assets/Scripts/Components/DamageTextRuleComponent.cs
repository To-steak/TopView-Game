using System;

namespace TopView.Components
{
    // 피해량 글자의 표시 값. 판 시작 때 Definition에서 복사한다.
    [Serializable]
    public struct DamageTextRuleComponent
    {
        // 글자가 나타나서 사라질 때까지의 시간(틱)
        public int lifetimeTicks;
        // 사라지는 동안 글자가 위로 올라가는 거리(글자 RectTransform의 앵커 단위)
        public float riseDistance;
        // 같은 자리에서 겹치지 않도록 시작 위치를 어긋나게 하는 반지름(월드 유닛)
        public float spreadRadius;
    }
}
