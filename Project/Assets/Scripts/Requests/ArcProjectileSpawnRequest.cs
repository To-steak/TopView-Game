using TopView.Components;
using UnityEngine;

namespace TopView.Requests
{
    // 포물선 투사체를 만들어 달라는 요청. 투척 공격이 나갈 때 플레이어 System이 넣는다
    public struct ArcProjectileSpawnRequest
    {
        // 투사체 값
        public ArcProjectileComponent projectile;
        // 투사체를 무엇으로 그릴지
        public ProjectileViewComponent projectileView;
        // 던진 쪽의 진영. 진영이 다른 대상만 맞힌다
        public Faction faction;
        // 맞을 때의 피해량
        public float damage;
        // 던지는 위치(월드)
        public Vector3 position;
        // 회전(월드)
        public Quaternion rotation;
        // 착탄점(월드)
        public Vector3 targetPosition;
    }
}
