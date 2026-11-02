using UnityEngine;

namespace TopView.Records
{
    // 투사체가 대상에 맞았거나 폭발했다. View가 피격 이펙트를 재생한다
    public struct ProjectileHitRecord
    {
        // 맞은 위치나 폭발 위치(월드)
        public Vector3 position;
        // 피격 이펙트 표의 번호. 음수면 없음
        public int hitEffectIndex;
    }
}
