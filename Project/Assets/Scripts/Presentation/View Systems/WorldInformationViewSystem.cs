using TopView.Presentation.ViewContexts;
using TopView.Presentation.Views;
using UnityEngine;

namespace TopView.Presentation.ViewSystems
{
    // 앵커마다 정보 UI를 한 번 만들어 앵커 위치에 두고, 매 프레임 카메라를 바라보게 한다.
    public static class WorldInformationViewSystem
    {
        /// <summary>
        /// 아직 만들지 않은 앵커의 정보 UI를 만들어 앵커 위치에 두고, 모든 정보 UI가 카메라를 바라보게 한다.
        /// </summary>
        /// <param name="mainCamera">정보 UI가 바라볼 카메라. 없으면 null</param>
        /// <param name="context">월드 정보 UI 실행 상태</param>
        public static void Present(Camera mainCamera, WorldInformationViewContext context)
        {
            for (int index = 0; index < context.anchors.Length; index++)
            {
                WorldInformationView view = context.views[index];
                if (view == null)
                {
                    view = Create(context, index);
                }

                if (mainCamera != null)
                {
                    view.transform.rotation = Quaternion.LookRotation(view.transform.position - mainCamera.transform.position);
                }
            }
        }

        // 앵커는 움직이지 않으므로 위치는 만들 때 한 번만 맞춘다
        private static WorldInformationView Create(WorldInformationViewContext context, int anchorIndex)
        {
            WorldInformationAnchorView anchor = context.anchors[anchorIndex];
            WorldInformationView view = Object.Instantiate(anchor.prefab, context.parent, false);
            view.transform.position = anchor.transform.position;
            context.views[anchorIndex] = view;
            return view;
        }
    }
}
