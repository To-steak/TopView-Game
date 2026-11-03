using System.Collections.Generic;
using DG.Tweening;
using TopView.Presentation.Views;
using UnityEngine;

namespace TopView.Presentation.ViewContexts
{
    // 피해량 글자 실행 상태. 판마다 새로 만든다.
    public class WorldTextViewContext
    {
        // 글자를 만들 때 부모로 쓸 월드 공간 Canvas. 씬에 없으면 null
        public Transform parent;
        // 쉬고 있는 글자
        public Stack<WorldTextView> pool = new Stack<WorldTextView>();
        // pool과 같은 순서로, 각 글자의 다시 쓰는 연출
        public Stack<Sequence> sequencePool = new Stack<Sequence>();
        // 표시 중인 글자
        public List<WorldTextView> playingViews = new List<WorldTextView>();
        // playingViews와 같은 순서로, 각 글자의 다시 쓰는 연출
        public List<Sequence> playingSequences = new List<Sequence>();
    }
}
