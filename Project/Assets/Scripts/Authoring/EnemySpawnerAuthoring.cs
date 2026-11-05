using TopView.Components;
using TopView.Definitions;
using UnityEngine;

namespace TopView.Authoring
{
    // 적 한 종류를 만드는 스포너. Player Detector Authoring에 연결되어 발동한다. 판 시작 때 Core가 이 Transform의 위치와 회전을 읽는다.
    public class EnemySpawnerAuthoring : MonoBehaviour
    {
        // 이 스포너에서 생기는 적 종류
        public EnemyDefinition definition;
        // 생성 주기와 최대 마릿수
        public EnemySpawnerRuleComponent rule;
    }
}
