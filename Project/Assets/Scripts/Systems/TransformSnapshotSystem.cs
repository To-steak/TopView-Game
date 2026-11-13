using TopView.Components;
using TopView.Entities;

namespace TopView.Systems
{
    // 이번 틱에 위치와 회전이 바뀌기 전의 값을 직전 값으로 남긴다. View 보간과 이동·명중 판정이 읽는다.
    public static class TransformSnapshotSystem
    {
        /// <summary>
        /// 움직이는 엔티티의 직전 값을 남긴다. 위치나 회전을 바꾸는 System보다 먼저 부른다. 근접 판정은 위치가 바뀌지 않아 남기지 않는다.
        /// </summary>
        /// <param name="players">플레이어 저장소</param>
        /// <param name="enemies">적 저장소</param>
        /// <param name="straightProjectiles">직선 투사체 저장소</param>
        /// <param name="arcProjectiles">포물선 투사체 저장소</param>
        public static void Tick(PlayerStorage players, EnemyStorage enemies, StraightProjectileStorage straightProjectiles, ArcProjectileStorage arcProjectiles)
        {
            for (int index = 0; index < players.entities.Length; index++)
            {
                ref PlayerEntity entity = ref players.entities[index];
                if (entity.isAlive)
                {
                    Snapshot(ref entity.position);
                    Snapshot(ref entity.rotation);
                }
            }

            for (int index = 0; index < enemies.entities.Length; index++)
            {
                ref EnemyEntity entity = ref enemies.entities[index];
                if (entity.isAlive)
                {
                    Snapshot(ref entity.position);
                    Snapshot(ref entity.rotation);
                }
            }

            for (int index = 0; index < straightProjectiles.entities.Length; index++)
            {
                ref StraightProjectileEntity entity = ref straightProjectiles.entities[index];
                if (entity.isAlive)
                {
                    Snapshot(ref entity.position);
                    Snapshot(ref entity.rotation);
                }
            }

            for (int index = 0; index < arcProjectiles.entities.Length; index++)
            {
                ref ArcProjectileEntity entity = ref arcProjectiles.entities[index];
                if (entity.isAlive)
                {
                    Snapshot(ref entity.position);
                    Snapshot(ref entity.rotation);
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
