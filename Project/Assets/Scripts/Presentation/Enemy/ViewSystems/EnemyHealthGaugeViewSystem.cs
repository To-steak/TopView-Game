using System.Collections.Generic;
using TopView.Entities;
using TopView.Presentation.Enemy.Views;
using UnityEngine;

namespace TopView.Presentation.Enemy.ViewSystems
{
    // 적 View가 있는 칸마다 체력 게이지를 풀에서 꺼내 붙이고, 적 위에 띄워 카메라를 바라보게 하고 체력만큼 채운다.
    public static class EnemyHealthGaugeViewSystem
    {
        /// <summary>
        /// 적 저장소의 칸마다 적 View와 게이지를 맞추고, 게이지의 위치·회전·채움을 적에 맞춘다. EnemyViewSystem.Present 다음에 부른다.
        /// </summary>
        /// <param name="enemies">적 저장소</param>
        /// <param name="gaugePrefabsByKind">적 종류별 EnemyHealthGaugeView 프리팹</param>
        /// <param name="offsetsByKind">적 종류별로 게이지를 적 위치에서 띄울 거리(월드)</param>
        /// <param name="colorsByKind">적 종류별 게이지 색</param>
        /// <param name="context">View 실행 상태</param>
        public static void Present(EnemyStorage enemies, GameObject[] gaugePrefabsByKind, Vector3[] offsetsByKind, Color[] colorsByKind, ViewContext context)
        {
            for (int enemyIndex = 0; enemyIndex < enemies.entities.Length; enemyIndex++)
            {
                EnemyView enemyView = context.enemyViews[enemyIndex];
                EnemyHealthGaugeView gauge = context.enemyHealthGaugeViews[enemyIndex];
                // 적 저장소는 종류별 구간으로 나뉘어 있어 칸 번호로 종류를 안다
                int kind = enemyIndex / enemies.capacityPerKind;

                if (enemyView == null)
                {
                    if (gauge != null)
                    {
                        Release(context, enemyIndex, kind);
                    }
                    continue;
                }

                if (gauge == null)
                {
                    gauge = Acquire(context, enemyIndex, kind, gaugePrefabsByKind, colorsByKind);
                }

                Vector3 position = enemyView.transform.position + offsetsByKind[kind];
                Quaternion rotation = context.mainCamera != null
                    ? Quaternion.LookRotation(position - context.mainCamera.transform.position)
                    : gauge.transform.rotation;
                gauge.transform.SetPositionAndRotation(position, rotation);

                ref readonly EnemyEntity enemy = ref enemies.entities[enemyIndex];
                if (enemy.maxHealth > 0f)
                {
                    float fill = enemy.health / enemy.maxHealth;
                    if (gauge.gaugeFront.fillAmount != fill)
                    {
                        gauge.gaugeFront.fillAmount = fill;
                    }
                }
            }
        }

        private static EnemyHealthGaugeView Acquire(ViewContext context, int enemyIndex, int kind, GameObject[] gaugePrefabsByKind, Color[] colorsByKind)
        {
            Stack<EnemyHealthGaugeView> pool = context.enemyHealthGaugePools[kind];
            EnemyHealthGaugeView gauge = pool.Count > 0 ? pool.Pop() : Object.Instantiate(gaugePrefabsByKind[kind]).GetComponent<EnemyHealthGaugeView>();
            gauge.gaugeFront.color = colorsByKind[kind];
            gauge.gameObject.SetActive(true);
            context.enemyHealthGaugeViews[enemyIndex] = gauge;
            return gauge;
        }

        private static void Release(ViewContext context, int enemyIndex, int kind)
        {
            EnemyHealthGaugeView gauge = context.enemyHealthGaugeViews[enemyIndex];
            context.enemyHealthGaugeViews[enemyIndex] = null;
            gauge.gameObject.SetActive(false);
            context.enemyHealthGaugePools[kind].Push(gauge);
        }
    }
}
