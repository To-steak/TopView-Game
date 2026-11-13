using TopView.Components;
using TopView.Entities;

namespace TopView.Systems
{
    // 적 System이 읽을 플레이어의 위치를 기록한다.
    public static class PlayerLocationSystem
    {
        /// <summary>
        /// 살아 있는 플레이어의 위치와 반지름을 기록한다. 없으면 기본값으로 비운다. 플레이어 이동이 끝난 뒤, 적 System보다 먼저 부른다.
        /// </summary>
        /// <param name="players">플레이어 저장소</param>
        /// <param name="playerLocation">기록할 플레이어 위치</param>
        public static void Tick(PlayerStorage players, ref PlayerLocationComponent playerLocation)
        {
            playerLocation = default;

            for (int index = 0; index < players.entities.Length; index++)
            {
                ref readonly PlayerEntity entity = ref players.entities[index];
                if (!entity.isAlive)
                {
                    continue;
                }

                playerLocation.hasPlayer = true;
                playerLocation.position = entity.position.current;
                playerLocation.radius = entity.body.radius;
                return;
            }
        }
    }
}
