using TopView.Components;
using UnityEngine;

namespace TopView.Requests
{
    // 전리품 하나를 만들어 달라는 요청. 적의 사망 연출이 끝날 때 EnemyDeathEndSystem이 넣는다
    public struct LootSpawnRequest
    {
        // 전리품 종류
        public LootKind kind;
        // 획득할 때 얻는 양
        public int amount;
        // 위치(월드)
        public Vector3 position;
    }
}
