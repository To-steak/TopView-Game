using TopView.Presentation.Views;
using UnityEngine;

namespace TopView.Presentation.ViewContexts
{
    // 월드 정보 UI 실행 상태. 판마다 새로 만든다.
    public class WorldInformationViewContext
    {
        // 씬에 놓인 앵커 중 프리팹이 올바르게 연결된 것
        public WorldInformationAnchorView[] anchors;
        // anchors와 같은 순서로, 각 앵커에 띄운 정보 UI. 아직 만들지 않았으면 null
        public WorldInformationView[] views;
        // 정보 UI를 만들 때 부모로 쓸 월드 공간 Canvas. 씬에 없으면 null
        public Transform parent;
    }
}
