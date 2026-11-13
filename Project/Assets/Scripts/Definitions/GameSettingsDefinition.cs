using TopView.Components;
using UnityEngine;
using UnityEngine.Serialization;

namespace TopView.Definitions
{
    // 한 판의 규칙과 저장소 용량
    [CreateAssetMenu(fileName = "Game Settings Definition", menuName = "Definitions/Game Settings Definition")]
    public class GameSettingsDefinition : ScriptableObject
    {
        // 게임 규칙 값
        public GameRuleComponent rule;
        // Application.targetFrameRate에 넣을 값. Logic은 쓰지 않는다
        public int targetFrameRate;
        // 플레이 중에 쓸 OS 커서 텍스처. 일시정지 중에는 기본 커서를 쓴다. Logic은 쓰지 않는다
        public Texture2D crosshairCursor;
        // 적 종류마다 동시에 있을 수 있는 최대 수. 판을 준비할 때 적 구간 칸 수로 쓴다
        public int enemyCapacityPerKind;
        // 적 하나의 경로에 담을 수 있는 꺾이는 점의 최대 수. 시작점도 한 칸을 쓴다
        public int enemyPathCornerCapacity;
        // 동시에 있을 수 있는 직선 투사체 최대 수. 판을 준비할 때 직선 투사체 저장소 칸 수로 쓴다
        [FormerlySerializedAs("projectileCapacity")] public int straightProjectileCapacity;
        // 동시에 있을 수 있는 포물선 투사체 최대 수. 판을 준비할 때 포물선 투사체 저장소 칸 수로 쓴다
        public int arcProjectileCapacity;
        // 동시에 있을 수 있는 전리품 최대 수. 판을 준비할 때 전리품 저장소 칸 수로 쓴다
        public int lootCapacity;
    }
}
