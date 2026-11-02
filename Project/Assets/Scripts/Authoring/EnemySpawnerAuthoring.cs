using TopView.Definitions;
using UnityEngine;

namespace TopView.Authoring
{
    // 적 한 종류가 생기는 지점. 판 시작 때 Core가 이 Transform의 위치와 회전을 읽는다.
    public class EnemySpawnerAuthoring : MonoBehaviour
    {
        // 이 지점에서 생기는 적 종류
        public EnemyDefinition definition;
    }
}
