#if UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Requests;

namespace TopView.Core
{
    // PlayMode 테스트가 World의 데이터를 읽고 쓰는 통로. 테스트가 상황을 만들고 World.Tick을 직접 불러 결과를 본다.
    // 테스트 어셈블리가 있을 때만 컴파일되어 빌드에는 들어가지 않는다.
    public partial class World
    {
        // 판이 준비되었는지. false면 아래 통로를 쓰지 않는다
        public bool IsTestRoundReady => _isRoundReady;
        // 엔티티 저장소
        public EntityStorage TestEntities => _entities;
        // 적 피해 요청 큐. 플레이어의 공격이 넣는다
        public Queue<DamageRequest> TestEnemyDamageRequests => _enemyDamageRequests;

        // 한 판의 진행 상태
        public ref GameProgressComponent TestProgress => ref _progress;
    }
}
#endif
