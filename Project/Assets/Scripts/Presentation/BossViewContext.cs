using TopView.Entities;
using TopView.Presentation.Boss.Views;

namespace TopView.Presentation
{
    // 보스 View 실행 상태. 판마다 새로 만든다.
    public class BossViewContext
    {
        // 보스 View. 아직 만들지 않았으면 null
        public BossView view;
        // view가 보여 주는 보스
        public EntityIdentifier viewEntity;
        // 보스 Animator 트리거 파라미터. 보스 View를 처음 만들 때 채운다
        public int[] triggerHashes;
    }
}
