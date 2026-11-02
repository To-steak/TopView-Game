using TopView.Components;
using TopView.Entities;

namespace TopView.Systems
{
    // 이번 틱에 위치와 회전이 바뀌기 전의 값을 직전 값으로 남긴다. View 보간과 이동·명중 판정이 읽는다.
    public static class TransformSnapshotSystem
    {
        /// <summary>
        /// 살아 있는 플레이어, 적과 공유 저장소 엔티티의 직전 값을 남긴다. 위치나 회전을 바꾸는 System보다 먼저 부른다.
        /// </summary>
        /// <param name="players">플레이어 저장소</param>
        /// <param name="enemies">적 저장소</param>
        /// <param name="masks">공유 저장소의 칸별 컴포넌트 표시</param>
        /// <param name="positions">공유 저장소의 위치</param>
        /// <param name="rotations">공유 저장소의 회전</param>
        public static void Tick(PlayerStorage players, EnemyStorage enemies, ComponentMask[] masks, PositionComponent[] positions, RotationComponent[] rotations)
        {
            for (int playerIndex = 0; playerIndex < players.entities.Length; playerIndex++)
            {
                ref PlayerEntity player = ref players.entities[playerIndex];
                if (player.isAlive)
                {
                    Snapshot(ref player.position);
                    Snapshot(ref player.rotation);
                }
            }

            for (int enemyIndex = 0; enemyIndex < enemies.entities.Length; enemyIndex++)
            {
                ref EnemyEntity enemy = ref enemies.entities[enemyIndex];
                if (enemy.isAlive)
                {
                    Snapshot(ref enemy.position);
                    Snapshot(ref enemy.rotation);
                }
            }

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
