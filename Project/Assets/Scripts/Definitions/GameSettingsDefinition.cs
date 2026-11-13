using TopView.Components;
using UnityEngine;

namespace TopView.Definitions
{
    // 한 판의 규칙과 화면 설정. 저장소 칸 수는 Core의 상수다
    [CreateAssetMenu(fileName = "Game Settings Definition", menuName = "Definitions/Game Settings Definition")]
    public class GameSettingsDefinition : ScriptableObject
    {
        // 게임 규칙 값
        public GameRuleComponent rule;
        // Application.targetFrameRate에 넣을 값. Logic은 쓰지 않는다
        public int targetFrameRate;
        // 플레이 중에 쓸 OS 커서 텍스처. 일시정지 중에는 기본 커서를 쓴다. Logic은 쓰지 않는다
        public Texture2D crosshairCursor;
    }
}
