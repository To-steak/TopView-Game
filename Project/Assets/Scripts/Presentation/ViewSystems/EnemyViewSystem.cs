using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Presentation.Views;
using TopView.Presentation.ViewContexts;
using TopView.Records;
using UnityEngine;

namespace TopView.Presentation.ViewSystems
{
    // 살아 있는 적마다 View를 풀에서 꺼내 붙이고, 위치·회전 보간과 Animator 파라미터를 엔티티에 맞춘다. 공격을 내보낸 적은 공격 소리를, 피해를 받은 적은 피격 소리를 재생한다.
    public static class EnemyViewSystem
    {
        private static readonly int IS_WALK_HASH = Animator.StringToHash("isWalk");
        private static readonly int ATTACK_HASH = Animator.StringToHash("Attack");
        private static readonly int SHOT_HASH = Animator.StringToHash("Shot");
        private static readonly int BIG_SHOT_HASH = Animator.StringToHash("BigShot");
        private static readonly int DIE_HASH = Animator.StringToHash("Die");
        private static readonly int IDLE_STATE_HASH = Animator.StringToHash("Idle");

        private const int BASE_LAYER_INDEX = 0;
        // 그릴 적이 가진 컴포넌트
        private const ComponentMask ENEMY_MASK = ComponentMask.EnemyKind | ComponentMask.Position | ComponentMask.Rotation;

        /// <summary>
        /// 저장소의 칸마다 살아 있는 적과 View를 맞추고, 이번 프레임의 상태 변경 기록을 Animator에 반영하고, 공격 기록마다 공격 소리를, 피해 기록마다 피격 소리를 재생한다.
        /// </summary>
        /// <param name="entities">엔티티 배열. EnemyKind, Position, Rotation을 가진 칸만 그린다</param>
        /// <param name="stateRecords">이번 프레임의 적 상태 변경 기록</param>
        /// <param name="attackRecords">이번 프레임에 적이 공격을 내보낸 기록</param>
        /// <param name="damagedRecords">이번 프레임의 적 피해 기록</param>
        /// <param name="viewPrefabsByKind">적 종류별 EnemyView 프리팹</param>
        /// <param name="attackAnimationsByKind">적 종류별, 공격 번호별 애니메이션</param>
        /// <param name="attackClipsByKind">적 종류별, 공격 번호별 공격 소리. 소리가 없는 공격은 null</param>
        /// <param name="isPaused">판이 일시정지 중이면 true. Animator를 멈춘다</param>
        /// <param name="context">적 View 실행 상태</param>
        public static void Present(Entity[] entities, List<EnemyStateChangedRecord> stateRecords, List<EnemyAttackCommittedRecord> attackRecords, List<EnemyDamagedRecord> damagedRecords, GameObject[] viewPrefabsByKind, EnemyAttackAnimation[][] attackAnimationsByKind, AudioClip[][] attackClipsByKind, bool isPaused, EnemyViewContext context)
        {
            float alpha = Mathf.Clamp01((Time.time - Time.fixedTime) / Time.fixedDeltaTime);
            float animatorSpeed = isPaused ? 0f : 1f;

            for (int index = 0; index < entities.Length; index++)
            {
                ref readonly Entity entity = ref entities[index];
                EnemyView view = context.views[index];
                bool hasEnemy = (entity.mask & ENEMY_MASK) == ENEMY_MASK;

                if (view != null && (!hasEnemy || context.viewGenerations[index] != entity.generation))
                {
                    // View가 있는 칸은 적 종류별 구간이라, 적이 사라져 마스크가 꺼져도 enemyKind에는 그 구간의 종류 번호가 남아 있다
                    Release(context, index, entity.enemyKind.kind);
                    view = null;
                }

                // View 프리팹이 없는 종류는 판을 준비할 때 경고를 남겼으므로 여기서는 건너뛴다
                if (!hasEnemy || viewPrefabsByKind[entity.enemyKind.kind] == null)
                {
                    continue;
                }

                if (view == null)
                {
                    view = Acquire(context, index, entity.enemyKind.kind, entity.generation, viewPrefabsByKind);
                }

                view.animator.speed = animatorSpeed;
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

                int kind = entities[record.enemy.index].enemyKind.kind;
                ApplyStateRecord(in record, view.animator, attackAnimationsByKind[kind], context.triggerHashesByKind[kind]);
            }

            for (int recordIndex = 0; recordIndex < attackRecords.Count; recordIndex++)
            {
                EnemyAttackCommittedRecord record = attackRecords[recordIndex];
                EnemyView view = context.views[record.enemy.index];
                // View가 없거나 다른 적으로 바뀌었거나 소리가 연결되지 않았으면 건너뛴다. 연결 누락과 빈 소리는 판을 준비할 때 남겼다
                if (view == null || context.viewGenerations[record.enemy.index] != record.enemy.generation || view.soundEffectSource == null)
                {
                    continue;
                }

                AudioClip clip = attackClipsByKind[entities[record.enemy.index].enemyKind.kind][record.attackIndex];
                if (clip != null)
                {
                    view.soundEffectSource.PlayOneShot(clip);
                }
            }

            for (int recordIndex = 0; recordIndex < damagedRecords.Count; recordIndex++)
            {
                EnemyDamagedRecord record = damagedRecords[recordIndex];
                EnemyView view = context.views[record.enemy.index];
                // View가 없거나 다른 적으로 바뀌었거나 소리가 연결되지 않았으면 건너뛴다. 연결 누락은 판을 준비할 때 남겼다
                if (view == null || context.viewGenerations[record.enemy.index] != record.enemy.generation || view.soundEffectSource == null || view.hitClip == null)
                {
                    continue;
                }

                view.soundEffectSource.PlayOneShot(view.hitClip);
            }
        }

        private static EnemyView Acquire(EnemyViewContext context, int index, int kind, int generation, GameObject[] viewPrefabsByKind)
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

            PlayIdle(view.animator, context.triggerHashesByKind[kind]);
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

        private static void ApplyStateRecord(in EnemyStateChangedRecord record, Animator animator, EnemyAttackAnimation[] attackAnimations, int[] triggerHashes)
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
                    animator.SetTrigger(GetAttackTriggerHash(attackAnimations[record.attackIndex]));
                    break;
                case EnemyState.Die:
                    animator.SetTrigger(DIE_HASH);
                    break;
            }
        }

        private static int GetAttackTriggerHash(EnemyAttackAnimation attackAnimation)
        {
            switch (attackAnimation)
            {
                case EnemyAttackAnimation.Shot:
                    return SHOT_HASH;
                case EnemyAttackAnimation.BigShot:
                    return BIG_SHOT_HASH;
                default:
                    return ATTACK_HASH;
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
