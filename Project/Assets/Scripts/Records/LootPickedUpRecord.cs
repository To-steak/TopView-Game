using TopView.Components;
using UnityEngine;

namespace TopView.Records
{
    // 플레이어가 전리품을 획득했다. View가 획득 글자를 띄운다
    public struct LootPickedUpRecord
    {
        // 획득한 전리품 종류
        public LootKind kind;
        // 획득한 양
        public int amount;
        // 획득한 전리품의 위치(월드)
        public Vector3 position;
    }
}
