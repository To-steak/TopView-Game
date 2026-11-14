using System.Collections.Generic;
using TopView.Entities;
using TopView.Presentation.Views;
using TopView.Presentation.ViewContexts;
using UnityEngine;

namespace TopView.Presentation.ViewSystems
{
    // 살아 있는 전리품마다 View를 풀에서 꺼내 그 위치에 두고 모델을 회전시킨다. 전리품은 움직이지 않으므로 꺼낼 때 한 번만 옮긴다.
    public static class LootViewSystem
    {
        /// <summary>
        /// 저장소의 칸마다 Loot를 가진 엔티티와 View를 맞춘다.
        /// </summary>
        /// <param name="entities">엔티티 배열. Loot를 가진 칸만 그린다</param>
        /// <param name="lootPrefabs">전리품 종류 번호별 LootView 프리팹. 없는 종류는 null</param>
        /// <param name="isPaused">판이 일시정지 중이면 true. 모델을 회전시키지 않는다</param>
        /// <param name="context">전리품 View 실행 상태</param>
        public static void Present(Entity[] entities, GameObject[] lootPrefabs, bool isPaused, LootViewContext context)
        {
            for (int index = 0; index < entities.Length; index++)
            {
                ref readonly Entity entity = ref entities[index];
                LootView view = context.views[index];
                bool hasLoot = (entity.mask & ComponentMask.Loot) != 0;

                if (view != null && (!hasLoot || context.viewGenerations[index] != entity.generation))
                {
                    Release(context, index);
                    view = null;
                }

                if (!hasLoot)
                {
                    continue;
                }

                if (view == null)
                {
                    int kind = (int)entity.loot.kind;
                    if (lootPrefabs[kind] == null)
                    {
                        continue;
                    }

                    view = Acquire(context, index, entity.generation, kind, lootPrefabs, entity.position.current);
                }

                Vector3 rotationSpeed = new Vector3(view.pitchSpeed, view.yawSpeed, view.rollSpeed);
                if (!isPaused && rotationSpeed != Vector3.zero && view.rotationPivot != null)
                {
                    view.rotationPivot.Rotate(rotationSpeed * Time.deltaTime, Space.Self);
                }
            }
        }

        private static LootView Acquire(LootViewContext context, int index, int generation, int kind, GameObject[] lootPrefabs, Vector3 position)
        {
            Stack<LootView> pool = context.viewPools[kind];
            LootView view = pool.Count > 0 ? pool.Pop() : Object.Instantiate(lootPrefabs[kind]).GetComponent<LootView>();

            view.transform.position = position;
            view.gameObject.SetActive(true);

            context.views[index] = view;
            context.viewGenerations[index] = generation;
            context.viewKinds[index] = kind;
            return view;
        }

        private static void Release(LootViewContext context, int index)
        {
            LootView view = context.views[index];
            context.views[index] = null;
            view.gameObject.SetActive(false);
            context.viewPools[context.viewKinds[index]].Push(view);
        }
    }
}
