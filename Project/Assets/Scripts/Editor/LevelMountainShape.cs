using System;

namespace TopView.Editor
{
    /// <summary>
    /// 레벨 칠하기 창의 산 모드에서 고르는 산 모양. 에디터 전용.
    /// </summary>
    [Serializable]
    public struct LevelMountainShape
    {
        // 벽을 뺀 안쪽의 X 칸 수
        public int interiorWidth;
        // 벽을 뺀 안쪽의 Z 칸 수
        public int interiorDepth;
        // 1층 바닥의 월드 Y 높이
        public float startHeight;
        // 1층의 높이 (월드 유닛)
        public float lowerHeight;
        // 2층의 높이 (월드 유닛). 2층은 1층 바로 위에서 시작한다
        public float upperHeight;
    }
}
