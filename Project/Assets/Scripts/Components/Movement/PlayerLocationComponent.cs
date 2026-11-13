using UnityEngine;

namespace TopView.Components
{
    // 이번 틱에 적이 노리는 플레이어의 위치. 런타임 값이며 매 틱 PlayerLocationSystem이 다시 채운다.
    public struct PlayerLocationComponent
    {
        // 살아 있는 플레이어가 있는지. false면 나머지 값은 기본값이다
        public bool hasPlayer;
        // 플레이어 위치(월드)
        public Vector3 position;
        // 플레이어 이동 충돌 캡슐의 반지름. 감지와 공격 범위에 더한다
        public float radius;
    }
}
