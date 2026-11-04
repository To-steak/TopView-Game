using System;

namespace TopView.Editor
{
    /// <summary>
    /// 레벨 칠하기 창의 방 찍기 모드에서 고르는 방 모양. 에디터 전용.
    /// </summary>
    [Serializable]
    public struct LevelRoomShape
    {
        // 테두리를 뺀 안쪽의 X 칸 수 (길을 가운데에 두려면 홀수)
        public int interiorWidth;
        // 테두리를 뺀 안쪽의 Z 칸 수 (길을 가운데에 두려면 홀수)
        public int interiorDepth;
        // +Z 면에 길을 낼지
        public bool hasTopRoad;
        // +Z 면의 길이 테두리 바깥으로 나가는 칸 수
        public int topRoadLength;
        // -Z 면에 길을 낼지
        public bool hasBottomRoad;
        // -Z 면의 길이 테두리 바깥으로 나가는 칸 수
        public int bottomRoadLength;
        // -X 면에 길을 낼지
        public bool hasLeftRoad;
        // -X 면의 길이 테두리 바깥으로 나가는 칸 수
        public int leftRoadLength;
        // +X 면에 길을 낼지
        public bool hasRightRoad;
        // +X 면의 길이 테두리 바깥으로 나가는 칸 수
        public int rightRoadLength;
    }
}
