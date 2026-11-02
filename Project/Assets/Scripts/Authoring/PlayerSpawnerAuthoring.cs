using TopView.Definitions;
using UnityEngine;

namespace TopView.Authoring
{
    // 플레이어가 생기는 지점. 씬에 하나만 둔다. 판 시작 때 Core가 이 Transform의 위치를 읽는다.
    public class PlayerSpawnerAuthoring : MonoBehaviour
    {
        // 이 지점에서 생기는 플레이어
        public PlayerDefinition definition;
    }
}
