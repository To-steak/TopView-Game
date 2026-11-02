using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Presentation.Projectile.Views;
using UnityEngine;

namespace TopView.Presentation.Projectile.ViewSystems
{
    // 살아 있는 투사체마다 View를 풀에서 꺼내 붙여 위치를 보간한다.
    public static class ProjectileViewSystem
    {
        /// <summary>
        /// 공유 저장소의 칸마다 살아 있는 투사체와 View를 맞춘다.
        /// </summary>
        /// <param name="masks">칸별 컴포넌트 표시</param>
        /// <param name="generations">칸이 채워진 횟수</param>
        /// <param name="projectiles">투사체 값</param>
        /// <param name="positions">위치</param>
        /// <param name="rotations">회전</param>
        /// <param name="projectilePrefabs">투사체 View 표. 번호별 ProjectileView 프리팹</param>
        /// <param name="context">투사체 View 실행 상태</param>
        public static void Present(ComponentMask[] masks, int[] generations, ProjectileComponent[] projectiles, PositionComponent[] positions, RotationComponent[] rotations, GameObject[] projectilePrefabs, ProjectileViewContext context)
        {
            float alpha = Mathf.Clamp01((Time.time - Time.fixedTime) / Time.fixedDeltaTime);

            for (int index = 0; index < masks.Length; index++)
            {
                bool isProjectile = (masks[index] & ComponentMask.ProjectileArchetype) == ComponentMask.ProjectileArchetype;
                ProjectileView view = context.views[index];

                if (view != null && (!isProjectile || context.viewGenerations[index] != generations[index]))
                {
                    Release(context, index);
                    view = null;
                }

                if (!isProjectile || projectiles[index].viewIndex < 0)
                {
                    continue;
                }

                Vector3 position = Vector3.Lerp(positions[index].previous, positions[index].current, alpha);
                if (view == null)
                {
                    view = Acquire(context, index, generations[index], projectiles[index].viewIndex, projectilePrefabs, position, rotations[index].current);
                }
                else
                {
                    view.transform.SetPositionAndRotation(position, rotations[index].current);
                }

                if (view.pitchSpeed != 0f && view.pitchPivot != null)
                {
                    view.pitchPivot.Rotate(Vector3.right * view.pitchSpeed * Time.deltaTime, Space.Self);
                }
            }
        }

        private static ProjectileView Acquire(ProjectileViewContext context, int index, int generation, int viewIndex, GameObject[] projectilePrefabs, Vector3 position, Quaternion rotation)
        {
            Stack<ProjectileView> pool = context.viewPools[viewIndex];
            ProjectileView view = pool.Count > 0 ? pool.Pop() : Object.Instantiate(projectilePrefabs[viewIndex]).GetComponent<ProjectileView>();

            view.transform.SetPositionAndRotation(position, rotation);
            view.gameObject.SetActive(true);
            if (view.trail != null)
            {
                view.trail.Clear();
            }

            context.views[index] = view;
            context.viewGenerations[index] = generation;
            context.viewIndices[index] = viewIndex;
            return view;
        }

        private static void Release(ProjectileViewContext context, int index)
        {
            ProjectileView view = context.views[index];
            context.views[index] = null;
            view.gameObject.SetActive(false);
            context.viewPools[context.viewIndices[index]].Push(view);
        }
    }
}
