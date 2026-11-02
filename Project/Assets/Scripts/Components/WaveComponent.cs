using System;
using UnityEngine;

namespace TopView.Components
{
    // 웨이브 하나. 시작 시각이 되면 적 종류의 스폰을 켜거나 보스를 한 번 내보낸다.
    [Serializable]
    public struct WaveComponent
    {
        // 판 시작부터 이 웨이브가 시작되기까지의 시간(틱)
        public int startTicks;
        // true면 보스를 내보내고, false면 enemyKindIndex 적의 스폰을 켠다
        public bool isBoss;
        // 스폰을 켤 적 종류의 번호. 판 시작 때 Core가 Game Settings Definition의 웨이브 적 목록에서 채운다. isBoss가 true면 쓰지 않는다
        [HideInInspector]
        public int enemyKindIndex;
    }
}
