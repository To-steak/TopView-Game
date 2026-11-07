using System.Collections.Generic;
using DG.Tweening;
using TopView.Components;
using TopView.Entities;
using TopView.Presentation.Views;
using TopView.Presentation.ViewContexts;
using UnityEngine;
using UnityEngine.UI;

namespace TopView.Presentation.ViewSystems
{
    // 적 View가 있는 칸마다 체력 게이지를 풀에서 꺼내 붙이고, 적 위에 띄워 카메라를 바라보게 하고, 마스크로 가린 게이지를 체력 비율까지 부드럽게 옮기고 체력을 글자로 보여 준다.
    public static class EnemyHealthGaugeViewSystem
    {
        /// <summary>
        /// 저장소의 칸마다 적 View와 게이지를 맞추고, 게이지의 위치·회전·보이는 비율·글자를 적에 맞춘다. EnemyViewSystem.Present 다음에 부른다.
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

                // 막 꺼낸 게이지는 비율이 같아도 글자가 이전 값이므로 체력 표시를 다시 쓴다
                bool isAcquired = gauge == null;
                if (isAcquired)
                {
                    gauge = Acquire(context, index, kind, gaugePrefabsByKind);
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
                    if (isAcquired || context.healthGaugeRatios[index] != healthRatio)
                    {
                        context.healthGaugeRatios[index] = healthRatio;
                        gauge.healthText.text = $"{health.current:F0} / {health.max:F0}";

                        // 막 꺼낸 게이지는 이전 적의 비율이 남아 있으므로 연출 없이 맞춘다
                        if (isAcquired)
                        {
                            context.healthGaugeDisplayedRatios[index] = healthRatio;
                            SetMaskRatio(gauge.healthMask, healthRatio);
                        }
                        context.healthGaugeStartRatios[index] = context.healthGaugeDisplayedRatios[index];
                        context.healthGaugeElapsedSeconds[index] = 0f;
                    }

                    UpdateDisplayedRatio(context, index, gauge, healthRatio);
                }
            }
        }

        // 연출 시간을 이 프레임만큼 늘리고, 시작 비율에서 목표 비율까지 DOTween 기본 이징으로 옮긴 비율을 마스크에 맞춘다.
        private static void UpdateDisplayedRatio(EnemyViewContext context, int index, EnemyHealthGaugeView gauge, float targetRatio)
        {
            float displayedRatio = context.healthGaugeDisplayedRatios[index];
            if (displayedRatio == targetRatio)
            {
                return;
            }

            float durationSeconds = gauge.tweenTicks * Time.fixedDeltaTime;
            float elapsedSeconds = context.healthGaugeElapsedSeconds[index] + Time.deltaTime;
            context.healthGaugeElapsedSeconds[index] = elapsedSeconds;

            displayedRatio = elapsedSeconds < durationSeconds
                ? DOVirtual.EasedValue(context.healthGaugeStartRatios[index], targetRatio, elapsedSeconds / durationSeconds, DOTween.defaultEaseType)
                : targetRatio;
            context.healthGaugeDisplayedRatios[index] = displayedRatio;
            SetMaskRatio(gauge.healthMask, displayedRatio);
        }

        // 마스크의 오른쪽 여백을 비율 밖의 폭으로 맞춘다.
        private static void SetMaskRatio(RectMask2D mask, float ratio)
        {
            Vector4 padding = mask.padding;
            padding.z = mask.rectTransform.rect.width * (1f - ratio);
            mask.padding = padding;
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
            gauge.gameObject.SetActive(false);
            context.healthGaugePools[kind].Push(gauge);
        }
    }
}
