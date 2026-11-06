using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Presentation.Views;
using TopView.Presentation.ViewContexts;
using TopView.Records;
using UnityEngine;

namespace TopView.Presentation.ViewSystems
{
    // 살아 있는 적마다 View를 풀에서 꺼내 붙이고, 위치·회전 보간과 Animator 파라미터를 엔티티에 맞춘다.
    public static class EnemyViewSystem
    {
        private static readonly int IS_WALK_HASH = Animator.StringToHash("isWalk");
        private static readonly int ATTACK_HASH = Animator.StringToHash("Attack");
        private static readonly int SHOT_HASH = Animator.StringToHash("Shot");
        private static readonly int BIG_SHOT_HASH = Animator.StringToHash("BigShot");
        private static readonly int DIE_HASH = Animator.StringToHash("Die");
        private static readonly int IDLE_STATE_HASH = Animator.StringToHash("Idle");
        private static readonly int TAUNT_STATE_HASH = Animator.StringToHash("Taunt");

        private const int BASE_LAYER_INDEX = 0;

        /// <summary>
        /// 저장소의 칸마다 살아 있는 적과 View를 맞추고, 이번 프레임의 상태 변경 기록을 Animator에 반영한다.
        /// </summary>
        /// <param name="entities">적 엔티티 배열</param>
        /// <param name="stateRecords">이번 프레임의 적 상태 변경 기록</param>
        /// <param name="viewPrefabsByKind">적 종류별 EnemyView 프리팹</param>
        /// <param name="context">적 View 실행 상태</param>
        public static void Present(EnemyEntity[] entities, List<EnemyStateChangedRecord> stateRecords, GameObject[] viewPrefabsByKind, EnemyViewContext context)
        {
            float alpha = Mathf.Clamp01((Time.time - Time.fixedTime) / Time.fixedDeltaTime);

            for (int index = 0; index < entities.Length; index++)
            {
                ref readonly EnemyEntity entity = ref entities[index];
                EnemyView view = context.views[index];

                if (view != null && (!entity.isAlive || context.viewGenerations[index] != entity.generation))
                {
                    // 칸은 종류별 구간으로 나뉘어 있어 칸을 다시 채워도 종류 번호는 그대로다
                    Release(context, index, entity.kind.kind);
                    view = null;
                }

                // View 프리팹이 없는 종류는 판을 준비할 때 경고를 남겼으므로 여기서는 건너뛴다
                if (!entity.isAlive || viewPrefabsByKind[entity.kind.kind] == null)
                {
                    continue;
                }

                if (view == null)
                {
                    view = Acquire(context, index, entity.kind.kind, entity.generation, entity.actionState.state, viewPrefabsByKind);
                }

                Vector3 position = Vector3.Lerp(entity.position.previous, entity.position.current, alpha);
                view.transform.SetPositionAndRotation(position, Quaternion.Slerp(entity.rotation.previous, entity.rotation.current, alpha));
            }

            for (int recordIndex = 0; recordIndex < stateRecords.Count; recordIndex++)
            {
                EnemyStateChangedRecord record = stateRecords[recordIndex];
                EnemyView view = context.views[record.enemy.index];
                if (view == null || context.viewGenerations[record.enemy.index] != record.enemy.generation)
                {
                    continue;
                }

                ApplyStateRecord(in record, view.animator, context.triggerHashesByKind[entities[record.enemy.index].kind.kind]);
            }
        }

        // 칸에 그 종류의 View를 붙이고, 도발 중이면 Taunt 상태를, 아니면 Idle 상태를 처음부터 재생한다.
        private static EnemyView Acquire(EnemyViewContext context, int index, int kind, int generation, EnemyState state, GameObject[] viewPrefabsByKind)
        {
            Stack<EnemyView> pool = context.viewPools[kind];
            EnemyView view = pool.Count > 0 ? pool.Pop() : Object.Instantiate(viewPrefabsByKind[kind]).GetComponent<EnemyView>();
            view.gameObject.SetActive(true);

            if (context.triggerHashesByKind[kind] == null)
            {
                context.triggerHashesByKind[kind] = CollectTriggerHashes(view.animator);
            }

            context.views[index] = view;
            context.viewGenerations[index] = generation;

            PlayFromStart(view.animator, context.triggerHashesByKind[kind], state == EnemyState.Taunt ? TAUNT_STATE_HASH : IDLE_STATE_HASH);
            return view;
        }

        // 칸의 View를 떼어 그 종류의 풀에 돌려놓는다.
        private static void Release(EnemyViewContext context, int index, int kind)
        {
            EnemyView view = context.views[index];
            context.views[index] = null;
            view.gameObject.SetActive(false);
            context.viewPools[kind].Push(view);
        }

        private static void ApplyStateRecord(in EnemyStateChangedRecord record, Animator animator, int[] triggerHashes)
        {
            switch (record.state)
            {
                case EnemyState.Idle:
                    PlayFromStart(animator, triggerHashes, IDLE_STATE_HASH);
                    break;
                case EnemyState.Walk:
                    animator.SetBool(IS_WALK_HASH, true);
                    break;
                case EnemyState.Attack:
                    animator.SetBool(IS_WALK_HASH, false);
                    animator.SetTrigger(GetAttackTriggerHash(record.attackKind));
                    break;
                case EnemyState.Die:
                    animator.SetTrigger(DIE_HASH);
                    break;
            }
        }

        private static int GetAttackTriggerHash(EnemyAttackKind attackKind)
        {
            switch (attackKind)
            {
                case EnemyAttackKind.Body:
                    return ATTACK_HASH;
                case EnemyAttackKind.BossRock:
                    return BIG_SHOT_HASH;
                default:
                    return SHOT_HASH;
            }
        }

        // 트리거를 모두 지우고 걷기를 끈 뒤 Animator 상태 하나를 처음부터 재생한다.
        private static void PlayFromStart(Animator animator, int[] triggerHashes, int stateHash)
        {
            for (int triggerIndex = 0; triggerIndex < triggerHashes.Length; triggerIndex++)
            {
                animator.ResetTrigger(triggerHashes[triggerIndex]);
            }
            animator.SetBool(IS_WALK_HASH, false);
            animator.Play(stateHash, BASE_LAYER_INDEX, 0f);
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
