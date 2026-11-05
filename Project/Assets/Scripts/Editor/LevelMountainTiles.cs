using System;
using UnityEngine;

namespace TopView.Editor
{
    /// <summary>
    /// 산 한 종류를 이루는 타일 묶음. 칠하기 창에서 이름으로 고른다.
    /// </summary>
    [Serializable]
    public struct LevelMountainTiles
    {
        // 칠하기 창의 버튼에 보일 이름 (Rock, Stone 등)
        public string name;
        // 1층의 네 면 (cliff_large_rock)
        public GameObject lowerSidePrefab;
        // 1층의 네 꼭짓점 (cliff_cornerLarge_rock)
        public GameObject lowerCornerPrefab;
        // 2층의 네 면 (cliff_half_rock)
        public GameObject upperSidePrefab;
        // 2층의 네 꼭짓점 (cliff_halfCorner_rock)
        public GameObject upperCornerPrefab;
        // 2층 위를 덮는 뚜껑 (ground_grass)
        public GameObject capPrefab;
    }
}
