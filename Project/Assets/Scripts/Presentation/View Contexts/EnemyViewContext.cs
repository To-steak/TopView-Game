using System.Collections.Generic;
using TopView.Presentation.Views;
using UnityEngine;

namespace TopView.Presentation.ViewContexts
{
    // 적 View와 체력 게이지 실행 상태. 판마다 새로 만든다.
    public class EnemyViewContext
    {
        // 엔티티 저장소 칸별 적 View. 빈 칸은 null
        public EnemyView[] views;
        // 엔티티 저장소 칸별 적 View가 보여 주는 엔티티의 generation
        public int[] viewGenerations;
        // 적 종류별로 쉬고 있는 View
        public Stack<EnemyView>[] viewPools;
        // 적 종류별 Animator 트리거 파라미터. 그 종류의 View를 처음 만들 때 채운다
        public int[][] triggerHashesByKind;
        // 엔티티 저장소 칸별 적 체력 게이지. 빈 칸은 null
        public EnemyHealthGaugeView[] healthGaugeViews;
        // 엔티티 저장소 칸별 적 체력 게이지가 보여 주는 엔티티의 generation
        public int[] healthGaugeGenerations;
        // 엔티티 저장소 칸별 적 체력 게이지가 마지막으로 맞춘 체력 비율. 연출 중에도 목표값을 가리킨다
        public float[] healthGaugeRatios;
        // 적 종류별로 쉬고 있는 체력 게이지
        public Stack<EnemyHealthGaugeView>[] healthGaugePools;
        // 체력 게이지를 만들 때 부모로 쓸 월드 공간 Canvas. 씬에 없으면 null
        public Transform healthGaugeParent;
    }
}
