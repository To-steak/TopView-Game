using DG.Tweening;
using TopView.Presentation.ViewContexts;
using UnityEngine;

namespace TopView.Presentation.ViewSystems
{
    // 판이 일시정지되면 주인이 없는 전역 연출(소리, DOTween)을 멈추고, 풀리면 이어서 움직이게 한다.
    // 소리와 DOTween은 Unity가 씬 전체에 한 번에 거는 기능이라 UI 트윈까지 함께 멈춘다. Animator와 파티클은 각 ViewSystem이 자기 것을 멈춘다.
    public static class PauseViewSystem
    {
        /// <summary>
        /// 일시정지 상태가 바뀐 프레임에만 소리와 DOTween을 멈추거나 다시 움직인다.
        /// </summary>
        /// <param name="isPaused">판이 일시정지 중인지</param>
        /// <param name="context">일시정지 연출 실행 상태</param>
        public static void Present(bool isPaused, PauseViewContext context)
        {
            if (isPaused == context.isApplied)
            {
                return;
            }

            AudioListener.pause = isPaused;
            if (isPaused)
            {
                DOTween.PauseAll();
            }
            else
            {
                DOTween.PlayAll();
            }

            context.isApplied = isPaused;
        }

        /// <summary>
        /// 걸어 둔 일시정지를 푼다. 소리와 DOTween은 씬이 바뀌어도 남으므로, 판을 끝내고 씬을 떠나기 전에 부른다.
        /// </summary>
        /// <param name="context">일시정지 연출 실행 상태</param>
        public static void Release(PauseViewContext context)
        {
            if (!context.isApplied)
            {
                return;
            }

            AudioListener.pause = false;
            DOTween.PlayAll();
            context.isApplied = false;
        }
    }
}
