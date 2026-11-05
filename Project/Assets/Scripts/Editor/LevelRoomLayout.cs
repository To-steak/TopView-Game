using System.Collections.Generic;
using UnityEngine;

namespace TopView.Editor
{
    /// <summary>
    /// 방 모양을 타일 배치 목록으로 바꾼다. 상태 없음.
    /// 안쪽은 pathOpen 하나를, 테두리는 pathSide 하나를 늘려 채우고, 모서리에 pathCorner를 둔다.
    /// 길을 낸 면은 가운데 5칸을 pathCornerSmall, pathOpen 3칸, pathCornerSmall로 채우고, 그 바깥으로 3칸 너비의 길과 양옆의 pathSide를 잇는다.
    /// 칸 번호는 방의 왼쪽 아래 모서리가 (0, 0)이고, 안쪽은 1부터 시작한다. 안쪽 칸 수가 짝수면 길은 +X, +Z 쪽으로 치우친다.
    /// 회전값은 모델의 기본 방향을 따른다. pathSide는 +Z, pathCorner는 -X +Z, pathCornerSmall은 -X -Z 쪽이 풀밭이다.
    /// </summary>
    public static class LevelRoomLayout
    {
        // 길을 낼 수 있는 안쪽의 최소 칸 수 (pathCornerSmall 2칸 + 길 3칸)
        public const int MINIMUM_INTERIOR_SIZE = 5;
        // 길의 최소 칸 수 (0이면 테두리에 입구만 내고, 다른 방에서 뻗어 온 길과 맞닿게 한다)
        public const int MINIMUM_ROAD_LENGTH = 0;

        private const int ROAD_HALF_WIDTH = 1;
        // 길 중심에서 pathCornerSmall까지의 칸 수
        private const int INNER_CORNER_OFFSET = 2;

        /// <summary>
        /// 방 전체의 타일 배치를 목록에 채운다. 방 안쪽 중심이 위치에서 가장 가까운 1유닛 격자에 맞는다.
        /// </summary>
        /// <param name="origin">격자 기준점</param>
        /// <param name="tiles">방을 이루는 타일</param>
        /// <param name="shape">방 모양</param>
        /// <param name="point">방 안쪽 중심을 둘 월드 위치</param>
        /// <param name="pieces">비우고 채울 목록</param>
        public static void Build(Vector3 origin, in LevelRoomTiles tiles, in LevelRoomShape shape, Vector3 point, List<LevelRoomPiece> pieces)
        {
            pieces.Clear();
            int width = shape.interiorWidth;
            int depth = shape.interiorDepth;
            int centerColumn = (width + 2) / 2;
            int centerRow = (depth + 2) / 2;
            int lastColumn = width + 1;
            int lastRow = depth + 1;
            Vector3 offset = origin + new Vector3(Mathf.RoundToInt(point.x - origin.x) - centerColumn, 0f, Mathf.RoundToInt(point.z - origin.z) - centerRow);

            AddPiece(pieces, offset, tiles.openPrefab, 1, width, 1, depth, 0);
            AddPiece(pieces, offset, tiles.cornerPrefab, 0, 0, lastRow, lastRow, 0);
            AddPiece(pieces, offset, tiles.cornerPrefab, lastColumn, lastColumn, lastRow, lastRow, 1);
            AddPiece(pieces, offset, tiles.cornerPrefab, lastColumn, lastColumn, 0, 0, 2);
            AddPiece(pieces, offset, tiles.cornerPrefab, 0, 0, 0, 0, 3);

            AddEdge(pieces, offset, in tiles, true, lastRow, 1, width, centerColumn, shape.hasTopRoad, shape.topRoadLength, 0, 3, 1, 1, 2);
            AddEdge(pieces, offset, in tiles, true, 0, -1, width, centerColumn, shape.hasBottomRoad, shape.bottomRoadLength, 2, 3, 1, 0, 3);
            AddEdge(pieces, offset, in tiles, false, lastColumn, 1, depth, centerRow, shape.hasRightRoad, shape.rightRoadLength, 1, 2, 0, 3, 2);
            AddEdge(pieces, offset, in tiles, false, 0, -1, depth, centerRow, shape.hasLeftRoad, shape.leftRoadLength, 3, 2, 0, 0, 1);
        }

        // 테두리 한 면과 그 면의 길을 채운다. along은 면을 따라가는 칸 번호, across는 면에 수직인 칸 번호다.
        private static void AddEdge(List<LevelRoomPiece> pieces, Vector3 offset, in LevelRoomTiles tiles, bool isHorizontal, int line, int outward, int length, int center, bool hasRoad, int roadLength, int sideTurns, int lowWallTurns, int highWallTurns, int lowInnerTurns, int highInnerTurns)
        {
            if (!hasRoad)
            {
                AddSpan(pieces, offset, tiles.sidePrefab, isHorizontal, 1, length, line, line, sideTurns);
                return;
            }

            int lowInner = center - INNER_CORNER_OFFSET;
            int highInner = center + INNER_CORNER_OFFSET;
            AddSpan(pieces, offset, tiles.sidePrefab, isHorizontal, 1, lowInner - 1, line, line, sideTurns);
            AddSpan(pieces, offset, tiles.sidePrefab, isHorizontal, highInner + 1, length, line, line, sideTurns);
            AddSpan(pieces, offset, tiles.innerCornerPrefab, isHorizontal, lowInner, lowInner, line, line, lowInnerTurns);
            AddSpan(pieces, offset, tiles.innerCornerPrefab, isHorizontal, highInner, highInner, line, line, highInnerTurns);

            int roadEnd = line + outward * roadLength;
            AddSpan(pieces, offset, tiles.openPrefab, isHorizontal, center - ROAD_HALF_WIDTH, center + ROAD_HALF_WIDTH, Mathf.Min(line, roadEnd), Mathf.Max(line, roadEnd), 0);

            if (roadLength == 0)
            {
                return;
            }

            int wallStart = line + outward;
            AddSpan(pieces, offset, tiles.sidePrefab, isHorizontal, lowInner, lowInner, Mathf.Min(wallStart, roadEnd), Mathf.Max(wallStart, roadEnd), lowWallTurns);
            AddSpan(pieces, offset, tiles.sidePrefab, isHorizontal, highInner, highInner, Mathf.Min(wallStart, roadEnd), Mathf.Max(wallStart, roadEnd), highWallTurns);
        }

        private static void AddSpan(List<LevelRoomPiece> pieces, Vector3 offset, GameObject prefab, bool isHorizontal, int alongMinimum, int alongMaximum, int acrossMinimum, int acrossMaximum, int quarterTurns)
        {
            if (isHorizontal)
            {
                AddPiece(pieces, offset, prefab, alongMinimum, alongMaximum, acrossMinimum, acrossMaximum, quarterTurns);
            }
            else
            {
                AddPiece(pieces, offset, prefab, acrossMinimum, acrossMaximum, alongMinimum, alongMaximum, quarterTurns);
            }
        }

        // 칸 범위가 비어 있으면 놓지 않는다 (방이 작으면 테두리 조각이 없다)
        private static void AddPiece(List<LevelRoomPiece> pieces, Vector3 offset, GameObject prefab, int minimumColumn, int maximumColumn, int minimumRow, int maximumRow, int quarterTurns)
        {
            if (maximumColumn < minimumColumn || maximumRow < minimumRow)
            {
                return;
            }

            pieces.Add(new LevelRoomPiece
            {
                prefab = prefab,
                center = offset + new Vector3((minimumColumn + maximumColumn) * 0.5f, 0f, (minimumRow + maximumRow) * 0.5f),
                width = maximumColumn - minimumColumn + 1,
                depth = maximumRow - minimumRow + 1,
                quarterTurns = quarterTurns,
            });
        }
    }
}
