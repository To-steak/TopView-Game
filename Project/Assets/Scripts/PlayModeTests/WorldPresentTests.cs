using System.Collections;
using NUnit.Framework;
using TopView.Components;
using UnityEngine.TestTools;

namespace TopView.PlayModeTests
{
    // World.Present가 ViewSystem에 넘기는 값에서 생기는 동작을 실제 World로 확인한다. Present는 App이 LateUpdate에서 부르므로 한 프레임을 넘겨 본다.
    public class WorldPresentTests
    {
        private WorldTestScene _scene;

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
    }
}
