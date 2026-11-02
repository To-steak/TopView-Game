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
        /// <param name="masks">칸별 컴포넌트 표시</param>
        /// <param name="generations">칸이 채워진 횟수</param>
        /// <param name="positions">위치</param>
        /// <param name="rotations">회전</param>
        /// <param name="attackLists">공격 목록</param>
        /// <param name="actionStates">보스 행동 상태</param>
        /// <param name="stateRecords">이번 프레임의 보스 상태 변경 기록</param>
        /// <param name="viewPrefab">BossView가 붙은 프리팹</param>
        /// <param name="context">보스 View 실행 상태</param>
        public static void Present(ComponentMask[] masks, int[] generations, PositionComponent[] positions, RotationComponent[] rotations, AttackListComponent[] attackLists, BossActionStateComponent[] actionStates, List<BossStateChangedRecord> stateRecords, GameObject viewPrefab, BossViewContext context)
        {
            if (!TryFindAliveBoss(masks, out int bossIndex))
            {
                if (context.view != null && context.view.gameObject.activeSelf)
                {
                    context.view.gameObject.SetActive(false);
                }
                return;
            }

            ref readonly PositionComponent position = ref positions[bossIndex];
            ref readonly RotationComponent rotation = ref rotations[bossIndex];
            EntityIdentifier identifier = new EntityIdentifier { index = bossIndex, generation = generations[bossIndex] };

            if (context.view == null || !context.view.gameObject.activeSelf || context.viewEntity.generation != identifier.generation)
            {
                ShowView(in position, in rotation, in actionStates[bossIndex], attackLists[bossIndex].attacks, in identifier, viewPrefab, context);
            }

            Animator animator = context.view.animator;
            for (int recordIndex = 0; recordIndex < stateRecords.Count; recordIndex++)
            {
                BossStateChangedRecord record = stateRecords[recordIndex];
                if (record.boss.index == identifier.index && record.boss.generation == identifier.generation)
                {
                    ApplyState(record.state, record.attackKind, animator, context.triggerHashes);
                }
            }

            float alpha = Mathf.Clamp01((Time.time - Time.fixedTime) / Time.fixedDeltaTime);
            context.view.transform.SetPositionAndRotation(
                Vector3.Lerp(position.previous, position.current, alpha),
                Quaternion.Slerp(rotation.previous, rotation.current, alpha));
        }

        private static bool TryFindAliveBoss(ComponentMask[] masks, out int bossIndex)
        {
            for (bossIndex = 0; bossIndex < masks.Length; bossIndex++)
            {
                if ((masks[bossIndex] & ComponentMask.BossArchetype) == ComponentMask.BossArchetype)
                {
                    return true;
                }
            }

            bossIndex = -1;
            return false;
        }

        // View를 만들거나 다시 켜고, 지금 상태에 들어갈 때의 Animator 파라미터를 건다.
        private static void ShowView(in PositionComponent position, in RotationComponent rotation, in BossActionStateComponent actionState, EnemyAttackComponent[] attacks, in EntityIdentifier identifier, GameObject viewPrefab, BossViewContext context)
        {
            if (context.view == null)
            {
                context.view = Object.Instantiate(viewPrefab, position.current, rotation.current).GetComponent<BossView>();
                context.triggerHashes = CollectTriggerHashes(context.view.animator);
            }
            else
            {
                context.view.gameObject.SetActive(true);
            }

            context.viewEntity = identifier;

            EnemyAttackKind attackKind = actionState.state == BossState.Attack ? attacks[actionState.attackIndex].kind : default;
            ApplyState(actionState.state, attackKind, context.view.animator, context.triggerHashes);
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
