using System;
using UnityEngine;

namespace TopView.Components
{
    // 투사체 공격에서 한 발을 내보내는 시점과 위치
    [Serializable]
    public struct ProjectileShotComponent
    {
        // 공격 상태에 들어간 뒤 이 발이 나가기까지의 시간(틱)
        public int startTicks;
        // 이 발이 생기는 위치. 공격하는 쪽의 회전 기준 로컬 위치
        public Vector3 spawnOffset;
    }
}
