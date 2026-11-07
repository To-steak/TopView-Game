using DG.Tweening;
using TopView.Presentation.ViewContexts;
using UnityEngine;

namespace TopView.Presentation.ViewSystems
{
    // 배경음악을 처리한다. 판이 시작되면 평소 곡을 처음부터 페이드 인해 반복한다.
    public static class MusicViewSystem
    {
        /// <summary>
        /// 이번 판에 아직 곡을 틀지 않았으면 평소 곡을 처음부터 페이드 인한다. 이미 틀었으면 아무것도 하지 않는다.
        /// </summary>
        /// <param name="context">배경음악 실행 상태</param>
        public static void Present(MusicViewContext context)
        {
            if (context.hasStarted)
            {
                return;
            }

            FadeIn(context.normalSource, context.volume, context.fadeTicks * Time.fixedDeltaTime);
            context.hasStarted = true;
        }

        private static void FadeIn(AudioSource source, float volume, float fadeSeconds)
        {
            source.DOKill();
            source.volume = 0f;
            source.Play();

            if (fadeSeconds <= 0f)
            {
                source.volume = volume;
                return;
            }

            source.DOFade(volume, fadeSeconds).SetLink(source.gameObject);
        }
    }
}
