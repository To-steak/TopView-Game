using System;
using UnityEngine;

namespace TopView.Editor
{
    /// <summary>
    /// 방 한 종류를 이루는 타일 묶음. 칠하기 창에서 이름으로 고른다.
    /// </summary>
    [Serializable]
    public struct LevelRoomTiles
    {
        // 칠하기 창의 버튼에 보일 이름 (Path, River 등)
        public string name;
        // 방 안쪽과 길을 채우는 타일 (ground_pathOpen)
        public GameObject openPrefab;
        // 방 테두리와 길 양옆을 채우는 타일 (ground_pathSide)
        public GameObject sidePrefab;
        // 방 네 모서리의 타일 (ground_pathCorner)
        public GameObject cornerPrefab;
        // 테두리와 길이 만나는 곳의 타일 (ground_pathCornerSmall)
        public GameObject innerCornerPrefab;
    }
}
