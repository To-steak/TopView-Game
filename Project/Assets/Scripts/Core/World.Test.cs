#if UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Records;
using TopView.Requests;

namespace TopView.Core
{
    // PlayMode 테스트가 World의 데이터를 읽고 쓰는 통로. 테스트가 상황을 만들고 World.Tick을 직접 불러 결과를 본다.
    // 테스트 어셈블리가 있을 때만 컴파일되어 빌드에는 들어가지 않는다.
    public partial class World
    {
        // 판이 준비되었는지. false면 아래 통로를 쓰지 않는다
        public bool IsTestRoundReady => _isRoundReady;
        // Game Settings Definition에 크로스헤어 커서 텍스처가 연결되어 있는지
        public bool IsTestCrosshairAssigned => _gameSettingsDefinition.crosshairCursor != null;
        // 지금 OS 커서에 크로스헤어를 걸어 두었는지. App이 LateUpdate에서 부르는 Present가 바꾼다
        public bool IsTestCrosshairApplied => _viewContext.cursor.isCrosshairApplied;
        // 엔티티 저장소
        public EntityStorage TestEntities => _entities;
        // 적 피해 요청 큐. 플레이어의 공격이 넣는다
        public Queue<DamageRequest> TestEnemyDamageRequests => _enemyDamageRequests;
        // 게임오버 요청 큐
        public Queue<GameOverRequest> TestGameOverRequests => _gameOverRequests;
        // 플레이어 생성 요청 큐. 판을 준비할 때 Core가 씬의 플레이어를 넣는다
        public Queue<PlayerSpawnRequest> TestPlayerSpawnRequests => _playerSpawnRequests;
        // 이번 프레임의 적 상태 변경 기록. Present 마지막에 비워지므로 같은 프레임 안에서 읽는다
        public List<EnemyStateChangedRecord> TestEnemyStateChangedRecords => _enemyStateChangedRecords;

        // 한 판의 규칙 값. 판을 준비할 때 Definition에서 복사한 값이라, 테스트가 Definition 수치에 기대지 않으려면 여기서 바꾼다
        public ref GameRuleComponent TestRule => ref _rule;
        // 한 판의 진행 상태
        public ref GameProgressComponent TestProgress => ref _progress;
    }
}
#endif
