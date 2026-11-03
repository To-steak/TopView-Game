using UnityEngine;

namespace TopView.Presentation.Enemy.Views
{
    // 적 프리팹의 참조 묶음
    public class EnemyView : MonoBehaviour
    {
        // 적 모델의 Animator
        public Animator animator;
        // 체력 게이지를 띄울 위치. 루트 바로 아래에 두어 애니메이션을 따라 흔들리지 않게 한다
        public Transform healthGaugeAnchor;
        // 피해량 글자를 띄울 위치. 루트 바로 아래에 두어 애니메이션을 따라 흔들리지 않게 한다
        public Transform damageAnchor;
    }
}
