using System.Collections;
using NUnit.Framework;
using TopView.Components;
using TopView.Entities;
using UnityEngine;
using UnityEngine.TestTools;

namespace TopView.PlayModeTests
{
    // World.Tick의 System 호출 순서에서 생기는 동작을 실제 World로 확인한다.
    public class WorldTickTests
    {
        private WorldTestScene _scene;

        // 테스트 엔티티를 놓을 자리. 씬의 지형과 겹치지 않게 멀리 둔다
        private static readonly Vector3 TEST_ORIGIN = new Vector3(1000f, 1f, 1000f);
        private const float PROJECTILE_SPEED = 50f;
        private const float DAMAGE = 10f;

        /// <summary>
        /// 틱을 멈춘 채 World 씬을 불러와 판을 준비한다.
        /// </summary>
        /// <returns>판이 준비되면 끝나는 코루틴</returns>
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            _scene = new WorldTestScene();
            yield return _scene.Load();
        }

        /// <summary>
        /// 멈춘 틱을 되돌린다.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            _scene.Unload();
        }

        /// <summary>
        /// 수명의 마지막 틱에 움직인 구간에 있는 적도 직선 투사체에 맞는다. 수명이 L틱이면 L번 움직이고 L번 모두 명중 판정을 받아야 한다.
        /// </summary>
        /// <returns>테스트 코루틴</returns>
        [UnityTest]
        public IEnumerator StraightProjectileHitsOnLastLifetimeTick()
        {
            EntityStorage storage = _scene.world.TestEntities;
            float stepDistance = PROJECTILE_SPEED * Time.fixedDeltaTime;
            // 한 틱 이동 구간의 끝 가까이에 적을 둔다
            int enemyIndex = PlaceEnemy(storage, TEST_ORIGIN + Vector3.forward * stepDistance * 0.9f);
            PlaceStraightProjectile(storage, TEST_ORIGIN, 1);

            _scene.world.Tick();

            Assert.AreEqual(1, _scene.world.TestEnemyDamageRequests.Count, "수명 마지막 틱의 이동 구간에 있던 적에게 피해 요청이 들어가야 한다.");
            Assert.AreEqual(enemyIndex, _scene.world.TestEnemyDamageRequests.Peek().target.index);
            yield break;
        }

        // 피격 대상이 되는 데 필요한 컴포넌트만 가진 적을 놓는다. 행동 상태가 없어 적 System은 움직이지 않는다.
        private static int PlaceEnemy(EntityStorage storage, Vector3 position)
        {
            int index = WorldTestScene.TakeFreeIndex(storage.freeEnemyIndicesByKind[0]);
            storage.entities[index] = new Entity
            {
                mask = ComponentMask.EnemyKind | ComponentMask.Health | ComponentMask.Position | ComponentMask.Body,
                generation = storage.entities[index].generation + 1,
                enemyKind = new EnemyKindComponent { kind = 0 },
                health = new HealthComponent { current = 100f, max = 100f },
                position = new PositionComponent { previous = position, current = position },
                body = new BodyComponent { radius = 0.5f },
                navigationAgent = storage.entities[index].navigationAgent
            };
            return index;
        }

        // 앞으로 나는 플레이어의 직선 투사체를 놓는다. 환경 레이어를 비워 지형에 막히지 않는다.
        private static int PlaceStraightProjectile(EntityStorage storage, Vector3 position, int lifetimeTicks)
        {
            int index = WorldTestScene.TakeFreeIndex(storage.freeStraightProjectileIndices);
            storage.entities[index] = new Entity
            {
                mask = EntityArchetype.STRAIGHT_PROJECTILE,
                generation = storage.entities[index].generation + 1,
                position = new PositionComponent { previous = position, current = position },
                rotation = new RotationComponent { previous = Quaternion.identity, current = Quaternion.identity },
                projectileHit = new ProjectileHitComponent { radius = 0.1f },
                projectileView = new ProjectileViewComponent { viewIndex = -1, hitEffectIndex = -1 },
                payload = new AttackPayloadComponent { faction = Faction.Player, damage = DAMAGE },
                straightFlight = new StraightFlightComponent { speed = PROJECTILE_SPEED, lifetimeTicks = lifetimeTicks },
                straightMotion = new StraightProjectileMotionComponent { velocity = Vector3.forward * PROJECTILE_SPEED }
            };
            return index;
        }
    }
}
