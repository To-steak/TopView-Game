using System.Collections.Generic;
using TopView.Entities;
using TopView.Presentation.Views;
using TopView.Presentation.ViewContexts;
using UnityEngine;

namespace TopView.Presentation.ViewSystems
{
    // 살아 있는 투사체마다 View를 풀에서 꺼내 붙여 위치를 보간한다.
    public static class ProjectileViewSystem
    {
        /// <summary>
        /// 저장소의 칸마다 살아 있는 투사체와 View를 맞춘다.
        /// </summary>
        /// <param name="entities">투사체 엔티티 배열</param>
        /// <param name="projectilePrefabs">투사체 View 표. 번호별 ProjectileView 프리팹</param>
        /// <param name="isPaused">판이 일시정지 중이면 true. 모델을 회전시키지 않는다</param>
        /// <param name="context">투사체 View 실행 상태</param>
        public static void Present(ProjectileEntity[] entities, GameObject[] projectilePrefabs, bool isPaused, ProjectileViewContext context)
        {
            float alpha = Mathf.Clamp01((Time.time - Time.fixedTime) / Time.fixedDeltaTime);

            for (int index = 0; index < entities.Length; index++)
            {
                ref readonly ProjectileEntity entity = ref entities[index];
                ProjectileView view = context.views[index];

                if (view != null && (!entity.isAlive || context.viewGenerations[index] != entity.generation))
                {
                    Release(context, index);
                    view = null;
                }

                if (!entity.isAlive || entity.projectileView.viewIndex < 0)
                {
                    continue;
                }

                Vector3 position = Vector3.Lerp(entity.position.previous, entity.position.current, alpha);
                if (view == null)
                {
                    view = Acquire(context, index, entity.generation, entity.projectileView.viewIndex, projectilePrefabs, position, entity.rotation.current);
                }
                else
                {
                    view.transform.SetPositionAndRotation(position, entity.rotation.current);
                }

                Vector3 rotationSpeed = new Vector3(view.pitchSpeed, view.yawSpeed, view.rollSpeed);
                if (!isPaused && rotationSpeed != Vector3.zero && view.rotationPivot != null)
                {
                    view.rotationPivot.Rotate(rotationSpeed * Time.deltaTime, Space.Self);
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
