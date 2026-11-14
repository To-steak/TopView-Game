using System.Collections.Generic;
using NUnit.Framework;
using TopView.Components;
using TopView.Entities;
using TopView.Records;
using TopView.Requests;
using TopView.Systems;
using UnityEngine;

namespace TopView.Tests
{
    // 직선 투사체가 이동 구간에서 가장 먼저 닿은 대상을 맞히는지 확인한다.
    public class ProjectileHitSystemTests
    {
        private EntityStorage _storage;
        private HitTargetComponent[] _playerHitTargets;
        private HitTargetComponent[] _enemyHitTargets;
        private int _enemyHitTargetCount;
        private Queue<DamageRequest> _playerDamageRequests;
        private Queue<DamageRequest> _enemyDamageRequests;
        private List<ProjectileHitRecord> _hitRecords;
        private Queue<ProjectileDespawnRequest> _despawnRequests;

        private const int PROJECTILE_INDEX = 0;
        // 한 틱 이동 거리(유닛). 투사체는 (0, 1, 0)에서 (0, 1, 2)로 +Z를 따라 움직인다
        private const float STEP_DISTANCE = 2f;
        private const float PROJECTILE_RADIUS = 0.1f;
        // 피격 대상 목록에 넣는 적의 칸 번호. 피해 요청의 대상으로만 쓴다
        private const int BIG_ENEMY_INDEX = 10;
        private const int SMALL_ENEMY_INDEX = 11;

        /// <summary>
        /// 직선 투사체 칸이 하나 있는 저장소와 빈 피격 대상 목록, 빈 큐를 만든다.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            _storage = new EntityStorage(0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0);
            _playerHitTargets = new HitTargetComponent[0];
            _enemyHitTargets = new HitTargetComponent[2];
            _enemyHitTargetCount = 0;
            _playerDamageRequests = new Queue<DamageRequest>();
            _enemyDamageRequests = new Queue<DamageRequest>();
            _hitRecords = new List<ProjectileHitRecord>();
            _despawnRequests = new Queue<ProjectileDespawnRequest>();
            PlaceProjectile();
        }

        /// <summary>
        /// 구간 위의 대상 하나는 맞힌다.
        /// </summary>
        [Test]
        public void HitsTargetOnSegment()
        {
            AddEnemyTarget(BIG_ENEMY_INDEX, new Vector3(0f, 0f, 1f), 0.3f);

            Tick();

            Assert.AreEqual(1, _enemyDamageRequests.Count);
            Assert.AreEqual(BIG_ENEMY_INDEX, _enemyDamageRequests.Peek().target.index);
        }

        /// <summary>
        /// 원에 먼저 들어가는 대상을 맞힌다. 큰 적 A는 구간 7할 지점 옆(선에서 1)에 있어 구간 앞쪽에서 원에 들어가고,
        /// 작은 적 B는 구간 5할 지점 위에 있어 그보다 늦게 들어간다. 원에서 가장 가까운 점의 비율로 고르면 B를 고른다.
        /// </summary>
        [Test]
        public void HitsTargetEnteredFirst()
        {
            AddEnemyTarget(BIG_ENEMY_INDEX, new Vector3(1f, 0f, STEP_DISTANCE * 0.7f), 1.5f);
            AddEnemyTarget(SMALL_ENEMY_INDEX, new Vector3(0f, 0f, STEP_DISTANCE * 0.5f), 0.3f);

            Tick();

            Assert.AreEqual(1, _enemyDamageRequests.Count);
            Assert.AreEqual(BIG_ENEMY_INDEX, _enemyDamageRequests.Peek().target.index, "원에 먼저 들어간 큰 적이 아니라 뒤의 작은 적을 맞혔다.");
        }

        private void Tick()
        {
            ProjectileHitSystem.Tick(_storage, _playerHitTargets, 0, _enemyHitTargets, _enemyHitTargetCount, _playerDamageRequests, _enemyDamageRequests, _hitRecords, _despawnRequests);
        }

        private void PlaceProjectile()
        {
            ref Entity projectile = ref _storage.entities[PROJECTILE_INDEX];
            projectile.mask = EntityArchetype.STRAIGHT_PROJECTILE;
            projectile.generation = 1;
            projectile.position = new PositionComponent { previous = new Vector3(0f, 1f, 0f), current = new Vector3(0f, 1f, STEP_DISTANCE) };
            projectile.projectileHit = new ProjectileHitComponent { radius = PROJECTILE_RADIUS };
            projectile.projectileView = new ProjectileViewComponent { viewIndex = -1, hitEffectIndex = -1 };
            projectile.payload = new AttackPayloadComponent { faction = Faction.Player, damage = 10f };
            projectile.straightFlight = new StraightFlightComponent { lifetimeTicks = 100 };
            projectile.straightMotion = new StraightProjectileMotionComponent { elapsedTicks = 1 };
        }

        private void AddEnemyTarget(int index, Vector3 position, float radius)
        {
            _enemyHitTargets[_enemyHitTargetCount] = new HitTargetComponent
            {
                target = new EntityIdentifier { index = index, generation = 1 },
                position = position,
                radius = radius,
                bottomHeight = float.NegativeInfinity,
                topHeight = float.PositiveInfinity
            };
            _enemyHitTargetCount++;
        }
    }
}
