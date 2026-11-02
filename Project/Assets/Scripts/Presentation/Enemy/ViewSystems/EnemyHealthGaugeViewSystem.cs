using System.Collections.Generic;
using TopView.Entities;
using TopView.Presentation.Enemy.Views;
using UnityEngine;

namespace TopView.Presentation.Enemy.ViewSystems
{
    // 적 View가 있는 칸마다 체력 게이지를 풀에서 꺼내 붙이고, 적 위에 띄워 카메라를 바라보게 하고 체력 비율만큼 너비를 맞추고 체력을 글자로 보여 준다.
    public static class EnemyHealthGaugeViewSystem
    {
        /// <summary>
        /// 적 저장소의 칸마다 적 View와 게이지를 맞추고, 게이지의 위치·회전·너비·글자를 적에 맞춘다. EnemyViewSystem.Present 다음에 부른다.
        /// </summary>
        /// <param name="enemies">적 저장소</param>
        /// <param name="gaugePrefabsByKind">적 종류별 EnemyHealthGaugeView 프리팹</param>
        /// <param name="offsetsByKind">적 종류별로 게이지를 적 위치에서 띄울 거리(월드)</param>
        /// <param name="colorsByKind">적 종류별 게이지 색</param>
        /// <param name="mainCamera">게이지가 바라볼 카메라. 없으면 null</param>
        /// <param name="context">적 View 실행 상태</param>
        public static void Present(EnemyStorage enemies, GameObject[] gaugePrefabsByKind, Vector3[] offsetsByKind, Color[] colorsByKind, UnityEngine.Camera mainCamera, EnemyViewContext context)
        {
            for (int enemyIndex = 0; enemyIndex < enemies.entities.Length; enemyIndex++)
            {
                EnemyView enemyView = context.views[enemyIndex];
                EnemyHealthGaugeView gauge = context.healthGaugeViews[enemyIndex];
                // 적 저장소는 종류별 구간으로 나뉘어 있어 칸 번호로 종류를 안다
                int kind = enemyIndex / enemies.capacityPerKind;

                // 게이지 프리팹이 없는 종류는 판을 준비할 때 경고를 남겼으므로 여기서는 건너뛴다
                if (enemyView == null || gaugePrefabsByKind[kind] == null)
                {
                    if (gauge != null)
                    {
                        Release(context, enemyIndex, kind);
                    }
                    continue;
                }

                // 막 꺼낸 게이지는 비율이 같아도 글자가 이전 값이므로 체력 표시를 다시 쓴다
                bool isAcquired = gauge == null;
                if (isAcquired)
                {
                    gauge = Acquire(context, enemyIndex, kind, gaugePrefabsByKind, colorsByKind);
                }

                Vector3 position = enemyView.transform.position + offsetsByKind[kind];
                Quaternion rotation = mainCamera != null
                    ? Quaternion.LookRotation(position - mainCamera.transform.position)
                    : gauge.transform.rotation;
                gauge.transform.SetPositionAndRotation(position, rotation);

                ref readonly EnemyEntity enemy = ref enemies.entities[enemyIndex];
                if (enemy.maxHealth > 0f)
                {
                    float healthRatio = enemy.health / enemy.maxHealth;
                    RectTransform gaugeFrontTransform = gauge.gaugeFront.rectTransform;
                    if (isAcquired || gaugeFrontTransform.anchorMax.x != healthRatio)
                    {
                        gaugeFrontTransform.anchorMax = new Vector2(healthRatio, 1f);
                        gauge.healthText.text = $"{enemy.health} / {enemy.maxHealth}";
                    }
                }
            }
        }

        private static EnemyHealthGaugeView Acquire(EnemyViewContext context, int enemyIndex, int kind, GameObject[] gaugePrefabsByKind, Color[] colorsByKind)
        {
            Stack<EnemyHealthGaugeView> pool = context.healthGaugePools[kind];
            EnemyHealthGaugeView gauge = pool.Count > 0 ? pool.Pop() : Object.Instantiate(gaugePrefabsByKind[kind], context.healthGaugeParent, false).GetComponent<EnemyHealthGaugeView>();
            gauge.gaugeFront.color = colorsByKind[kind];
            gauge.gameObject.SetActive(true);
            context.healthGaugeViews[enemyIndex] = gauge;
            return gauge;
        }

        private static void Release(EnemyViewContext context, int enemyIndex, int kind)
        {
            EnemyHealthGaugeView gauge = context.healthGaugeViews[enemyIndex];
            context.healthGaugeViews[enemyIndex] = null;
            gauge.gameObject.SetActive(false);
            context.healthGaugePools[kind].Push(gauge);
        }
    }
}
