using UnityEngine;

namespace TopView.Components
{
    // 적이 생기는 지점. 판 시작 때 Core가 Spawner Authoring에서 채운다. 적 종류는 이 값을 담은 목록의 번호가 정한다.
    public struct SpawnPointComponent
    {
        // 위치(월드)
        public Vector3 position;
        // 회전(월드)
        public Quaternion rotation;
    }
}
