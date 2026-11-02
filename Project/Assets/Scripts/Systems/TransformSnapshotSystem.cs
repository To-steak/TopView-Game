using TopView.Components;
using TopView.Entities;

namespace TopView.Systems
{
    // 이번 틱에 위치와 회전이 바뀌기 전의 값을 직전 값으로 남긴다. View 보간과 이동·명중 판정이 읽는다.
    public static class TransformSnapshotSystem
    {
        /// <summary>
        /// 위치와 회전을 가진 엔티티의 직전 값을 남긴다. 위치나 회전을 바꾸는 System보다 먼저 부른다.
        /// </summary>
        /// <param name="masks">칸별 컴포넌트 표시</param>
        /// <param name="positions">위치</param>
        /// <param name="rotations">회전</param>
        public static void Tick(ComponentMask[] masks, PositionComponent[] positions, RotationComponent[] rotations)
        {
            for (int index = 0; index < masks.Length; index++)
            {
                if ((masks[index] & ComponentMask.Position) != 0)
                {
                    Snapshot(ref positions[index]);
                }
                if ((masks[index] & ComponentMask.Rotation) != 0)
                {
                    Snapshot(ref rotations[index]);
                }
            }
        }

        private static void Snapshot(ref PositionComponent position)
        {
            position.previous = position.current;
        }

        private static void Snapshot(ref RotationComponent rotation)
        {
            rotation.previous = rotation.current;
        }
    }
}
