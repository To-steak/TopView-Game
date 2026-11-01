using System.Collections.Generic;
using TopView.Entities;
using TopView.Presentation.Projectile.Views;
using UnityEngine;

namespace TopView.Presentation.Projectile.ViewSystems
{
    // 살아 있는 투사체마다 View를 풀에서 꺼내 붙여 위치를 보간한다.
    public static class ProjectileViewSystem
    {
        /// <summary>
        /// 투사체 저장소의 칸마다 살아 있는 엔티티와 View를 맞춘다.
        /// </summary>
        /// <param name="projectiles">투사체 저장소</param>
        /// <param name="projectilePrefabs">투사체 View 표. 번호별 ProjectileView 프리팹</param>
        /// <param name="pitchSpeeds">투사체 View 표 번호별로 모델이 앞으로 구르는 각속도(도/초)</param>
        /// <param name="context">View 실행 상태</param>
        public static void Present(ProjectileStorage projectiles, GameObject[] projectilePrefabs, float[] pitchSpeeds, ViewContext context)
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
    }
}
