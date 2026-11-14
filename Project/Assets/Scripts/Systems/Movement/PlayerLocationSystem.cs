using TopView.Components;
using TopView.Entities;

namespace TopView.Systems
{
    // 적 System이 읽을 플레이어의 위치를 기록한다.
    public static class PlayerLocationSystem
    {
        // 처리할 플레이어가 가진 컴포넌트
        private const ComponentMask PLAYER_MASK = ComponentMask.PlayerActionState | ComponentMask.Position | ComponentMask.Body;

        /// <summary>
        /// 살아 있는(Die가 아닌) 플레이어의 위치와 반지름을 기록한다. 없으면 기본값으로 비운다. 플레이어 이동이 끝난 뒤, 적 System보다 먼저 부른다.
        /// </summary>
        /// <param name="storage">엔티티 저장소. 플레이어(PlayerActionState, Position, Body)만 처리한다</param>
        /// <param name="playerLocation">기록할 플레이어 위치</param>
        public static void Tick(EntityStorage storage, ref PlayerLocationComponent playerLocation)
        {
            playerLocation = default;

            for (int index = 0; index < storage.entities.Length; index++)
            {
                ref readonly Entity entity = ref storage.entities[index];
                // 사망한 플레이어는 적이 쫓지 않는다. Die는 체력이 0이 된 틱에 PlayerDeathSystem이 넣는다
                if ((entity.mask & PLAYER_MASK) != PLAYER_MASK || entity.playerActionState.state == PlayerState.Die)
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
