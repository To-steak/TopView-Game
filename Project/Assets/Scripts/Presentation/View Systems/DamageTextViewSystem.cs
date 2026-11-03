using System.Collections.Generic;
using DG.Tweening;
using TopView.Components;
using TopView.Presentation.Views;
using TopView.Presentation.ViewContexts;
using TopView.Records;
using UnityEngine;

namespace TopView.Presentation.ViewSystems
{
    // 적 피해 기록마다 피해량 글자를 풀에서 꺼내 띄우고, 카메라를 바라보게 하며, 연출이 끝난 글자를 풀에 돌려놓는다.
    public static class DamageTextViewSystem
    {
        /// <summary>
        /// 끝난 글자를 풀에 돌려놓고 이번 프레임의 기록마다 글자를 띄운 뒤, 표시 중인 글자를 카메라로 돌린다. EnemyViewSystem.Present 다음에 부른다.
        /// </summary>
        /// <param name="damagedRecords">이번 프레임에 적이 피해를 받은 기록</param>
        /// <param name="enemyViews">적 저장소 칸별 View</param>
        /// <param name="enemyViewGenerations">적 저장소 칸별 View가 보여 주는 엔티티의 generation</param>
        /// <param name="viewPrefab">DamageTextView가 붙은 프리팹. 없으면 글자를 띄우지 않는다</param>
        /// <param name="rule">피해량 글자의 표시 값</param>
        /// <param name="mainCamera">글자가 바라볼 카메라. 없으면 null</param>
        /// <param name="context">피해량 글자 실행 상태</param>
        public static void Present(List<EnemyDamagedRecord> damagedRecords, EnemyView[] enemyViews, int[] enemyViewGenerations, GameObject viewPrefab, in WorldTextRuleComponent rule, UnityEngine.Camera mainCamera, WorldTextViewContext context)
        {
            ReturnFinishedTexts(context);

            // 프리팹이나 Canvas가 없으면 판을 준비할 때 로그를 남겼으므로 여기서는 건너뛴다
            if (viewPrefab != null && context.parent != null)
            {
                for (int recordIndex = 0; recordIndex < damagedRecords.Count; recordIndex++)
                {
                    EnemyDamagedRecord record = damagedRecords[recordIndex];
                    Vector3 position = GetStartPosition(in record, enemyViews, enemyViewGenerations, rule.spreadRadius, mainCamera);
                    ShowText(context, position, record.amount, viewPrefab, rule.lifetimeTicks * Time.fixedDeltaTime, rule.riseDistance);
                }
            }

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

        // 적 View가 있으면 그 적의 Damage Anchor에서, 없으면 기록의 위치에서 시작한다. 이 위치로 고정하므로 글자는 적이 사라져도 제자리에 남는다.
        private static Vector3 GetStartPosition(in EnemyDamagedRecord record, EnemyView[] enemyViews, int[] enemyViewGenerations, float spreadRadius, UnityEngine.Camera mainCamera)
        {
            EnemyView enemyView = enemyViews[record.enemy.index];
            bool hasAnchor = enemyView != null
                && enemyViewGenerations[record.enemy.index] == record.enemy.generation
                && enemyView.damageAnchor != null;
            Vector3 position = hasAnchor ? enemyView.damageAnchor.position : record.position;

            Quaternion spreadRotation = mainCamera != null ? mainCamera.transform.rotation : Quaternion.identity;
            Vector3 spread = Random.insideUnitCircle * spreadRadius;
            return position + spreadRotation * spread;
        }

        private static void ShowText(WorldTextViewContext context, Vector3 position, float amount, GameObject viewPrefab, float lifetimeSeconds, float riseDistance)
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

            view.transform.position = position;
            // 같은 Canvas의 체력 게이지보다 앞에 보이도록 맨 뒤 형제로 옮긴다
            view.transform.SetAsLastSibling();
            view.amountText.text = amount.ToString("F0");
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
