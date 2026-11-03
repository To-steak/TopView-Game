using DG.Tweening;
using TopView.Components;
using TopView.Presentation.ViewContexts;
using TopView.Presentation.Views;
using UnityEngine;

namespace TopView.Presentation.ViewSystems
{
    // 대기 목록의 글자를 풀에서 꺼내 띄우고, 카메라를 바라보게 하며, 연출이 끝난 글자를 풀에 돌려놓는다.
    public static class WorldTextViewSystem
    {
        /// <summary>
        /// 끝난 글자를 풀에 돌려놓고 대기 목록의 글자를 띄운 뒤, 표시 중인 글자를 카메라로 돌린다. 대기 목록을 채우는 ViewSystem 다음에 부른다.
        /// </summary>
        /// <param name="viewPrefab">WorldTextView가 붙은 프리팹. 없으면 글자를 띄우지 않는다</param>
        /// <param name="rule">월드 글자의 표시 값</param>
        /// <param name="mainCamera">글자가 바라볼 카메라. 없으면 null</param>
        /// <param name="context">월드 글자 실행 상태</param>
        public static void Present(GameObject viewPrefab, in WorldTextRuleComponent rule, UnityEngine.Camera mainCamera, WorldTextViewContext context)
        {
            ReturnFinishedTexts(context);

            // 프리팹이나 Canvas가 없으면 판을 준비할 때 로그를 남겼으므로 여기서는 건너뛴다
            if (viewPrefab != null && context.parent != null)
            {
                for (int entryIndex = 0; entryIndex < context.pendingTexts.Count; entryIndex++)
                {
                    WorldTextEntry entry = context.pendingTexts[entryIndex];
                    ShowText(context, in entry, viewPrefab, rule.lifetimeTicks * Time.fixedDeltaTime, rule.riseDistance);
                }
            }
            context.pendingTexts.Clear();

            if (mainCamera != null)
            {
                FaceCamera(context, mainCamera);
            }
        }

        private static void ReturnFinishedTexts(WorldTextViewContext context)
        {
            for (int playingIndex = context.playingViews.Count - 1; playingIndex >= 0; playingIndex--)
            {
                Sequence sequence = context.playingSequences[playingIndex];
                if (!sequence.IsComplete())
                {
                    continue;
                }

                WorldTextView view = context.playingViews[playingIndex];
                view.gameObject.SetActive(false);
                context.pool.Push(view);
                context.sequencePool.Push(sequence);
                context.playingViews.RemoveAt(playingIndex);
                context.playingSequences.RemoveAt(playingIndex);
            }
        }

        private static void ShowText(WorldTextViewContext context, in WorldTextEntry entry, GameObject viewPrefab, float lifetimeSeconds, float riseDistance)
        {
            WorldTextView view;
            Sequence sequence;
            if (context.pool.Count > 0)
            {
                view = context.pool.Pop();
                sequence = context.sequencePool.Pop();
            }
            else
            {
                view = Object.Instantiate(viewPrefab, context.parent, false).GetComponent<WorldTextView>();
                sequence = CreateSequence(view, lifetimeSeconds, riseDistance);
            }

            view.transform.position = entry.position;
            // 같은 Canvas의 체력 게이지보다 앞에 보이도록 맨 뒤 형제로 옮긴다
            view.transform.SetAsLastSibling();
            view.amountText.text = entry.text;
            view.amountText.color = entry.color;
            view.gameObject.SetActive(true);
            sequence.Restart();

            context.playingViews.Add(view);
            context.playingSequences.Add(sequence);
        }

        // 글자와 함께 다시 쓰는 연출을 만든다. 끝나도 지워지지 않고, 글자가 파괴될 때 함께 지워진다.
        private static Sequence CreateSequence(WorldTextView view, float lifetimeSeconds, float riseDistance)
        {
            RectTransform textTransform = view.amountText.rectTransform;
            Sequence sequence = DOTween.Sequence();
            sequence.Join(textTransform.DOAnchorPosY(textTransform.anchoredPosition.y + riseDistance, lifetimeSeconds));
            sequence.Join(view.canvasGroup.DOFade(0f, lifetimeSeconds));
            sequence.SetAutoKill(false);
            sequence.SetLink(view.gameObject);
            sequence.Pause();
            return sequence;
        }

        private static void FaceCamera(WorldTextViewContext context, UnityEngine.Camera mainCamera)
        {
            Vector3 cameraPosition = mainCamera.transform.position;
            for (int playingIndex = 0; playingIndex < context.playingViews.Count; playingIndex++)
            {
                Transform viewTransform = context.playingViews[playingIndex].transform;
                viewTransform.rotation = Quaternion.LookRotation(viewTransform.position - cameraPosition);
            }
        }
    }
}
