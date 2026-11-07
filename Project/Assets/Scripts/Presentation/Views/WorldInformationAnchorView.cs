using UnityEngine;

namespace TopView.Presentation.Views
{
    // 씬에 놓은 빈 오브젝트에 붙여 그 위치에 정보 UI를 띄운다. 움직이지 않는 오브젝트에 붙인다.
    public class WorldInformationAnchorView : MonoBehaviour
    {
        // 이 위치에 띄울 정보 UI 프리팹
        public WorldInformationView prefab;
    }
}
