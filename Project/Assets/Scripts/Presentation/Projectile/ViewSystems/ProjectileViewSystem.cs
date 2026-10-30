using System.Collections.Generic;
using TopView.Entities;
using TopView.Presentation.Projectile.Views;
using TopView.Records;
using UnityEngine;

namespace TopView.Presentation.Projectile.ViewSystems
{
    // 살아 있는 투사체마다 View를 풀에서 꺼내 붙여 위치를 보간하고, 명중·폭발 기록마다 피격 이펙트를 풀에서 꺼내 재생한다.
    public static class ProjectileViewSystem
    {
        /// <summary>
        /// 투사체 저장소의 칸마다 살아 있는 엔티티와 View를 맞추고, 끝난 피격 이펙트를 풀에 돌려놓은 뒤 이번 프레임의 기록마다 이펙트를 재생한다.
        /// </summary>
        /// <param name="projectiles">투사체 저장소</param>
        /// <param name="projectilePrefabs">투사체 View 표. 번호별 ProjectileView 프리팹</param>
        /// <param name="pitchSpeeds">투사체 View 표 번호별로 모델이 앞으로 구르는 각속도(도/초)</param>
        /// <param name="hitRecords">이번 프레임의 명중·폭발 기록</param>
        /// <param name="hitEffectPrefabs">피격 이펙트 표. 번호별 ParticleSystem 프리팹</param>
        /// <param name="context">View 실행 상태</param>
        public static void Present(ProjectileStorage projectiles, GameObject[] projectilePrefabs, float[] pitchSpeeds, List<ProjectileHitRecord> hitRecords, GameObject[] hitEffectPrefabs, ViewContext context)
        {
            float alpha = Mathf.Clamp01((Time.time - Time.fixedTime) / Time.fixedDeltaTime);

            for (int projectileIndex = 0; projectileIndex < projectiles.entities.Length; projectileIndex++)
            {
                ref readonly ProjectileEntity projectile = ref projectiles.entities[projectileIndex];
                ProjectileView view = context.projectileViews[projectileIndex];

                if (view != null && (!projectile.isAlive || context.projectileViewGenerations[projectileIndex] != projectile.generation))
                {
                    Release(context, projectileIndex);
                    view = null;
                }

                if (!projectile.isAlive || projectile.projectile.viewIndex < 0)
                {
                    continue;
                }

                Vector3 position = Vector3.Lerp(projectile.previousPosition, projectile.currentPosition, alpha);
                if (view == null)
                {
                    view = Acquire(context, projectileIndex, in projectile, projectilePrefabs, position);
                }
                else
                {
                    view.transform.SetPositionAndRotation(position, projectile.rotation);
                }

                float pitchSpeed = pitchSpeeds[projectile.projectile.viewIndex];
                if (pitchSpeed != 0f && view.pitchPivot != null)
                {
                    view.pitchPivot.Rotate(Vector3.right * pitchSpeed * Time.deltaTime, Space.Self);
                }
            }

            ReturnFinishedHitEffects(context);

            for (int recordIndex = 0; recordIndex < hitRecords.Count; recordIndex++)
            {
                ProjectileHitRecord record = hitRecords[recordIndex];
                if (record.hitEffectIndex >= 0)
                {
                    PlayHitEffect(context, record.hitEffectIndex, record.position, hitEffectPrefabs);
                }
            }
        }

        private static ProjectileView Acquire(ViewContext context, int projectileIndex, in ProjectileEntity projectile, GameObject[] projectilePrefabs, Vector3 position)
        {
            int viewIndex = projectile.projectile.viewIndex;
            Stack<ProjectileView> pool = context.projectileViewPools[viewIndex];
            ProjectileView view = pool.Count > 0 ? pool.Pop() : Object.Instantiate(projectilePrefabs[viewIndex]).GetComponent<ProjectileView>();

            view.transform.SetPositionAndRotation(position, projectile.rotation);
            view.gameObject.SetActive(true);
            if (view.trail != null)
            {
                view.trail.Clear();
            }

            context.projectileViews[projectileIndex] = view;
            context.projectileViewGenerations[projectileIndex] = projectile.generation;
            context.projectileViewIndices[projectileIndex] = viewIndex;
            return view;
        }

        private static void Release(ViewContext context, int projectileIndex)
        {
            ProjectileView view = context.projectileViews[projectileIndex];
            context.projectileViews[projectileIndex] = null;
            view.gameObject.SetActive(false);
            context.projectileViewPools[context.projectileViewIndices[projectileIndex]].Push(view);
        }

        private static void ReturnFinishedHitEffects(ViewContext context)
        {
            for (int playingIndex = context.playingHitEffects.Count - 1; playingIndex >= 0; playingIndex--)
            {
                ParticleSystem effect = context.playingHitEffects[playingIndex];
                if (effect.IsAlive(true))
                {
                    continue;
                }

                effect.gameObject.SetActive(false);
                context.hitEffectPools[context.playingHitEffectIndices[playingIndex]].Push(effect);
                context.playingHitEffects.RemoveAt(playingIndex);
                context.playingHitEffectIndices.RemoveAt(playingIndex);
            }
        }

        // 풀에서 꺼낸 이펙트는 스스로 지워지지 않도록 Stop Action을 끈 뒤 재생한다.
        private static void PlayHitEffect(ViewContext context, int hitEffectIndex, Vector3 position, GameObject[] hitEffectPrefabs)
        {
            Stack<ParticleSystem> pool = context.hitEffectPools[hitEffectIndex];
            ParticleSystem effect;
            if (pool.Count > 0)
            {
                effect = pool.Pop();
            }
            else
            {
                effect = Object.Instantiate(hitEffectPrefabs[hitEffectIndex]).GetComponent<ParticleSystem>();
                ParticleSystem.MainModule main = effect.main;
                main.stopAction = ParticleSystemStopAction.None;
            }

            effect.transform.SetPositionAndRotation(position, Quaternion.identity);
            effect.gameObject.SetActive(true);
            effect.Play(true);
            context.playingHitEffects.Add(effect);
            context.playingHitEffectIndices.Add(hitEffectIndex);
        }
    }
}
