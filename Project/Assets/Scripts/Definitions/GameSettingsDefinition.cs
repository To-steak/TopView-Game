using TopView.Components;
using UnityEngine;

namespace TopView.Definitions
{
    // 한 판의 규칙과 웨이브 구성
    [CreateAssetMenu(fileName = "Game Settings Definition", menuName = "Definitions/Game Settings Definition")]
    public class GameSettingsDefinition : ScriptableObject
    {
        // 게임 규칙 값
        public GameRuleComponent rule;
        // 웨이브 목록
        public WaveComponent[] waves;
        // 웨이브별 스폰을 켤 적 종류. 번호는 waves와 같고, 보스 웨이브 칸은 비워 둔다
        public EnemyDefinition[] waveEnemies;
        // Application.targetFrameRate에 넣을 값. Logic은 쓰지 않는다
        public int targetFrameRate;
        // 적 종류마다 동시에 있을 수 있는 최대 수. 판을 준비할 때 적 저장소 칸 수로 쓴다
        public int enemyCapacityPerKind;
        // 동시에 있을 수 있는 투사체 최대 수. 판을 준비할 때 투사체 저장소 칸 수로 쓴다
        public int projectileCapacity;
    }
}
