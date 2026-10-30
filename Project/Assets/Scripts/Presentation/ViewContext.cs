using System.Collections.Generic;
using TopView.Entities;
using TopView.Presentation.Boss.Views;
using TopView.Presentation.Camera.Views;
using TopView.Presentation.Enemy.Views;
using TopView.Presentation.Player.Views;
using TopView.Presentation.Projectile.Views;
using UnityEngine;

namespace TopView.Presentation
{
    // View 쪽 실행 상태. 판마다 새로 만든다.
    public class ViewContext
    {
        // 화면을 그리는 카메라
        public UnityEngine.Camera mainCamera;
        // 플레이어를 따라가는 카메라. 씬에 없으면 null
        public PlayerCameraView playerCameraView;
        // 플레이어 카메라가 따라갈 대상을 이미 정했는지
        public bool isPlayerCameraFollowing;

        // 플레이어 View. 아직 만들지 않았으면 null
        public PlayerView playerView;
        // playerView가 보여 주는 플레이어
        public EntityIdentifier playerViewEntity;
        // playerView의 무기 소켓에 붙인 무기 모델. 번호는 무기 번호와 같다
        public GameObject[] playerWeaponModels;

        // 적 저장소 칸별 View. 빈 칸은 null
        public EnemyView[] enemyViews;
        // 적 저장소 칸별 View가 보여 주는 엔티티의 generation
        public int[] enemyViewGenerations;
        // 적 종류별로 쉬고 있는 View
        public Stack<EnemyView>[] enemyViewPools;
        // 적 종류별 Animator 트리거 파라미터. 그 종류의 View를 처음 만들 때 채운다
        public int[][] enemyTriggerHashesByKind;

        // 보스 View. 아직 만들지 않았으면 null
        public BossView bossView;
        // bossView가 보여 주는 보스
        public EntityIdentifier bossViewEntity;
        // 보스 Animator 트리거 파라미터. 보스 View를 처음 만들 때 채운다
        public int[] bossTriggerHashes;

        // 투사체 저장소 칸별 View. 빈 칸은 null
        public ProjectileView[] projectileViews;
        // 투사체 저장소 칸별 View가 보여 주는 엔티티의 generation
        public int[] projectileViewGenerations;
        // 투사체 저장소 칸별 View가 속한 투사체 View 표의 번호
        public int[] projectileViewIndices;
        // 투사체 View 표의 번호별로 쉬고 있는 View
        public Stack<ProjectileView>[] projectileViewPools;
        // 피격 이펙트 표의 번호별로 쉬고 있는 ParticleSystem
        public Stack<ParticleSystem>[] hitEffectPools;
        // 재생 중인 피격 이펙트
        public List<ParticleSystem> playingHitEffects = new List<ParticleSystem>();
        // playingHitEffects와 같은 순서로, 각 이펙트가 속한 피격 이펙트 표의 번호
        public List<int> playingHitEffectIndices = new List<int>();
    }
}
