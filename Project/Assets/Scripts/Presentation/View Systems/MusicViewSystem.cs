using DG.Tweening;
using TopView.Entities;
using TopView.Presentation.ViewContexts;
using UnityEngine;

namespace TopView.Presentation.ViewSystems
{
    // 배경음악을 처리한다. 처음에는 평소 곡을 처음부터 페이드 인해 반복하고, 판에서 화면 체력바를 띄우는 적(보스)이 처음 나타나면 보스 곡으로 크로스페이드한다.
    public static class MusicViewSystem
    {
        /// <summary>
        /// 이번 판에 아직 곡을 틀지 않았으면 평소 곡을 처음부터 페이드 인한다. 이미 틀었으면 아무것도 하지 않는다. 적이 없는 씬에서 쓴다.
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

        /// <summary>
        /// 평소 곡을 틀고, 살아 있는 보스가 처음 보이면 보스 곡으로 크로스페이드한다. 보스 곡으로 바꾼 뒤에는 보스가 죽어도 판이 끝날 때까지 보스 곡을 유지한다.
        /// </summary>
        /// <param name="enemies">적 엔티티 배열</param>
        /// <param name="isBossByKind">적 종류별로 보스인지. 오버레이 체력 바에 체력을 보여 주는 종류가 보스다</param>
        /// <param name="context">배경음악 실행 상태</param>
        public static void Present(EnemyEntity[] enemies, bool[] isBossByKind, MusicViewContext context)
        {
            Present(context);

            if (context.hasSwitchedToBoss)
            {
                // 다 줄어든 평소 곡은 멈춰 둔다
                if (context.normalSource.isPlaying && context.normalSource.volume <= 0f)
                {
                    context.normalSource.Stop();
                }
                return;
            }

            // 보스 곡이 없는 경우는 판을 준비할 때 로그를 남겼다
            if (context.bossSource == null || !HasAliveBoss(enemies, isBossByKind))
            {
                return;
            }

            float fadeSeconds = context.fadeTicks * Time.fixedDeltaTime;
            FadeOut(context.normalSource, fadeSeconds);
            FadeIn(context.bossSource, context.volume, fadeSeconds);
            context.hasSwitchedToBoss = true;
        }

        private static bool HasAliveBoss(EnemyEntity[] enemies, bool[] isBossByKind)
        {
            for (int index = 0; index < enemies.Length; index++)
            {
                ref readonly EnemyEntity entity = ref enemies[index];
                if (entity.isAlive && isBossByKind[entity.kind.kind])
                {
                    return true;
                }
            }

            return false;
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

        // 볼륨을 0까지 줄인다. 멈추는 일은 Present가 볼륨이 0이 된 것을 보고 한다.
        private static void FadeOut(AudioSource source, float fadeSeconds)
        {
            source.DOKill();

            if (fadeSeconds <= 0f)
            {
                source.volume = 0f;
                return;
            }

            source.DOFade(0f, fadeSeconds).SetLink(source.gameObject);
        }
    }
}
