using System.Collections.Generic;
using TopView.Presentation.Enemy.Views;
using UnityEngine;

namespace TopView.Presentation
{
    // 적 View와 체력 게이지 실행 상태. 판마다 새로 만든다.
    public class EnemyViewContext
    {
        // 적 저장소 칸별 View. 빈 칸은 null
        public EnemyView[] views;
        // 적 저장소 칸별 View가 보여 주는 엔티티의 generation
        public int[] viewGenerations;
        // 적 종류별로 쉬고 있는 View
        public Stack<EnemyView>[] viewPools;
        // 적 종류별 Animator 트리거 파라미터. 그 종류의 View를 처음 만들 때 채운다
        public int[][] triggerHashesByKind;
        // 적 저장소 칸별 체력 게이지. 빈 칸은 null
        public EnemyHealthGaugeView[] healthGaugeViews;
        // 적 종류별로 쉬고 있는 체력 게이지
        public Stack<EnemyHealthGaugeView>[] healthGaugePools;
        // 체력 게이지를 만들 때 부모로 쓸 월드 공간 Canvas. 씬에 없으면 null
        public Transform healthGaugeParent;
    }
}
