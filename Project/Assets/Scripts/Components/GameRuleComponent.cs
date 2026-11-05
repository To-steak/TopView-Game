using System;

namespace TopView.Components
{
    // 한 판의 규칙 값. 판 시작 때 Definition에서 World로 복사한다.
    [Serializable]
    public struct GameRuleComponent
    {
        // 제한 시간(틱). 0이 되면 게임오버
        public int timeLimitTicks;
        // 스포너에서 생기는 적의 레벨
        public int enemyLevel;
        // World의 난수 생성기 시드
        public int randomSeed;
        // 적의 목적지와 생성 위치를 NavMesh 위로 맞출 때 찾는 최대 거리(유닛)
        public float navMeshSampleDistance;
    }
}
