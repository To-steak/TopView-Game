using TopView.Entities;

namespace TopView.Records
{
    // 플레이어의 행동 상태가 바뀌었다
    public struct PlayerStateChangedRecord
    {
        // 상태가 바뀐 플레이어
        public EntityIdentifier player;
        // 바뀌기 전 상태
        public PlayerState previousState;
        // 바뀐 상태
        public PlayerState state;
    }
}
