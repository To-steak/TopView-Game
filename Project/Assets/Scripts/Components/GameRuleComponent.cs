using System;
using UnityEngine;

namespace TopView.Components
{
    // 한 판의 규칙 값. 판 시작 때 Definition에서 World로 복사한다.
    [Serializable]
    public struct GameRuleComponent
    {
        // 제한 시간(틱). 0이 되면 게임오버
        public int timeLimitTicks;
        // 스폰 중인 적 종류가 다음 적을 내보내기까지의 간격(틱)
        public int spawnIntervalTicks;
        // 웨이브로 생기는 적의 레벨
        public int enemyLevel;
        // 웨이브로 생기는 보스의 레벨
        public int bossLevel;
        // 플레이어가 생기는 위치(월드)
        public Vector3 playerStartPosition;
        // World의 난수 생성기 시드
        public int randomSeed;
    }
}
