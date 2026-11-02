#if UNITY_EDITOR
using TopView.Entities;
using UnityEngine;

namespace TopView.Core
{
    // 근접 판정: 디버그 표시. 지금은 에디터 전용 코드뿐이라 파일 전체를 UNITY_EDITOR로 감싼다
    public partial class Runner
    {
        // 지금 살아 있는 근접 판정마다 판정 원을 자홍으로 그린다. 플레이어·적·보스의 근접 공격이 모두 여기에 나온다.
        private void DrawMeleeGizmos()
        {
            EntityStorage entities = _world.entities;
            Gizmos.color = Color.magenta;
            for (int index = 0; index < entities.masks.Length; index++)
            {
                if ((entities.masks[index] & ComponentMask.MeleeArchetype) == ComponentMask.MeleeArchetype)
                {
                    DrawCircleGizmo(entities.positions[index].current, entities.melees[index].radius);
                }
            }
        }
    }
}
#endif
