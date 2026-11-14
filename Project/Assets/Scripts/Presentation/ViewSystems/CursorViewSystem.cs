using TopView.Presentation.ViewContexts;
using UnityEngine;

namespace TopView.Presentation.ViewSystems
{
    // 플레이 중에는 OS 커서를 크로스헤어 텍스처로 바꾸고, 일시정지 중이나 판이 끝난 뒤에는 기본 커서로 되돌린다.
    // 커서는 씬이 바뀌어도 남으므로 판을 끝낼 때 Release로 되돌린다.
    public static class CursorViewSystem
    {
        /// <summary>
        /// 크로스헤어를 걸어야 할 상태가 바뀐 프레임에만 커서를 바꾼다. 핫스폿은 텍스처 중앙이다.
        /// </summary>
        /// <param name="isCrosshairHidden">크로스헤어를 내려야 하면(일시정지 중이거나 판이 끝났으면) true. 기본 커서를 쓴다</param>
        /// <param name="crosshairTexture">플레이 중에 쓸 커서 텍스처</param>
        /// <param name="context">커서 연출 실행 상태</param>
        public static void Present(bool isCrosshairHidden, Texture2D crosshairTexture, CursorViewContext context)
        {
            // 텍스처가 없으면 판을 준비할 때 World가 오류를 남기고, 여기서는 기본 커서를 그대로 둔다
            bool isCrosshairShown = !isCrosshairHidden && crosshairTexture != null;
            if (isCrosshairShown == context.isCrosshairApplied)
            {
                return;
            }

            if (isCrosshairShown)
            {
                Vector2 hotspot = new Vector2(crosshairTexture.width * 0.5f, crosshairTexture.height * 0.5f);
                Cursor.SetCursor(crosshairTexture, hotspot, CursorMode.Auto);
            }
            else
            {
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            }

            context.isCrosshairApplied = isCrosshairShown;
        }

        /// <summary>
        /// 걸어 둔 크로스헤어 커서를 기본 커서로 되돌린다. 판을 끝내고 씬을 떠나기 전에 부른다.
        /// </summary>
        /// <param name="context">커서 연출 실행 상태</param>
        public static void Release(CursorViewContext context)
        {
            if (!context.isCrosshairApplied)
            {
                return;
            }

            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            context.isCrosshairApplied = false;
        }
    }
}
