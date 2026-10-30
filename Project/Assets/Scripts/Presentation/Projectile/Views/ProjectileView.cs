using UnityEngine;

namespace TopView.Presentation.Projectile.Views
{
    // 투사체 프리팹의 참조 묶음
    public class ProjectileView : MonoBehaviour
    {
        // 꼬리 효과. 없으면 null
        public TrailRenderer trail;
        // 앞으로 구르게 돌릴 모델 Transform. 구르지 않으면 null
        public Transform pitchPivot;
    }
}
