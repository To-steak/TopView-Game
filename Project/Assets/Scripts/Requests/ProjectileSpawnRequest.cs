using TopView.Components;
using UnityEngine;

namespace TopView.Requests
{
    // 투사체를 만들어 달라는 요청. 공격이 나갈 때 플레이어·적·보스 System이 넣는다
    public struct ProjectileSpawnRequest
    {
        // 투사체 값
        public ProjectileComponent projectile;
        // 쏜 쪽의 진영. 진영이 다른 대상만 맞힌다
        public Faction faction;
        // 맞을 때의 피해량
        public float damage;
        // 위치(월드)
        public Vector3 position;
        // 회전(월드)
        public Quaternion rotation;
        // 처음 속도(월드, 유닛/초)
        public Vector3 velocity;
    }
}
