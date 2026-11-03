using System.Collections.Generic;
using DG.Tweening;
using TopView.Presentation.DamageText.Views;
using UnityEngine;

namespace TopView.Presentation
{
    // 피해량 글자 실행 상태. 판마다 새로 만든다.
    public class DamageTextViewContext
    {
        // 글자를 만들 때 부모로 쓸 월드 공간 Canvas. 씬에 없으면 null
        public Transform parent;
        // 쉬고 있는 글자
        public Stack<DamageTextView> pool = new Stack<DamageTextView>();
        // pool과 같은 순서로, 각 글자의 다시 쓰는 연출
        public Stack<Sequence> sequencePool = new Stack<Sequence>();
        // 표시 중인 글자
        public List<DamageTextView> playingViews = new List<DamageTextView>();
        // playingViews와 같은 순서로, 각 글자의 다시 쓰는 연출
        public List<Sequence> playingSequences = new List<Sequence>();
    }
}
