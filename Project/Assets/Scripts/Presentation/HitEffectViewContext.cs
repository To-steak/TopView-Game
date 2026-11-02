using System.Collections.Generic;
using UnityEngine;

namespace TopView.Presentation
{
    // 피격 이펙트 실행 상태. 판마다 새로 만든다.
    public class HitEffectViewContext
    {
        // 피격 이펙트 표의 번호별로 쉬고 있는 ParticleSystem
        public Stack<ParticleSystem>[] pools;
        // 재생 중인 피격 이펙트
        public List<ParticleSystem> playingEffects = new List<ParticleSystem>();
        // playingEffects와 같은 순서로, 각 이펙트가 속한 피격 이펙트 표의 번호
        public List<int> playingEffectIndices = new List<int>();
    }
}
