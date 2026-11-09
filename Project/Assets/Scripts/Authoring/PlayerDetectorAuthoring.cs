using TopView.Components;
using UnityEngine;

namespace TopView.Authoring
{
    // 플레이어를 감지해 연결된 적 스포너를 발동하는 감지기. 판 시작 때 Core가 이 Transform의 위치를 읽는다.
    public class PlayerDetectorAuthoring : MonoBehaviour
    {
        // 감지 범위
        public PlayerDetectorRangeComponent range;
        // 플레이어를 감지하면 발동할 적 스포너. 감지 범위 밖에 있어도 된다
        public EnemySpawnerAuthoring[] spawners;
    }
}
