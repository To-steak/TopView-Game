using UnityEngine;

namespace TopView.Components
{
    // 적이 생기는 지점. 판 시작 때 Core가 Spawner Authoring에서 채운다.
    public struct SpawnPointComponent
    {
        // 이 지점에서 생기는 적 종류 번호
        public int enemyKindIndex;
        // 위치(월드)
        public Vector3 position;
        // 회전(월드)
        public Quaternion rotation;
    }
}
