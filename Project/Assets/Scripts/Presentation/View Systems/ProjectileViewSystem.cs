using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Presentation.Views;
using TopView.Presentation.ViewContexts;
using UnityEngine;

namespace TopView.Presentation.ViewSystems
{
    // 살아 있는 투사체마다 View를 풀에서 꺼내 붙여 위치를 보간한다.
    public static class ProjectileViewSystem
    {
        // 그릴 투사체가 가진 컴포넌트
        private const ComponentMask PROJECTILE_MASK = ComponentMask.Position | ComponentMask.Rotation | ComponentMask.ProjectileView;

        /// <summary>
        /// 저장소의 칸마다 ProjectileView를 가진 투사체와 View를 맞춘다.
        /// </summary>
        /// <param name="entities">엔티티 배열. Position, Rotation, ProjectileView를 가진 칸만 그린다</param>
        /// <param name="projectilePrefabs">투사체 View 표. 번호별 ProjectileView 프리팹</param>
        /// <param name="isPaused">판이 일시정지 중이면 true. 모델을 회전시키지 않는다</param>
        /// <param name="context">투사체 View 실행 상태</param>
        public static void Present(Entity[] entities, GameObject[] projectilePrefabs, bool isPaused, ProjectileViewContext context)
        {
            float alpha = GetInterpolationAlpha();
            for (int index = 0; index < entities.Length; index++)
            {
                ref readonly Entity entity = ref entities[index];
                bool hasProjectile = (entity.mask & PROJECTILE_MASK) == PROJECTILE_MASK;
                PresentSlot(context, index, hasProjectile, entity.generation, in entity.projectileView, in entity.position, in entity.rotation, projectilePrefabs, isPaused, alpha);
            }
        }

        private static float GetInterpolationAlpha()
        {
            return Mathf.Clamp01((Time.time - Time.fixedTime) / Time.fixedDeltaTime);
        }

        // 칸 하나의 View를 엔티티와 맞춘다. 엔티티가 없거나 바뀌었으면 View를 돌려놓고, 살아 있으면 꺼내 붙여 위치를 보간한다.
        private static void PresentSlot(ProjectileViewContext context, int index, bool isAlive, int generation, in ProjectileViewComponent projectileView, in PositionComponent position, in RotationComponent rotation, GameObject[] projectilePrefabs, bool isPaused, float alpha)
        {
            ProjectileView view = context.views[index];
            if (view != null && (!isAlive || context.viewGenerations[index] != generation))
            {
                Release(context, index);
                view = null;
            }

            if (!isAlive || projectileView.viewIndex < 0)
            {
                return;
            }

            Vector3 interpolatedPosition = Vector3.Lerp(position.previous, position.current, alpha);
            if (view == null)
            {
                view = Acquire(context, index, generation, projectileView.viewIndex, projectilePrefabs, interpolatedPosition, rotation.current);
            }
            else
            {
                view.transform.SetPositionAndRotation(interpolatedPosition, rotation.current);
            }

            Vector3 rotationSpeed = new Vector3(view.pitchSpeed, view.yawSpeed, view.rollSpeed);
            if (!isPaused && rotationSpeed != Vector3.zero && view.rotationPivot != null)
            {
                view.rotationPivot.Rotate(rotationSpeed * Time.deltaTime, Space.Self);
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
