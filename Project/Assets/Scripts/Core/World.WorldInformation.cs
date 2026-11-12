using System.Collections.Generic;
using TopView.Presentation.Views;
using UnityEngine;

namespace TopView.Core
{
    // 월드 정보 UI: 씬의 앵커 읽기
    public partial class World
    {
        // 씬에 놓인 정보 UI 앵커 중 프리팹이 연결된 것만 고른다. 표시용이라 빠진 앵커는 경고만 남기고 건너뛴다.
        private WorldInformationAnchorView[] ReadWorldInformationAnchors()
        {
            WorldInformationAnchorView[] sceneAnchors = FindObjectsByType<WorldInformationAnchorView>(FindObjectsSortMode.None);
            List<WorldInformationAnchorView> anchors = new List<WorldInformationAnchorView>(sceneAnchors.Length);
            for (int index = 0; index < sceneAnchors.Length; index++)
            {
                WorldInformationAnchorView anchor = sceneAnchors[index];
                if (anchor.prefab == null)
                {
                    Debug.LogWarning($"World >> {anchor.name}의 World Information Anchor View에 Prefab이 연결되어 있지 않아 이 위치에는 정보 UI를 띄우지 않는다.", anchor);
                    continue;
                }
                anchors.Add(anchor);
            }
            return anchors.ToArray();
        }
    }
}
