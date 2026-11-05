using UnityEngine;

namespace TopView.Editor
{
    /// <summary>
    /// 레벨 칠하기 설정. 격자(칸 크기·원점), 칠할 타일 프리팹, 흩뿌릴 소품 프리팹을 담는다. 에디터 전용이며 게임 데이터가 아니다.
    /// 격자 계산은 LevelGrid가 맡는다.
    /// </summary>
    [CreateAssetMenu(fileName = "Level Tile Set", menuName = "TopView/Level Tile Set")]
    public class LevelTileSet : ScriptableObject
    {
        [Header("Grid")]
        // 격자 기준점. 이 위치가 칸 (0, 0)의 중심이고, 높이는 칠하는 바닥면이다
        public Vector3 origin;
        // 타일 프리팹에 축별로 곱할 크기. x, z는 타일의 가로, 세로 길이이기도 하다 (월드 유닛, 타일 원본의 한 변이 1유닛이라 둘이 같다). y는 두께에만 쓴다
        public Vector3 tileScale = Vector3.one;

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

        [Header("Room")]
        // 방 종류 목록. 칠하기 창에서 이 중 하나를 골라 방을 놓는다
        public LevelRoomTiles[] roomTiles = new LevelRoomTiles[0];

        [Header("Mountain")]
        // 산 종류 목록. 칠하기 창에서 이 중 하나를 골라 산을 놓는다
        public LevelMountainTiles[] mountainTiles = new LevelMountainTiles[0];

        [Header("Options")]
        // 칠한 타일과 소품을 Batching Static으로 표시할지
        public bool markTilesStatic = true;

        private const float MINIMUM_TILE_SCALE = 0.01f;

        // 크기가 0이면 타일이 보이지 않고 겹침 판정에도 걸리지 않는다
        private void OnValidate()
        {
            tileScale = new Vector3(
                Mathf.Max(tileScale.x, MINIMUM_TILE_SCALE),
                Mathf.Max(tileScale.y, MINIMUM_TILE_SCALE),
                Mathf.Max(tileScale.z, MINIMUM_TILE_SCALE));
        }
    }
}
