using System.Collections.Generic;
using TopView.Presentation.Projectile.Views;

namespace TopView.Presentation
{
    // 투사체 View 실행 상태. 판마다 새로 만든다.
    public class ProjectileViewContext
    {
        // 투사체 저장소 칸별 View. 빈 칸은 null
        public ProjectileView[] views;
        // 투사체 저장소 칸별 View가 보여 주는 엔티티의 generation
        public int[] viewGenerations;
        // 투사체 저장소 칸별 View가 속한 투사체 View 표의 번호
        public int[] viewIndices;
        // 투사체 View 표의 번호별로 쉬고 있는 View
        public Stack<ProjectileView>[] viewPools;
    }
}
