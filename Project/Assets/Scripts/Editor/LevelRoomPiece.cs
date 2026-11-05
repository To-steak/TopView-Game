using UnityEngine;

namespace TopView.Editor
{
    /// <summary>
    /// 방, 산, 채우기가 놓는 타일 하나의 배치 정보. 에디터 전용.
    /// </summary>
    public struct LevelRoomPiece
    {
        // 놓을 프리팹
        public GameObject prefab;
        // 타일 중심의 월드 위치
        public Vector3 center;
        // 월드 기준 X 길이 (월드 유닛)
        public float width;
        // 월드 기준 Z 길이 (월드 유닛)
        public float depth;
        // 모델의 원본 높이에 곱할 배율
        public float heightScale;
        // Y축으로 시계 방향 90도 돌리는 횟수
        public int quarterTurns;
    }
}
