#if UNITY_EDITOR
using TopView.Commands;
using UnityEngine;

namespace TopView.Core
{
    // 디버그: 에디터의 Context Menu로 디버그 명령을 넣는다. 게임 상태는 다음 틱에 DebugCommandSystem이 바꾼다
    public partial class World
    {
        private const int DEBUG_GOLD_AMOUNT = 2000000000;

        // 판이 없으면(플레이 중이 아니면) 명령을 넣을 곳이 없어 경고하고 버린다
        [ContextMenu("Debug/Add 2,000,000,000 Gold")]
        private void AddDebugGold()
        {
            if (!_isRoundReady)
            {
                Debug.LogWarning("World >> 판이 없어 골드 디버그 명령을 넣지 않았다. 플레이 중에 다시 시도한다.");
                return;
            }

            _commandBuffer.debugCommands.Add(new DebugCommand { type = DebugCommandType.AddGold, amount = DEBUG_GOLD_AMOUNT });
        }
    }
}
#endif
