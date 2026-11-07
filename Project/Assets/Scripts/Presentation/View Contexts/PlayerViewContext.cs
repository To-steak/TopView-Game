using TopView.Presentation.Views;
using UnityEngine;

namespace TopView.Presentation.ViewContexts
{
    // 플레이어 View 실행 상태. 판마다 새로 만든다.
    public class PlayerViewContext
    {
        // 플레이어 View. 아직 만들지 않았으면 null
        public PlayerView view;
        // view의 무기 소켓에 붙인 무기 모델. 번호는 무기 번호와 같다
        public GameObject[] weaponModels;
        // weaponModels에 붙은 WeaponView. 번호는 무기 번호와 같고, 모델이나 WeaponView가 없는 칸은 null
        public WeaponView[] weaponViews;
        // view Animator의 원거리 레이어 번호. 레이어가 없으면 -1
        public int rangedLayerIndex;
        // view Animator의 행동 레이어(Swing, Throw, Swap) 번호. 레이어가 없으면 -1
        public int actionLayerIndex;
    }
}
