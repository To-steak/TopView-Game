using System.Collections.Generic;
using UnityEngine;

namespace TopView.Editor
{
    /// <summary>
    /// 산 모양을 타일 배치 목록으로 바꾼다. 상태 없음.
    /// 1층과 2층은 네 면에 벽 하나씩을 늘려 놓고 네 꼭짓점에 모서리를 둔다. 뚜껑은 안쪽 칸을 덮도록 하나를 늘려 2층 위에 놓는다.
    /// 벽과 모서리의 두께는 BORDER_THICKNESS 칸이다. 벽은 길이 × 높이 × 두께, 모서리는 두께 × 높이 × 두께로 늘린다.
    /// 칸 번호는 산의 왼쪽 아래 꼭짓점이 (0, 0)이고, 안쪽은 BORDER_THICKNESS부터 시작한다.
    /// 회전값은 모델의 기본 방향을 따른다. 벽은 +Z, 모서리는 +X +Z 쪽이 산의 바깥이다.
    /// </summary>
    public static class LevelMountainLayout
    {
        // 안쪽의 최소 칸 수
        public const int MINIMUM_INTERIOR_SIZE = 1;
        // 층의 최소 높이 (월드 유닛)
        public const float MINIMUM_HEIGHT = 0.1f;

        // 벽과 모서리의 두께 (칸)
        private const int BORDER_THICKNESS = 3;
        // 1층 모델의 원본 높이 (월드 유닛)
        private const float LOWER_MODEL_HEIGHT = 1f;
        // 2층 모델의 원본 높이 (월드 유닛)
        private const float UPPER_MODEL_HEIGHT = 0.5f;

        /// <summary>
        /// 2층 바닥의 월드 Y 높이를 구한다.
        /// </summary>
        /// <param name="shape">산 모양</param>
        /// <returns>2층 바닥의 월드 Y 높이</returns>
        public static float GetUpperStartHeight(in LevelMountainShape shape)
        {
            return shape.startHeight + shape.lowerHeight;
        }

        /// <summary>
        /// 뚜껑의 월드 Y 높이를 구한다.
        /// </summary>
        /// <param name="shape">산 모양</param>
        /// <returns>뚜껑의 월드 Y 높이</returns>
        public static float GetCapHeight(in LevelMountainShape shape)
        {
            return GetUpperStartHeight(in shape) + shape.upperHeight;
        }

        /// <summary>
        /// 1층 모델에 줄 Y Scale을 구한다.
        /// </summary>
        /// <param name="shape">산 모양</param>
        /// <returns>1층 모델의 Y Scale</returns>
        public static float GetLowerHeightScale(in LevelMountainShape shape)
        {
            return shape.lowerHeight / LOWER_MODEL_HEIGHT;
        }

        /// <summary>
        /// 2층 모델에 줄 Y Scale을 구한다.
        /// </summary>
        /// <param name="shape">산 모양</param>
        /// <returns>2층 모델의 Y Scale</returns>
        public static float GetUpperHeightScale(in LevelMountainShape shape)
        {
            return shape.upperHeight / UPPER_MODEL_HEIGHT;
        }

        /// <summary>
        /// 산 전체의 타일 배치를 목록에 채운다. 산 안쪽 중심이 위치에서 가장 가까운 1유닛 격자에 맞는다.
        /// </summary>
        /// <param name="origin">격자 기준점</param>
        /// <param name="tiles">산을 이루는 타일</param>
        /// <param name="shape">산 모양</param>
        /// <param name="point">산 안쪽 중심을 둘 월드 위치 (높이는 쓰지 않는다)</param>
        /// <param name="pieces">비우고 채울 목록</param>
        public static void Build(Vector3 origin, in LevelMountainTiles tiles, in LevelMountainShape shape, Vector3 point, List<LevelRoomPiece> pieces)
        {
            pieces.Clear();
            int width = shape.interiorWidth;
            int depth = shape.interiorDepth;
            int centerColumn = BORDER_THICKNESS + width / 2;
            int centerRow = BORDER_THICKNESS + depth / 2;
            float offsetX = origin.x + Mathf.RoundToInt(point.x - origin.x) - centerColumn;
            float offsetZ = origin.z + Mathf.RoundToInt(point.z - origin.z) - centerRow;
            Vector3 offset = new Vector3(offsetX, 0f, offsetZ);

            float upperStartHeight = GetUpperStartHeight(in shape);
            float capHeight = GetCapHeight(in shape);

            AddRing(pieces, offset, tiles.lowerSidePrefab, tiles.lowerCornerPrefab, width, depth, shape.startHeight, GetLowerHeightScale(in shape));
            AddRing(pieces, offset, tiles.upperSidePrefab, tiles.upperCornerPrefab, width, depth, upperStartHeight, GetUpperHeightScale(in shape));
            AddPiece(pieces, offset, tiles.capPrefab, BORDER_THICKNESS, BORDER_THICKNESS + width - 1, BORDER_THICKNESS, BORDER_THICKNESS + depth - 1, capHeight, 1f, 0);
        }

        // 한 층의 네 면과 네 꼭짓점을 채운다.
        private static void AddRing(List<LevelRoomPiece> pieces, Vector3 offset, GameObject sidePrefab, GameObject cornerPrefab, int width, int depth, float height, float heightScale)
        {
            int lastBorder = BORDER_THICKNESS - 1;
            int lastInteriorColumn = BORDER_THICKNESS + width - 1;
            int lastInteriorRow = BORDER_THICKNESS + depth - 1;
            int firstOuterColumn = lastInteriorColumn + 1;
            int firstOuterRow = lastInteriorRow + 1;
            int lastOuterColumn = firstOuterColumn + lastBorder;
            int lastOuterRow = firstOuterRow + lastBorder;

            AddPiece(pieces, offset, sidePrefab, BORDER_THICKNESS, lastInteriorColumn, firstOuterRow, lastOuterRow, height, heightScale, 0);
            AddPiece(pieces, offset, sidePrefab, firstOuterColumn, lastOuterColumn, BORDER_THICKNESS, lastInteriorRow, height, heightScale, 1);
            AddPiece(pieces, offset, sidePrefab, BORDER_THICKNESS, lastInteriorColumn, 0, lastBorder, height, heightScale, 2);
            AddPiece(pieces, offset, sidePrefab, 0, lastBorder, BORDER_THICKNESS, lastInteriorRow, height, heightScale, 3);

            AddPiece(pieces, offset, cornerPrefab, firstOuterColumn, lastOuterColumn, firstOuterRow, lastOuterRow, height, heightScale, 0);
            AddPiece(pieces, offset, cornerPrefab, firstOuterColumn, lastOuterColumn, 0, lastBorder, height, heightScale, 1);
            AddPiece(pieces, offset, cornerPrefab, 0, lastBorder, 0, lastBorder, height, heightScale, 2);
            AddPiece(pieces, offset, cornerPrefab, 0, lastBorder, firstOuterRow, lastOuterRow, height, heightScale, 3);
        }

        private static void AddPiece(List<LevelRoomPiece> pieces, Vector3 offset, GameObject prefab, int minimumColumn, int maximumColumn, int minimumRow, int maximumRow, float height, float heightScale, int quarterTurns)
        {
            pieces.Add(new LevelRoomPiece
            {
                prefab = prefab,
                center = offset + new Vector3((minimumColumn + maximumColumn) * 0.5f, height, (minimumRow + maximumRow) * 0.5f),
                width = maximumColumn - minimumColumn + 1,
                depth = maximumRow - minimumRow + 1,
                heightScale = heightScale,
                quarterTurns = quarterTurns,
            });
        }
    }
}
