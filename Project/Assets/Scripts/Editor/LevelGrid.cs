using UnityEngine;

namespace TopView.Editor
{
    /// <summary>
    /// 레벨 격자 칸과 월드 위치 변환. 상태 없음.
    /// 칸 (x, z)는 origin에서 +X로 x칸, +Z로 z칸 떨어진 칸이고, 칸 한 변은 1유닛으로 Tile Scale과 상관없다.
    /// 타일은 칸 (x, z)를 왼쪽 아래 칸으로 삼아 놓인다. 그래서 크기가 달라도 타일 가장자리가 칸 경계에 맞는다.
    /// </summary>
    public static class LevelGrid
    {
        /// <summary>
        /// 칸에 놓는 타일 중심의 월드 위치 (높이는 origin.y).
        /// </summary>
        /// <param name="tileSet">격자 설정</param>
        /// <param name="cell">칸 (x, z)</param>
        /// <returns>타일 중심</returns>
        public static Vector3 GetCellCenter(LevelTileSet tileSet, Vector2Int cell)
        {
            return tileSet.origin + new Vector3(cell.x + (tileSet.tileScale.x - 1f) * 0.5f, 0f, cell.y + (tileSet.tileScale.z - 1f) * 0.5f);
        }

        /// <summary>
        /// 월드 위치에 타일 중심이 가장 가까운 칸 (높이는 무시).
        /// </summary>
        /// <param name="tileSet">격자 설정</param>
        /// <param name="position">월드 위치</param>
        /// <returns>칸 (x, z)</returns>
        public static Vector2Int GetCell(LevelTileSet tileSet, Vector3 position)
        {
            return new Vector2Int(
                Mathf.RoundToInt(position.x - tileSet.origin.x - (tileSet.tileScale.x - 1f) * 0.5f),
                Mathf.RoundToInt(position.z - tileSet.origin.z - (tileSet.tileScale.z - 1f) * 0.5f));
        }
    }
}
