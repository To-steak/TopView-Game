using System.Collections;
using NUnit.Framework;
using TopView.Components;
using TopView.Entities;
using TopView.Presentation.Views;
using TopView.Presentation.ViewSystems;
using TopView.Requests;
using UnityEngine;
using UnityEngine.TestTools;

namespace TopView.PlayModeTests
{
    // World.Present가 ViewSystem에 넘기는 값에서 생기는 동작을 실제 World로 확인한다. Present는 App이 LateUpdate에서 부르므로 한 프레임을 넘겨 본다.
    public class WorldPresentTests
    {
        private WorldTestScene _scene;

        // 테스트 엔티티를 놓을 자리. 씬의 지형과 겹치지 않게 멀리 둔다
        private static readonly Vector3 TEST_ORIGIN = new Vector3(1000f, 1f, 1000f);
        private const int FLIGHT_TICKS = 10;

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
        /// 판이 끝나면 크로스헤어를 내리고 기본 커서로 게임 오버 화면의 버튼을 누르게 한다.
        /// </summary>
        /// <returns>테스트 코루틴</returns>
        [UnityTest]
        public IEnumerator CrosshairIsReleasedWhenRoundEnds()
        {
            Assert.IsTrue(_scene.world.IsTestCrosshairAssigned, "Game Settings Definition에 크로스헤어 커서가 연결되어 있어야 이 테스트가 뜻이 있다.");
            yield return null;
            Assert.IsTrue(_scene.world.IsTestCrosshairApplied, "플레이 중에는 크로스헤어가 걸려 있어야 한다.");

            _scene.world.TestProgress.phase = GamePhase.Lost;
            yield return null;

            Assert.IsFalse(_scene.world.IsTestCrosshairApplied, "판이 끝났는데 크로스헤어가 남아 있다.");
        }

        /// <summary>
        /// 투척 충전 중에 판이 끝나면 궤도선과 폭발 범위 원을 내린다. 판이 끝난 뒤에는 미리보기를 끄는 PlayerThrowPreviewSystem이 돌지 않는다.
        /// </summary>
        /// <returns>테스트 코루틴</returns>
        [UnityTest]
        public IEnumerator ThrowPreviewIsHiddenAfterRoundEnds()
        {
            EntityStorage storage = _scene.world.TestEntities;
            // 씬의 플레이어가 생기면 View가 그 칸을 먼저 찾을 수 있어 테스트가 놓는 플레이어만 남긴다
            _scene.world.TestPlayerSpawnRequests.Clear();
            PlaceChargingPlayer(storage, TEST_ORIGIN);
            PlayerView view = CreatePlayerView();

            _scene.world.Tick();
            PresentThrowPreview(storage, view);
            Assert.IsTrue(view.throwPreviewLine.gameObject.activeSelf, "판이 진행 중이고 투척 충전 중이면 궤도선이 보여야 이 테스트가 뜻이 있다.");

            _scene.world.TestGameOverRequests.Enqueue(new GameOverRequest { isVictory = false });
            _scene.world.Tick();
            Assert.AreEqual(GamePhase.Lost, _scene.world.TestProgress.phase);
            _scene.world.Tick();
            PresentThrowPreview(storage, view);

            bool isLineShown = view.throwPreviewLine.gameObject.activeSelf;
            bool isCircleShown = view.throwPreviewCircle.gameObject.activeSelf;
            Object.Destroy(view.gameObject);
            Assert.IsFalse(isLineShown, "판이 끝났는데 투척 궤도선이 보인다.");
            Assert.IsFalse(isCircleShown, "판이 끝났는데 폭발 범위 원이 보인다.");
            yield break;
        }

        // 투척 무기를 들고 충전 중인 플레이어를 놓는다. PlayerThrowPreviewSystem이 읽는 컴포넌트만 가져 다른 플레이어 System은 이 칸을 건너뛴다.
        private static void PlaceChargingPlayer(EntityStorage storage, Vector3 position)
        {
            int weaponIndex = WorldTestScene.TakeFreeIndex(storage.freeWeaponIndices);
            storage.entities[weaponIndex] = new Entity
            {
                mask = ComponentMask.ArcAttack | ComponentMask.Throw | ComponentMask.UpgradeLevel | ComponentMask.WeaponUpgrade,
                generation = storage.entities[weaponIndex].generation + 1,
                arcAttack = new ArcAttackComponent
                {
                    projectile = new ArcProjectileComponent
                    {
                        flight = new ArcFlightComponent { flightTicks = FLIGHT_TICKS, arcHeight = 1f },
                        explosion = new ExplosionComponent { radius = 2f }
                    },
                    shots = new ProjectileShotComponent[] { new ProjectileShotComponent() }
                },
                throwing = new ThrowComponent { maxThrowDistance = 5f }
            };

            int playerIndex = WorldTestScene.TakeFreeIndex(storage.freePlayerIndices);
            storage.entities[playerIndex] = new Entity
            {
                mask = ComponentMask.PlayerActionState | ComponentMask.Position | ComponentMask.Rotation | ComponentMask.WeaponSlots | ComponentMask.ThrowPreview,
                generation = storage.entities[playerIndex].generation + 1,
                playerActionState = new PlayerActionStateComponent { state = PlayerState.ThrowCharge, elapsedTicks = 1 },
                position = new PositionComponent { previous = position, current = position },
                rotation = new RotationComponent { previous = Quaternion.identity, current = Quaternion.identity },
                weaponSlots = new PlayerWeaponSlotsComponent { weapons = new EntityIdentifier[] { WorldTestScene.Identify(storage, weaponIndex) }, currentIndex = 0 },
                throwPreview = new PlayerThrowPreviewComponent { points = new Vector3[FLIGHT_TICKS + 1] }
            };
        }

        // World.Present처럼 진행 단계로 판이 끝났는지 구해 미리보기를 그린다. 테스트가 놓는 플레이어 칸에는 PlayerViewSystem이 읽는 컴포넌트가 없어 World.Present를 부르지 않는다.
        private void PresentThrowPreview(EntityStorage storage, PlayerView view)
        {
            bool isRoundEnded = _scene.world.TestProgress.phase != GamePhase.Playing;
            PlayerThrowPreviewViewSystem.Present(storage.entities, isRoundEnded, view, 0f);
        }

        // 궤도선과 원만 붙인 플레이어 View를 만든다.
        private static PlayerView CreatePlayerView()
        {
            PlayerView view = new GameObject("Test Player View").AddComponent<PlayerView>();
            view.throwPreviewLine = new GameObject("Throw Preview Line").AddComponent<LineRenderer>();
            view.throwPreviewLine.transform.SetParent(view.transform, false);
            view.throwPreviewCircle = new GameObject("Throw Preview Circle").transform;
            view.throwPreviewCircle.SetParent(view.transform, false);
            return view;
        }
    }
}
