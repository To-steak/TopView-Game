using UnityEngine;

namespace TopView.Editor
{
    /// <summary>
    /// 레벨 칠하기 설정. 격자(칸 크기·원점), 칠할 타일 프리팹, 흩뿌릴 소품 프리팹을 담는다. 에디터 전용이며 게임 데이터가 아니다.
    /// 칸 (x, z)의 중심 = origin + (x × tileScale, 0, z × tileScale).
    /// </summary>
    [CreateAssetMenu(fileName = "Level Tile Set", menuName = "TopView/Level Tile Set")]
    public class LevelTileSet : ScriptableObject
    {
        [Header("Grid")]
        // 격자 기준점. 이 위치가 칸 (0, 0)의 중심이고, 높이는 칠하는 바닥면이다
        public Vector3 origin;
        // 타일 프리팹에 곱할 크기이자 칸 한 변의 길이 (월드 유닛, 타일 원본의 한 변이 1유닛이라 둘이 같다)
        [Min(0.01f)] public float tileScale = 1f;

        [Header("Tiles")]
        // 칠할 타일 프리팹 (FBX 모델도 된다). Project 창에서 여러 개를 한꺼번에 끌어 넣을 수 있다
        public GameObject[] prefabs = new GameObject[0];

        [Header("Props")]
        // 소품 프리팹 (나무, 바위 등). FBX 모델도 된다
        public GameObject[] propPrefabs = new GameObject[0];
        // 소품 프리팹에 곱할 크기의 최솟값
        [Min(0.01f)] public float propMinimumScale = 0.8f;
        // 소품 프리팹에 곱할 크기의 최댓값
        [Min(0.01f)] public float propMaximumScale = 1.2f;
        // 소품 사이의 최소 거리 (월드 유닛, XZ 평면). 이보다 가까우면 놓지 않는다
        [Min(0.01f)] public float propSpacing = 1f;
        // 큰 브러시로 한 번 찍을 때 소품을 놓아 보는 횟수 (간격에 걸리면 실제로 놓이는 수는 줄어든다)
        [Min(1)] public int propScatterCount = 5;

        [Header("Options")]
        // 칠한 타일과 소품을 Batching Static으로 표시할지
        public bool markTilesStatic = true;
    }
}
