using System.Collections;
using NUnit.Framework;
using TopView.Components;
using TopView.Entities;
using TopView.Records;
using TopView.Requests;
using UnityEngine;
using UnityEngine.TestTools;

namespace TopView.Tests.PlayMode
{
    // World.Tick의 System 호출 순서에서 생기는 동작을 실제 World로 확인한다.
    public class WorldTickTests
    {
        private WorldTestScene _scene;

        // 테스트 엔티티를 놓을 자리. 씬의 지형과 겹치지 않게 멀리 둔다
        private static readonly Vector3 TEST_ORIGIN = new Vector3(1000f, 1f, 1000f);
        private const float PROJECTILE_SPEED = 50f;
        private const float DAMAGE = 10f;
        // 적이 쫓는 동안 지켜볼 틱 수
        private const int CHASE_TICKS = 20;
        // 경로를 구하지 못한 적이 다시 길을 찾기까지 기다리는 틱. 지켜보는 틱보다 길게 둔다
        private const int NAVIGATION_RETRY_INTERVAL_TICKS = 50;

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
            EntityStorage storage = _scene.Entities;
            float stepDistance = PROJECTILE_SPEED * Time.fixedDeltaTime;
            // 한 틱 이동 구간의 끝 가까이에 적을 둔다
            int enemyIndex = PlaceEnemy(storage, TEST_ORIGIN + Vector3.forward * stepDistance * 0.9f);
            PlaceStraightProjectile(storage, TEST_ORIGIN, 1);

            _scene.world.Tick();

            Assert.AreEqual(1, _scene.EnemyDamageRequests.Count, "수명 마지막 틱의 이동 구간에 있던 적에게 피해 요청이 들어가야 한다.");
            Assert.AreEqual(enemyIndex, _scene.EnemyDamageRequests.Peek().target.index);
            yield break;
        }

        /// <summary>
        /// 판이 끝난 틱에는 새 엔티티가 생기지 않는다. 판 준비 직후의 저장소는 비어 있고, 씬의 생성 요청은 첫 틱의 끝에서 처리된다. 그 첫 틱에 판이 끝나면 아무것도 생기지 않아야 한다.
        /// </summary>
        /// <returns>테스트 코루틴</returns>
        [UnityTest]
        public IEnumerator NothingSpawnsOnTickTheRoundEnds()
        {
            Assert.Greater(_scene.PlayerSpawnRequests.Count, 0, "씬의 플레이어 생성 요청이 있어야 이 테스트가 뜻이 있다.");
            Assert.AreEqual(0, CountEntities(_scene.Entities), "판 준비 직후의 저장소가 비어 있어야 한다.");
            _scene.GameOverRequests.Enqueue(new GameOverRequest { isVictory = false });

            _scene.world.Tick();

            Assert.AreEqual(GamePhase.Lost, _scene.Progress.phase);
            Assert.AreEqual(0, CountEntities(_scene.Entities), "판이 끝난 틱에 엔티티가 새로 생겼다.");
            yield break;
        }

        /// <summary>
        /// 길을 구할 수 없는 곳에서도 적이 Idle과 Walk를 매 틱 오가지 않는다. 테스트 자리(TEST_ORIGIN)에는 NavMesh가 없다.
        /// </summary>
        /// <returns>테스트 코루틴</returns>
        [UnityTest]
        public IEnumerator EnemyDoesNotFlipStatesWithoutPath()
        {
            EntityStorage storage = _scene.Entities;
            GameRuleComponent rule = _scene.Rule;
            rule.navigationRetryIntervalTicks = NAVIGATION_RETRY_INTERVAL_TICKS;
            _scene.Rule = rule;
            PlaceLocatedPlayer(storage, TEST_ORIGIN);
            int enemyIndex = PlaceChasingEnemy(storage, TEST_ORIGIN + Vector3.right * 10f);

            for (int tick = 0; tick < CHASE_TICKS; tick++)
            {
                _scene.world.Tick();
            }

            int stateChangeCount = 0;
            foreach (EnemyStateChangedRecord record in _scene.EnemyStateChangedRecords)
            {
                if (record.enemy.index == enemyIndex)
                {
                    stateChangeCount++;
                }
            }
            Assert.LessOrEqual(stateChangeCount, 2, $"{CHASE_TICKS}틱 동안 적의 상태가 {stateChangeCount}번 바뀌었다.");
            yield break;
        }

        private static int CountEntities(EntityStorage storage)
        {
            int count = 0;
            for (int index = 0; index < storage.entities.Length; index++)
            {
                if (storage.entities[index].mask != ComponentMask.None)
                {
                    count++;
                }
            }
            return count;
        }

        // 적이 쫓을 위치(PlayerLocationSystem이 읽는 것)만 가진 플레이어를 놓는다. 플레이어 System은 이 칸을 움직이지 않는다.
        private static int PlaceLocatedPlayer(EntityStorage storage, Vector3 position)
        {
            int index = WorldTestScene.TakeFreeIndex(storage.freePlayerIndices);
            storage.entities[index] = new Entity
            {
                mask = ComponentMask.PlayerActionState | ComponentMask.Position | ComponentMask.Body,
                generation = storage.entities[index].generation + 1,
                position = new PositionComponent { previous = position, current = position },
                body = new BodyComponent { radius = 0.5f }
            };
            return index;
        }

        // 공격 없이 플레이어를 쫓기만 하는 적을 Idle로 놓는다.
        private static int PlaceChasingEnemy(EntityStorage storage, Vector3 position)
        {
            int index = WorldTestScene.TakeFreeIndex(storage.freeEnemyIndicesByKind[0]);
            storage.entities[index] = new Entity
            {
                mask = ComponentMask.Position | ComponentMask.Rotation | ComponentMask.EnemyKind | ComponentMask.Health | ComponentMask.Body
                    | ComponentMask.NavigationMovement | ComponentMask.AttackList | ComponentMask.EnemyActionState | ComponentMask.NavigationAgent,
                generation = storage.entities[index].generation + 1,
                position = new PositionComponent { previous = position, current = position },
                rotation = new RotationComponent { previous = Quaternion.identity, current = Quaternion.identity },
                enemyKind = new EnemyKindComponent { kind = 0 },
                health = new HealthComponent { current = 100f, max = 100f },
                body = new BodyComponent { radius = 0.5f },
                navigationMovement = new NavigationMovementComponent { stopDistance = 1f, acceleration = 10f, angularSpeed = 360f },
                attackList = new AttackListComponent { attacks = new EnemyAttackComponent[0] },
                enemyActionState = new EnemyActionStateComponent { state = EnemyState.Idle, attackCooldownRemainingTicks = new int[0] },
                navigationAgent = new NavigationAgentComponent { maxSpeed = 3f, pathCorners = storage.entities[index].navigationAgent.pathCorners }
            };
            return index;
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
