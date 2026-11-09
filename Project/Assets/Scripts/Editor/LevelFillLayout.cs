using System.Collections.Generic;
using UnityEngine;

namespace TopView.Editor
{
    /// <summary>
    /// 빈 칸을 겹치지 않는 가장 적은 수의 사각형으로 나눈다. 상태 없음.
    /// 안쪽으로 꺾인 모서리마다 자르는 선이 필요하므로, 모서리 둘을 한 번에 해결하는 선을 서로 교차하지 않게 가장 많이 고르면 사각형이 가장 적다.
    /// 교차는 가로 선과 세로 선 사이에만 생겨, 이분 매칭과 쾨니그 정리로 그 선들을 정확히 고른다.
    /// 번호 규칙: 칸 (x, z)의 네 꼭짓점은 격자점 (x, z)부터 (x + 1, z + 1)까지다.
    /// </summary>
    public static class LevelFillLayout
    {
        /// <summary>
        /// 빈 칸을 나눈 사각형을 목록에 채운다.
        /// </summary>
        /// <param name="isEmptyCells">칸마다 빈 칸인지 나타내는 배열 ([x, z])</param>
        /// <param name="rectangles">비우고 채울 목록 (칸 번호 기준)</param>
        public static void Build(bool[,] isEmptyCells, List<RectInt> rectangles)
        {
            rectangles.Clear();
            int width = isEmptyCells.GetLength(0);
            int depth = isEmptyCells.GetLength(1);
            // [x, z]는 격자선 x에서 z번째 줄을 지나는 자른 선이다 (x - 1번 칸과 x번 칸 사이)
            bool[,] verticalCuts = new bool[width + 1, depth];
            // [x, z]는 격자선 z에서 x번째 칸을 지나는 자른 선이다 (z - 1번 칸과 z번 칸 사이)
            bool[,] horizontalCuts = new bool[width, depth + 1];
            bool[,] isResolvedCorners = new bool[width + 1, depth + 1];

            List<LevelFillChord> horizontalChords = new List<LevelFillChord>();
            List<LevelFillChord> verticalChords = new List<LevelFillChord>();
            CollectChords(isEmptyCells, horizontalChords, verticalChords);

            bool[] isHorizontalKept = new bool[horizontalChords.Count];
            bool[] isVerticalKept = new bool[verticalChords.Count];
            SelectChords(horizontalChords, verticalChords, isHorizontalKept, isVerticalKept);

            ApplyHorizontalChords(horizontalChords, isHorizontalKept, horizontalCuts, isResolvedCorners);
            ApplyVerticalChords(verticalChords, isVerticalKept, verticalCuts, isResolvedCorners);
            CutRemainingCorners(isEmptyCells, isResolvedCorners, verticalCuts, horizontalCuts);
            ExtractRectangles(isEmptyCells, verticalCuts, horizontalCuts, rectangles);
        }

        private static bool IsEmpty(bool[,] isEmptyCells, int column, int row)
        {
            return column >= 0 && column < isEmptyCells.GetLength(0)
                && row >= 0 && row < isEmptyCells.GetLength(1)
                && isEmptyCells[column, row];
        }

        // 둘러싼 네 칸 중 셋만 비면 안쪽으로 꺾인 모서리다. 두 칸이 대각선으로만 비면 각 칸의 볼록한 모서리라 해당하지 않는다.
        private static bool IsConcaveCorner(bool[,] isEmptyCells, int x, int z)
        {
            int emptyCount = 0;
            emptyCount += IsEmpty(isEmptyCells, x - 1, z - 1) ? 1 : 0;
            emptyCount += IsEmpty(isEmptyCells, x, z - 1) ? 1 : 0;
            emptyCount += IsEmpty(isEmptyCells, x - 1, z) ? 1 : 0;
            emptyCount += IsEmpty(isEmptyCells, x, z) ? 1 : 0;
            return emptyCount == 3;
        }

        private static void CollectChords(bool[,] isEmptyCells, List<LevelFillChord> horizontalChords, List<LevelFillChord> verticalChords)
        {
            int width = isEmptyCells.GetLength(0);
            int depth = isEmptyCells.GetLength(1);
            for (int z = 1; z < depth; z++)
            {
                for (int x = 1; x < width; x++)
                {
                    if (!IsConcaveCorner(isEmptyCells, x, z))
                    {
                        continue;
                    }

                    int endX = x;
                    while (IsEmpty(isEmptyCells, endX, z - 1) && IsEmpty(isEmptyCells, endX, z))
                    {
                        endX++;
                    }
                    if (endX > x && IsConcaveCorner(isEmptyCells, endX, z))
                    {
                        horizontalChords.Add(new LevelFillChord { line = z, start = x, end = endX });
                    }

                    int endZ = z;
                    while (IsEmpty(isEmptyCells, x - 1, endZ) && IsEmpty(isEmptyCells, x, endZ))
                    {
                        endZ++;
                    }
                    if (endZ > z && IsConcaveCorner(isEmptyCells, x, endZ))
                    {
                        verticalChords.Add(new LevelFillChord { line = x, start = z, end = endZ });
                    }
                }
            }
        }

        // 서로 만나지 않는 선을 가장 많이 고른다. 끝점을 함께 쓰는 선도 만나는 것으로 본다 (모서리 하나는 선 하나면 해결된다).
        private static void SelectChords(List<LevelFillChord> horizontalChords, List<LevelFillChord> verticalChords, bool[] isHorizontalKept, bool[] isVerticalKept)
        {
            List<int>[] crossings = new List<int>[horizontalChords.Count];
            for (int horizontalIndex = 0; horizontalIndex < horizontalChords.Count; horizontalIndex++)
            {
                crossings[horizontalIndex] = new List<int>();
                LevelFillChord horizontal = horizontalChords[horizontalIndex];
                for (int verticalIndex = 0; verticalIndex < verticalChords.Count; verticalIndex++)
                {
                    LevelFillChord vertical = verticalChords[verticalIndex];
                    if (horizontal.start <= vertical.line && vertical.line <= horizontal.end
                        && vertical.start <= horizontal.line && horizontal.line <= vertical.end)
                    {
                        crossings[horizontalIndex].Add(verticalIndex);
                    }
                }
            }

            int[] matchOfHorizontal = new int[horizontalChords.Count];
            int[] matchOfVertical = new int[verticalChords.Count];
            for (int index = 0; index < matchOfHorizontal.Length; index++)
            {
                matchOfHorizontal[index] = -1;
            }
            for (int index = 0; index < matchOfVertical.Length; index++)
            {
                matchOfVertical[index] = -1;
            }

            for (int horizontalIndex = 0; horizontalIndex < horizontalChords.Count; horizontalIndex++)
            {
                bool[] isVisited = new bool[verticalChords.Count];
                TryMatch(horizontalIndex, crossings, isVisited, matchOfHorizontal, matchOfVertical);
            }

            // 쾨니그 정리: 짝 없는 가로 선에서 번갈아 닿는 가로 선과, 닿지 않는 세로 선이 최대 독립 집합이다
            bool[] isVerticalReached = new bool[verticalChords.Count];
            for (int horizontalIndex = 0; horizontalIndex < horizontalChords.Count; horizontalIndex++)
            {
                if (matchOfHorizontal[horizontalIndex] < 0 && !isHorizontalKept[horizontalIndex])
                {
                    Reach(horizontalIndex, crossings, matchOfVertical, isHorizontalKept, isVerticalReached);
                }
            }
            for (int verticalIndex = 0; verticalIndex < verticalChords.Count; verticalIndex++)
            {
                isVerticalKept[verticalIndex] = !isVerticalReached[verticalIndex];
            }
        }

        private static bool TryMatch(int horizontalIndex, List<int>[] crossings, bool[] isVisited, int[] matchOfHorizontal, int[] matchOfVertical)
        {
            List<int> candidates = crossings[horizontalIndex];
            for (int candidateIndex = 0; candidateIndex < candidates.Count; candidateIndex++)
            {
                int verticalIndex = candidates[candidateIndex];
                if (isVisited[verticalIndex])
                {
                    continue;
                }
                isVisited[verticalIndex] = true;

                int currentMatch = matchOfVertical[verticalIndex];
                if (currentMatch < 0 || TryMatch(currentMatch, crossings, isVisited, matchOfHorizontal, matchOfVertical))
                {
                    matchOfHorizontal[horizontalIndex] = verticalIndex;
                    matchOfVertical[verticalIndex] = horizontalIndex;
                    return true;
                }
            }
            return false;
        }

        private static void Reach(int horizontalIndex, List<int>[] crossings, int[] matchOfVertical, bool[] isHorizontalReached, bool[] isVerticalReached)
        {
            isHorizontalReached[horizontalIndex] = true;
            List<int> candidates = crossings[horizontalIndex];
            for (int candidateIndex = 0; candidateIndex < candidates.Count; candidateIndex++)
            {
                int verticalIndex = candidates[candidateIndex];
                if (isVerticalReached[verticalIndex])
                {
                    continue;
                }
                isVerticalReached[verticalIndex] = true;

                int match = matchOfVertical[verticalIndex];
                if (match >= 0 && !isHorizontalReached[match])
                {
                    Reach(match, crossings, matchOfVertical, isHorizontalReached, isVerticalReached);
                }
            }
        }

        private static void ApplyHorizontalChords(List<LevelFillChord> chords, bool[] isKept, bool[,] horizontalCuts, bool[,] isResolvedCorners)
        {
            for (int chordIndex = 0; chordIndex < chords.Count; chordIndex++)
            {
                if (!isKept[chordIndex])
                {
                    continue;
                }

                LevelFillChord chord = chords[chordIndex];
                for (int column = chord.start; column < chord.end; column++)
                {
                    horizontalCuts[column, chord.line] = true;
                }
                isResolvedCorners[chord.start, chord.line] = true;
                isResolvedCorners[chord.end, chord.line] = true;
            }
        }

        private static void ApplyVerticalChords(List<LevelFillChord> chords, bool[] isKept, bool[,] verticalCuts, bool[,] isResolvedCorners)
        {
            for (int chordIndex = 0; chordIndex < chords.Count; chordIndex++)
            {
                if (!isKept[chordIndex])
                {
                    continue;
                }

                LevelFillChord chord = chords[chordIndex];
                for (int row = chord.start; row < chord.end; row++)
                {
                    verticalCuts[chord.line, row] = true;
                }
                isResolvedCorners[chord.line, chord.start] = true;
                isResolvedCorners[chord.line, chord.end] = true;
            }
        }

        // 고른 선으로 해결되지 않은 모서리마다, 막힌 칸의 반대쪽으로 벽이나 세로로 자른 선을 만날 때까지 가로로 자른다.
        private static void CutRemainingCorners(bool[,] isEmptyCells, bool[,] isResolvedCorners, bool[,] verticalCuts, bool[,] horizontalCuts)
        {
            int width = isEmptyCells.GetLength(0);
            int depth = isEmptyCells.GetLength(1);
            for (int z = 1; z < depth; z++)
            {
                for (int x = 1; x < width; x++)
                {
                    if (isResolvedCorners[x, z] || !IsConcaveCorner(isEmptyCells, x, z))
                    {
                        continue;
                    }

                    bool isBlockedOnLeft = !IsEmpty(isEmptyCells, x - 1, z - 1) || !IsEmpty(isEmptyCells, x - 1, z);
                    int direction = isBlockedOnLeft ? 1 : -1;
                    int currentX = x;
                    while (true)
                    {
                        int column = direction > 0 ? currentX : currentX - 1;
                        if (!IsEmpty(isEmptyCells, column, z - 1) || !IsEmpty(isEmptyCells, column, z))
                        {
                            break;
                        }

                        horizontalCuts[column, z] = true;
                        currentX += direction;
                        if (verticalCuts[currentX, z - 1] || verticalCuts[currentX, z])
                        {
                            break;
                        }
                    }
                }
            }
        }

        // 왼쪽 아래 칸부터 자른 선과 찬 칸에 막힐 때까지 사각형을 넓혀 꺼낸다.
        private static void ExtractRectangles(bool[,] isEmptyCells, bool[,] verticalCuts, bool[,] horizontalCuts, List<RectInt> rectangles)
        {
            int width = isEmptyCells.GetLength(0);
            int depth = isEmptyCells.GetLength(1);
            bool[,] isTaken = new bool[width, depth];
            for (int row = 0; row < depth; row++)
            {
                for (int column = 0; column < width; column++)
                {
                    if (!isEmptyCells[column, row] || isTaken[column, row])
                    {
                        continue;
                    }

                    int rectangleWidth = 1;
                    while (column + rectangleWidth < width
                        && isEmptyCells[column + rectangleWidth, row]
                        && !isTaken[column + rectangleWidth, row]
                        && !verticalCuts[column + rectangleWidth, row])
                    {
                        rectangleWidth++;
                    }

                    int rectangleDepth = 1;
                    while (row + rectangleDepth < depth && CanExtendRow(isEmptyCells, isTaken, verticalCuts, horizontalCuts, column, rectangleWidth, row + rectangleDepth))
                    {
                        rectangleDepth++;
                    }

                    for (int takenRow = row; takenRow < row + rectangleDepth; takenRow++)
                    {
                        for (int takenColumn = column; takenColumn < column + rectangleWidth; takenColumn++)
                        {
                            isTaken[takenColumn, takenRow] = true;
                        }
                    }
                    rectangles.Add(new RectInt(column, row, rectangleWidth, rectangleDepth));
                }
            }
        }

        private static bool CanExtendRow(bool[,] isEmptyCells, bool[,] isTaken, bool[,] verticalCuts, bool[,] horizontalCuts, int startColumn, int columnCount, int row)
        {
            for (int column = startColumn; column < startColumn + columnCount; column++)
            {
                if (!isEmptyCells[column, row] || isTaken[column, row] || horizontalCuts[column, row])
                {
                    return false;
                }
                if (column > startColumn && verticalCuts[column, row])
                {
                    return false;
                }
            }
            return true;
        }
    }
}
