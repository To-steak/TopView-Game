using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Presentation.Enemy.Views;
using TopView.Records;
using UnityEngine;

namespace TopView.Presentation.Enemy.ViewSystems
{
    // 살아 있는 적마다 View를 풀에서 꺼내 붙이고, 위치·회전 보간과 Animator 파라미터를 엔티티에 맞춘다.
    public static class EnemyViewSystem
    {
        private static readonly int IS_WALK_HASH = Animator.StringToHash("isWalk");
        private static readonly int ATTACK_HASH = Animator.StringToHash("Attack");
        private static readonly int SHOT_HASH = Animator.StringToHash("Shot");
        private static readonly int DIE_HASH = Animator.StringToHash("Die");
        private static readonly int IDLE_STATE_HASH = Animator.StringToHash("Idle");

        private const int BASE_LAYER_INDEX = 0;

        /// <summary>
        /// 적 저장소의 칸마다 살아 있는 엔티티와 View를 맞추고, 이번 프레임의 상태 변경 기록을 Animator에 반영한다.
        /// </summary>
        /// <param name="enemies">적 저장소</param>
        /// <param name="stateRecords">이번 프레임의 적 상태 변경 기록</param>
        /// <param name="viewPrefabsByKind">적 종류별 EnemyView 프리팹</param>
        /// <param name="context">View 실행 상태</param>
        public static void Present(EnemyStorage enemies, List<EnemyStateChangedRecord> stateRecords, GameObject[] viewPrefabsByKind, ViewContext context)
        {
            float alpha = Mathf.Clamp01((Time.time - Time.fixedTime) / Time.fixedDeltaTime);

            for (int enemyIndex = 0; enemyIndex < enemies.entities.Length; enemyIndex++)
            {
                ref readonly EnemyEntity enemy = ref enemies.entities[enemyIndex];
                EnemyView view = context.enemyViews[enemyIndex];

                if (view != null && (!enemy.isAlive || context.enemyViewGenerations[enemyIndex] != enemy.generation))
                {
                    Release(context, enemyIndex, enemyIndex / enemies.capacityPerKind);
                    view = null;
                }

                if (!enemy.isAlive)
                {
                    continue;
                }

                if (view == null)
                {
                    view = Acquire(context, enemyIndex, in enemy, viewPrefabsByKind);
                }

                Vector3 position = Vector3.Lerp(enemy.previousPosition, enemy.currentPosition, alpha);
                position.y += enemy.navigation.baseOffset;
                view.transform.SetPositionAndRotation(position, Quaternion.Slerp(enemy.previousRotation, enemy.currentRotation, alpha));
            }

            for (int recordIndex = 0; recordIndex < stateRecords.Count; recordIndex++)
            {
                EnemyStateChangedRecord record = stateRecords[recordIndex];
                EnemyView view = context.enemyViews[record.enemy.index];
                if (view == null || context.enemyViewGenerations[record.enemy.index] != record.enemy.generation)
                {
                    continue;
                }

                int kind = enemies.entities[record.enemy.index].kind;
                ApplyStateRecord(in record, view.animator, context.enemyTriggerHashesByKind[kind]);
            }
        }

        private static EnemyView Acquire(ViewContext context, int enemyIndex, in EnemyEntity enemy, GameObject[] viewPrefabsByKind)
        {
            Stack<EnemyView> pool = context.enemyViewPools[enemy.kind];
            EnemyView view = pool.Count > 0 ? pool.Pop() : Object.Instantiate(viewPrefabsByKind[enemy.kind]).GetComponent<EnemyView>();
            view.gameObject.SetActive(true);

            if (context.enemyTriggerHashesByKind[enemy.kind] == null)
            {
                context.enemyTriggerHashesByKind[enemy.kind] = CollectTriggerHashes(view.animator);
            }

            context.enemyViews[enemyIndex] = view;
            context.enemyViewGenerations[enemyIndex] = enemy.generation;

            PlayIdle(view.animator, context.enemyTriggerHashesByKind[enemy.kind]);
            return view;
        }

        // 칸의 View를 떼어 그 종류의 풀에 돌려놓는다. 적 저장소는 종류별 구간으로 나뉘어 있어 칸 번호로 종류를 안다.
        private static void Release(ViewContext context, int enemyIndex, int kind)
        {
            EnemyView view = context.enemyViews[enemyIndex];
            context.enemyViews[enemyIndex] = null;
            view.gameObject.SetActive(false);
            context.enemyViewPools[kind].Push(view);
        }

        private static void ApplyStateRecord(in EnemyStateChangedRecord record, Animator animator, int[] triggerHashes)
        {
            switch (record.state)
            {
                case EnemyState.Idle:
                    PlayIdle(animator, triggerHashes);
                    break;
                case EnemyState.Walk:
                    animator.SetBool(IS_WALK_HASH, true);
                    break;
                case EnemyState.Attack:
                    animator.SetBool(IS_WALK_HASH, false);
                    animator.SetTrigger(record.attackKind == EnemyAttackKind.Body ? ATTACK_HASH : SHOT_HASH);
                    break;
                case EnemyState.Die:
                    animator.SetTrigger(DIE_HASH);
                    break;
            }
        }

        // 트리거를 모두 지우고 걷기를 끈 뒤 Idle 상태를 처음부터 재생한다.
        private static void PlayIdle(Animator animator, int[] triggerHashes)
        {
            for (int triggerIndex = 0; triggerIndex < triggerHashes.Length; triggerIndex++)
            {
                animator.ResetTrigger(triggerHashes[triggerIndex]);
            }
            animator.SetBool(IS_WALK_HASH, false);
            animator.Play(IDLE_STATE_HASH, BASE_LAYER_INDEX, 0f);
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
