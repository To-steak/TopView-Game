using NUnit.Framework;
using TopView.Components;
using TopView.Entities;
using TopView.Systems;
using TopView.Utilities;
using UnityEngine;

namespace TopView.Tests
{
    // 적에게서 플레이어를 밀어낼 때 지형(벽) 속으로 밀어 넣지 않는지 확인한다.
    // 적은 원점에, 벽은 x = WALL_FACE_X부터 +X 쪽에 둔다. 플레이어는 벽면을 따라(벽에 캡슐이 닿은 채) 적 쪽으로 다가간다.
    public class PlayerBlockSystemTests
    {
        private EntityStorage _storage;
        private GameObject _wall;

        // 저장소의 칸 번호. 구간 순서(적, 플레이어)대로 칸을 하나씩 둔다
        private const int ENEMY_INDEX = 0;
        private const int PLAYER_INDEX = 1;
        private const float CAPSULE_RADIUS = 0.5f;
        private const float ENEMY_RADIUS = 0.5f;
        // 벽면의 X 좌표. 플레이어 캡슐 중심이 x = 0.1이면 캡슐 가장자리가 벽면에 닿는다
        private const float WALL_FACE_X = 0.6f;
        private const float WALL_SIZE = 10f;
        // 벽이 있는 레이어. 기본 레이어(0)다
        private const int WALL_LAYER_MASK = 1;

        /// <summary>
        /// 적과 플레이어 구간에 칸이 하나씩 있는 저장소와, 기본 레이어에 벽을 만든다.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            _storage = new EntityStorage(0, 0, 0, 0, 0, 1, 1, 0, 1, 0, 0, 0);
            _wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _wall.transform.position = new Vector3(WALL_FACE_X + WALL_SIZE * 0.5f, 0f, 0f);
            _wall.transform.localScale = Vector3.one * WALL_SIZE;
            Physics.SyncTransforms();

            ref Entity enemy = ref _storage.entities[ENEMY_INDEX];
            enemy.mask = ComponentMask.EnemyKind | ComponentMask.Position | ComponentMask.Body;
            enemy.generation = 1;
            enemy.position = new PositionComponent { previous = Vector3.zero, current = Vector3.zero };
            enemy.body = new BodyComponent { radius = ENEMY_RADIUS };
        }

        /// <summary>
        /// 테스트가 만든 벽을 지운다.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_wall);
        }

        /// <summary>
        /// 벽이 없는 쪽으로 다가가면 적의 원 밖으로 밀어낸다.
        /// </summary>
        [Test]
        public void PushesPlayerOutOfEnemyCircle()
        {
            PlacePlayer(new Vector3(-0.6f, 0f, 0f), new Vector3(-0.3f, 0f, 0f));

            PlayerBlockSystem.Tick(_storage);

            Vector3 position = _storage.entities[PLAYER_INDEX].position.current;
            Assert.AreEqual(CAPSULE_RADIUS + ENEMY_RADIUS, new Vector2(position.x, position.z).magnitude, 0.0001f);
        }

        /// <summary>
        /// 벽면을 따라 적 쪽으로 다가가도, 밀어낸 자리에서 캡슐이 벽과 겹치지 않는다.
        /// 겹치면 다음 이동의 CapsuleCast가 이미 겹친 벽을 보지 못해 벽을 통과할 수 있다.
        /// </summary>
        [Test]
        public void DoesNotPushPlayerIntoWall()
        {
            PlacePlayer(new Vector3(0.1f, 0f, 0.6f), new Vector3(0.1f, 0f, 0.3f));

            PlayerBlockSystem.Tick(_storage);

            ref readonly Entity player = ref _storage.entities[PLAYER_INDEX];
            PlayerMovementUtility.GetCapsulePoints(in player.playerMovement, player.rotation.current, player.position.current, out Vector3 top, out Vector3 bottom);
            // 캡슐이 벽면에 딱 닿은 것은 겹친 것으로 보지 않도록 반지름을 조금 줄여 본다
            bool isOverlapping = Physics.CheckCapsule(top, bottom, CAPSULE_RADIUS - 0.01f, WALL_LAYER_MASK, QueryTriggerInteraction.Ignore);
            Assert.IsFalse(isOverlapping, $"밀어낸 자리 {player.position.current}에서 플레이어 캡슐이 벽과 겹친다.");
        }

        private void PlacePlayer(Vector3 previous, Vector3 current)
        {
            ref Entity player = ref _storage.entities[PLAYER_INDEX];
            player.mask = ComponentMask.PlayerActionState | ComponentMask.Position | ComponentMask.Rotation | ComponentMask.PlayerMovement;
            player.generation = 1;
            player.position = new PositionComponent { previous = previous, current = current };
            player.rotation = new RotationComponent { previous = Quaternion.identity, current = Quaternion.identity };
            player.playerMovement = new PlayerMovementComponent
            {
                capsuleRadius = CAPSULE_RADIUS,
                capsuleHeight = 2f,
                capsuleCenter = new Vector3(0f, 1f, 0f),
                collisionLayer = WALL_LAYER_MASK
            };
        }
    }
}
