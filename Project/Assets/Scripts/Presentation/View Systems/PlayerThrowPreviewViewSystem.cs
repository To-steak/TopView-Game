using TopView.Components;
using TopView.Entities;
using TopView.Presentation.Views;
using UnityEngine;

namespace TopView.Presentation.ViewSystems
{
    // 투척 충전 중인 플레이어의 궤도와 폭발 범위를 PlayerView의 선과 원에 그린다. 계산은 Logic이 하고, 여기서는 받은 값을 옮기기만 한다.
    public static class PlayerThrowPreviewViewSystem
    {
        /// <summary>
        /// 살아 있는 플레이어의 미리보기가 보일 때면 선과 원을 맞추고, 아니면 끈다. PlayerViewSystem 뒤에 부른다.
        /// </summary>
        /// <param name="entities">플레이어 엔티티 배열</param>
        /// <param name="view">플레이어 View. 아직 만들지 않았으면 null</param>
        /// <param name="circleHeightOffset">원이 바닥과 겹쳐 깜빡이지 않도록 폭발 위치보다 띄우는 높이(유닛)</param>
        public static void Present(Entity[] entities, PlayerView view, float circleHeightOffset)
        {
            if (view == null)
            {
                return;
            }

            bool isVisible = TryFindAlivePlayer(entities, out int playerIndex) && entities[playerIndex].throwPreview.isVisible;
            if (view.throwPreviewLine != null)
            {
                SetActive(view.throwPreviewLine.gameObject, isVisible);
                if (isVisible)
                {
                    ApplyLine(in entities[playerIndex].throwPreview, view.throwPreviewLine);
                }
            }
            if (view.throwPreviewCircle != null)
            {
                SetActive(view.throwPreviewCircle.gameObject, isVisible);
                if (isVisible)
                {
                    ApplyCircle(in entities[playerIndex].throwPreview, view.throwPreviewCircle, circleHeightOffset);
                }
            }
        }

        private static bool TryFindAlivePlayer(Entity[] entities, out int playerIndex)
        {
            for (playerIndex = 0; playerIndex < entities.Length; playerIndex++)
            {
                if ((entities[playerIndex].mask & ComponentMask.ThrowPreview) != 0)
                {
                    return true;
                }
            }

            playerIndex = -1;
            return false;
        }

        private static void SetActive(GameObject target, bool isActive)
        {
            if (target.activeSelf != isActive)
            {
                target.SetActive(isActive);
            }
        }

        private static void ApplyLine(in PlayerThrowPreviewComponent preview, LineRenderer line)
        {
            line.positionCount = preview.pointCount;
            for (int pointIndex = 0; pointIndex < preview.pointCount; pointIndex++)
            {
                line.SetPosition(pointIndex, preview.points[pointIndex]);
            }
        }

        // 폭발 위치에 두고, 지름이 폭발 반지름의 두 배가 되게 키운다. circle은 눕힌 Quad를 자식으로 둔 세워진 Root라 로컬 X, Z가 바닥의 지름이다.
        private static void ApplyCircle(in PlayerThrowPreviewComponent preview, Transform circle, float circleHeightOffset)
        {
            float diameter = preview.explosionRadius * 2f;
            circle.position = preview.impactPosition + Vector3.up * circleHeightOffset;
            circle.localScale = new Vector3(diameter, 1f, diameter);
        }
    }
}
