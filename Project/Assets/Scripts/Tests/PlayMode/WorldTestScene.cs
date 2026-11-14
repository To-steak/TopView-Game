using System.Collections;
using NUnit.Framework;
using TopView.Core;
using TopView.Entities;
using TopView.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TopView.Tests.PlayMode
{
    // 실제 World 씬을 불러와 틱을 멈춘 채 판을 준비하고, 테스트가 엔티티를 놓을 빈 칸을 꺼내 준다.
    // 씬을 불러오기 전에 Time.timeScale을 0으로 두어 App의 FixedUpdate가 한 번도 돌지 않게 한다. 그래서 판 준비 직후의 저장소는 비어 있고, 씬의 생성 요청은 테스트가 처음 부르는 Tick의 끝에서야 처리된다.
    public class WorldTestScene
    {
        // 불러온 씬의 World
        public World world;

        private const string SCENE_NAME = "World";
        // 판 준비를 기다리는 최대 프레임 수
        private const int MAX_WAIT_FRAMES = 300;

        /// <summary>
        /// 틱을 멈추고 World 씬을 불러와 판이 준비될 때까지 기다린다. 준비되지 않으면 테스트를 실패시킨다.
        /// </summary>
        /// <returns>판이 준비되면 끝나는 코루틴</returns>
        public IEnumerator Load()
        {
            Time.timeScale = 0f;
            yield return SceneManager.LoadSceneAsync(SCENE_NAME, LoadSceneMode.Single);

            for (int frame = 0; frame < MAX_WAIT_FRAMES; frame++)
            {
                world = Object.FindAnyObjectByType<World>();
                if (world != null && world.IsTestRoundReady)
                {
                    yield break;
                }
                yield return null;
            }

            Assert.Fail($"World 씬을 불러온 뒤 {MAX_WAIT_FRAMES}프레임 안에 판이 준비되지 않았다.");
        }

        /// <summary>
        /// 저장소를 비우고 멈춘 틱을 되돌린다. 테스트가 끝날 때 부른다.
        /// 테스트가 놓은 엔티티는 View에 필요한 컴포넌트가 빠져 있을 수 있어, 다음 테스트가 씬을 다시 불러오기 전의 프레임에 View가 읽지 않게 비운다.
        /// </summary>
        public void Unload()
        {
            if (world != null && world.IsTestRoundReady)
            {
                Entity[] entities = world.TestEntities.entities;
                for (int index = 0; index < entities.Length; index++)
                {
                    entities[index].mask = ComponentMask.None;
                }
            }
            Time.timeScale = 1f;
        }

        /// <summary>
        /// 빈 칸 목록에서 칸 하나를 꺼낸다. 칸을 채우는 쪽이 generation을 1 올린다.
        /// </summary>
        /// <param name="freeIndices">꺼낼 구간의 빈 칸 목록</param>
        /// <returns>꺼낸 칸 번호</returns>
        public static int TakeFreeIndex(System.Collections.Generic.List<int> freeIndices)
        {
            Assert.IsTrue(SpawnUtility.TryTakeFreeIndex(freeIndices, out int index), "테스트가 엔티티를 놓을 빈 칸이 없다.");
            return index;
        }

        /// <summary>
        /// 칸 번호와 그 칸의 지금 generation으로 식별자를 만든다.
        /// </summary>
        /// <param name="storage">엔티티 저장소</param>
        /// <param name="index">칸 번호</param>
        /// <returns>그 칸을 가리키는 식별자</returns>
        public static EntityIdentifier Identify(EntityStorage storage, int index)
        {
            return new EntityIdentifier { index = index, generation = storage.entities[index].generation };
        }
    }
}
