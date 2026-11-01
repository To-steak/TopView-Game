using TopView.Presentation.Player.Views;
using UnityEngine;

namespace TopView.Presentation
{
    // 플레이어 View 실행 상태. 판마다 새로 만든다.
    public class PlayerViewContext
    {
        // 플레이어 View. 아직 만들지 않았으면 null
        public PlayerView view;
        // view의 무기 소켓에 붙인 무기 모델. 번호는 무기 번호와 같다
        public GameObject[] weaponModels;
        // view Animator의 원거리 레이어 번호. 레이어가 없으면 -1
        public int rangedLayerIndex;
        // view Animator의 근접 레이어 번호. 레이어가 없으면 -1
        public int meleeLayerIndex;
        // view Animator의 투척 레이어 번호. 레이어가 없으면 -1
        public int throwLayerIndex;
    }
}
