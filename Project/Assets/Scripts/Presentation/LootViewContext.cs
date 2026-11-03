using System.Collections.Generic;
using TopView.Presentation.Loot.Views;

namespace TopView.Presentation
{
    // 전리품 View 실행 상태. 판마다 새로 만든다.
    public class LootViewContext
    {
        // 전리품 저장소 칸별 View. 빈 칸은 null
        public LootView[] views;
        // 전리품 저장소 칸별 View가 보여 주는 엔티티의 generation
        public int[] viewGenerations;
        // 전리품 저장소 칸별 View의 전리품 종류 번호
        public int[] viewKinds;
        // 전리품 종류 번호별로 쉬고 있는 View
        public Stack<LootView>[] viewPools;
    }
}
