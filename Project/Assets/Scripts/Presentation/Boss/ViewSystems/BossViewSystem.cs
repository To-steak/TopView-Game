using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Presentation.Boss.Views;
using TopView.Records;
using UnityEngine;

namespace TopView.Presentation.Boss.ViewSystems
{
    // 보스 View를 만들고, 위치·회전 보간과 Animator 파라미터를 엔티티에 맞춘다. 보스가 사라지면 View를 끈다.
    public static class BossViewSystem
    {
        private static readonly int TAUNT_HASH = Animator.StringToHash("Taunt");
        private static readonly int SHOT_HASH = Animator.StringToHash("Shot");
        private static readonly int BIG_SHOT_HASH = Animator.StringToHash("BigShot");
        private static readonly int DIE_HASH = Animator.StringToHash("Die");
        private static readonly int IDLE_STATE_HASH = Animator.StringToHash("Idle");

        private const int BASE_LAYER_INDEX = 0;

        /// <summary>
        /// 살아 있는 보스가 있으면 View를 만들거나 맞추고 상태 변경 기록을 Animator에 반영한다. 없으면 View를 끈다.
        /// </summary>
        /// <param name="bosses">보스 저장소</param>
        /// <param name="stateRecords">이번 프레임의 보스 상태 변경 기록</param>
        /// <param name="viewPrefab">BossView가 붙은 프리팹</param>
        /// <param name="context">View 실행 상태</param>
        public static void Present(BossStorage bosses, List<BossStateChangedRecord> stateRecords, GameObject viewPrefab, ViewContext context)
        {
            if (!TryFindAliveBoss(bosses, out int bossIndex))
            {
                if (context.bossView != null && context.bossView.gameObject.activeSelf)
                {
                    context.bossView.gameObject.SetActive(false);
                }
                return;
            }

            ref readonly BossEntity boss = ref bosses.entities[bossIndex];
            EntityIdentifier identifier = new EntityIdentifier { index = bossIndex, generation = boss.generation };

            if (context.bossView == null || !context.bossView.gameObject.activeSelf || context.bossViewEntity.generation != identifier.generation)
            {
                ShowView(in boss, in identifier, viewPrefab, context);
            }

            Animator animator = context.bossView.animator;
            for (int recordIndex = 0; recordIndex < stateRecords.Count; recordIndex++)
            {
                BossStateChangedRecord record = stateRecords[recordIndex];
                if (record.boss.index == identifier.index && record.boss.generation == identifier.generation)
                {
                    ApplyState(record.state, record.attackKind, animator, context.bossTriggerHashes);
                }
            }

            float alpha = Mathf.Clamp01((Time.time - Time.fixedTime) / Time.fixedDeltaTime);
            context.bossView.transform.SetPositionAndRotation(
                Vector3.Lerp(boss.previousPosition, boss.currentPosition, alpha),
                Quaternion.Slerp(boss.previousRotation, boss.currentRotation, alpha));
        }

        private static bool TryFindAliveBoss(BossStorage bosses, out int bossIndex)
        {
            for (bossIndex = 0; bossIndex < bosses.entities.Length; bossIndex++)
            {
                if (bosses.entities[bossIndex].isAlive)
                {
                    return true;
                }
            }

            bossIndex = -1;
            return false;
        }

        // View를 만들거나 다시 켜고, 지금 상태에 들어갈 때의 Animator 파라미터를 건다.
        private static void ShowView(in BossEntity boss, in EntityIdentifier identifier, GameObject viewPrefab, ViewContext context)
        {
            if (context.bossView == null)
            {
                context.bossView = Object.Instantiate(viewPrefab, boss.currentPosition, boss.currentRotation).GetComponent<BossView>();
                context.bossTriggerHashes = CollectTriggerHashes(context.bossView.animator);
            }
            else
            {
                context.bossView.gameObject.SetActive(true);
            }

            context.bossViewEntity = identifier;

            EnemyAttackKind attackKind = boss.state == BossState.Attack ? boss.attacks[boss.attackIndex].kind : default;
            ApplyState(boss.state, attackKind, context.bossView.animator, context.bossTriggerHashes);
        }

        private static void ApplyState(BossState state, EnemyAttackKind attackKind, Animator animator, int[] triggerHashes)
        {
            switch (state)
            {
                case BossState.Taunt:
                    animator.SetTrigger(TAUNT_HASH);
                    break;
                case BossState.Idle:
                    for (int triggerIndex = 0; triggerIndex < triggerHashes.Length; triggerIndex++)
                    {
                        animator.ResetTrigger(triggerHashes[triggerIndex]);
                    }
                    animator.Play(IDLE_STATE_HASH, BASE_LAYER_INDEX, 0f);
                    break;
                case BossState.Attack:
                    animator.SetTrigger(attackKind == EnemyAttackKind.BossRock ? BIG_SHOT_HASH : SHOT_HASH);
                    break;
                case BossState.Die:
                    animator.SetTrigger(DIE_HASH);
                    break;
            }
        }

        private static int[] CollectTriggerHashes(Animator animator)
        {
            List<int> triggerHashes = new List<int>();
            AnimatorControllerParameter[] parameters = animator.parameters;
            for (int parameterIndex = 0; parameterIndex < parameters.Length; parameterIndex++)
            {
                if (parameters[parameterIndex].type == AnimatorControllerParameterType.Trigger)
                {
                    triggerHashes.Add(parameters[parameterIndex].nameHash);
                }
            }

            return triggerHashes.ToArray();
        }
    }
}
