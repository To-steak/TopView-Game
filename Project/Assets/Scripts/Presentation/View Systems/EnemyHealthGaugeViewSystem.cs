using System.Collections.Generic;
using DG.Tweening;
using TopView.Components;
using TopView.Entities;
using TopView.Presentation.Views;
using TopView.Presentation.ViewContexts;
using UnityEngine;

namespace TopView.Presentation.ViewSystems
{
    // 적 View가 있는 칸마다 체력 게이지를 풀에서 꺼내 붙이고, 적 위에 띄워 카메라를 바라보게 하고 체력 비율까지 너비를 부드럽게 옮기고 체력과 레벨을 글자로 보여 준다.
    public static class EnemyHealthGaugeViewSystem
    {
        /// <summary>
        /// 저장소의 칸마다 적 View와 게이지를 맞추고, 게이지의 위치·회전·너비·글자를 적에 맞춘다. EnemyViewSystem.Present 다음에 부른다.
        /// </summary>
        /// <param name="entities">적 엔티티 배열</param>
        /// <param name="gaugePrefabsByKind">적 종류별 EnemyHealthGaugeView 프리팹</param>
        /// <param name="mainCamera">게이지가 바라볼 카메라. 없으면 null</param>
        /// <param name="context">적 View 실행 상태</param>
        public static void Present(EnemyEntity[] entities, GameObject[] gaugePrefabsByKind, Camera mainCamera, EnemyViewContext context)
        {
            for (int index = 0; index < context.views.Length; index++)
            {
                EnemyView enemyView = context.views[index];
                EnemyHealthGaugeView gauge = context.healthGaugeViews[index];
                if (enemyView == null && gauge == null)
                {
                    continue;
                }

                // 칸은 종류별 구간으로 나뉘어 있어 칸을 다시 채워도 종류 번호는 그대로다
                int kind = entities[index].kind.kind;

                // 게이지 프리팹이 없는 종류는 판을 준비할 때 경고를 남겼으므로 여기서는 건너뛴다
                if (enemyView == null || gaugePrefabsByKind[kind] == null)
                {
                    if (gauge != null)
                    {
                        Release(context, index, kind);
                    }
                    continue;
                }

                // 막 꺼냈거나 칸이 다시 채워져 다른 적을 보여 주게 된 게이지는 너비와 글자가 이전 적의 값이므로 다시 맞춘다
                bool isNewEnemy = gauge == null || context.healthGaugeGenerations[index] != entities[index].generation;
                if (gauge == null)
                {
                    gauge = Acquire(context, index, kind, gaugePrefabsByKind);
                }
                context.healthGaugeGenerations[index] = entities[index].generation;

                // 적의 레벨은 생성 때 정해져 바뀌지 않으므로 새 적일 때만 맞춘다
                if (isNewEnemy)
                {
                    gauge.levelText.text = $"Lv. {entities[index].combat.level:F0}";
                }

                Vector3 position = enemyView.healthGaugeAnchor.position;
                Quaternion rotation = mainCamera != null
                    ? Quaternion.LookRotation(position - mainCamera.transform.position)
                    : gauge.transform.rotation;
                gauge.transform.SetPositionAndRotation(position, rotation);

                ref readonly HealthComponent health = ref entities[index].health;
                if (health.max > 0f)
                {
                    float healthRatio = health.current / health.max;
                    if (isNewEnemy || context.healthGaugeRatios[index] != healthRatio)
                    {
                        context.healthGaugeRatios[index] = healthRatio;
                        gauge.healthText.text = $"{health.current:F0} / {health.max:F0}";

                        RectTransform gaugeFrontTransform = gauge.gaugeFront.rectTransform;
                        Vector2 anchorMax = new Vector2(healthRatio, 1f);
                        gaugeFrontTransform.DOKill();
                        // 새 적은 이전 적의 너비에서 연출하지 않고 바로 맞춘다
                        if (isNewEnemy)
                        {
                            gaugeFrontTransform.anchorMax = anchorMax;
                        }
                        else
                        {
                            gaugeFrontTransform.DOAnchorMax(anchorMax, gauge.tweenTicks * Time.fixedDeltaTime).SetLink(gauge.gameObject);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 게이지가 표시에 쓰는 칸이 모두 연결되어 있는지 본다. 로그는 남기지 않으며, World가 판을 준비할 때 이 결과로 빠진 칸을 알린다.
        /// </summary>
        /// <param name="gauge">검사할 EnemyHealthGaugeView</param>
        /// <returns>Gauge Front, Health Text, Level Text가 모두 연결되어 있으면 true</returns>
        public static bool HasAllReferences(EnemyHealthGaugeView gauge)
        {
            return gauge.gaugeFront != null && gauge.healthText != null && gauge.levelText != null;
        }

        private static EnemyHealthGaugeView Acquire(EnemyViewContext context, int enemyIndex, int kind, GameObject[] gaugePrefabsByKind)
        {
            Stack<EnemyHealthGaugeView> pool = context.healthGaugePools[kind];
            EnemyHealthGaugeView gauge = pool.Count > 0 ? pool.Pop() : Object.Instantiate(gaugePrefabsByKind[kind], context.healthGaugeParent, false).GetComponent<EnemyHealthGaugeView>();
            gauge.gameObject.SetActive(true);
            context.healthGaugeViews[enemyIndex] = gauge;
            return gauge;
        }

        private static void Release(EnemyViewContext context, int enemyIndex, int kind)
        {
            EnemyHealthGaugeView gauge = context.healthGaugeViews[enemyIndex];
            context.healthGaugeViews[enemyIndex] = null;
            gauge.gaugeFront.rectTransform.DOKill();
            gauge.gameObject.SetActive(false);
            context.healthGaugePools[kind].Push(gauge);
        }
    }
}
